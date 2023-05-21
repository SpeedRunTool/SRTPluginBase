using Microsoft.Data.Sqlite;
using System.Data;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace SRTPluginBase
{
    public abstract class PluginBase<T> : IPlugin where T : class, IPlugin
	{
        private readonly SqliteConnection sqliteConnection;

        public PluginBase()
        {
            SqliteConnectionStringBuilder sqliteConnectionStringBuilder = new SqliteConnectionStringBuilder()
            {
                DataSource = Path.Combine(".plugindb", $"{typeof(T).Name}.sqlite"),
                Mode = SqliteOpenMode.ReadWriteCreate
			};
			sqliteConnection = new SqliteConnection(sqliteConnectionStringBuilder.ToString());
            sqliteConnection.Open();
		}

		public abstract IPluginInfo Info { get; }

		public string GetConfigFile(Assembly a) => a.GetConfigFile();

		public virtual T LoadConfiguration() => Extensions.LoadConfiguration<T>(null);
        public T LoadConfiguration(string? configFile = null) => Extensions.LoadConfiguration<T>(configFile);

        public virtual void SaveConfiguration(T configuration) => configuration.SaveConfiguration(null);
        public void SaveConfiguration(T configuration, string? configFile = null) => configuration.SaveConfiguration(configFile);

		public virtual async Task<int> DbNonQuery(string query, IDbTransaction? dbTransaction, CancellationToken cancellationToken, params IDbDataParameter[] dbDataParameters)
		{
			using (SqliteCommand sqliteCommand = new SqliteCommand(query, sqliteConnection, dbTransaction as SqliteTransaction))
			{
				if (dbDataParameters is not null)
					sqliteCommand.Parameters.AddRange(dbDataParameters);

				return await sqliteCommand.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
			}
		}

		public virtual async Task<object?> DbScalar(string query, IDbTransaction? dbTransaction, CancellationToken cancellationToken, params IDbDataParameter[] dbDataParameters)
		{
			using (SqliteCommand sqliteCommand = new SqliteCommand(query, sqliteConnection, dbTransaction as SqliteTransaction))
			{
				if (dbDataParameters is not null)
					sqliteCommand.Parameters.AddRange(dbDataParameters);

				return await sqliteCommand.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
			}
		}

		public virtual async Task<IDataReader?> DbReader(string query, IDbTransaction? dbTransaction, CancellationToken cancellationToken, CommandBehavior commandBehavior = CommandBehavior.Default, params IDbDataParameter[] dbDataParameters)
		{
			using (SqliteCommand sqliteCommand = new SqliteCommand(query, sqliteConnection, dbTransaction as SqliteTransaction))
			{
				if (dbDataParameters is not null)
					sqliteCommand.Parameters.AddRange(dbDataParameters);

				return await sqliteCommand.ExecuteReaderAsync(commandBehavior, cancellationToken).ConfigureAwait(false);
			}
		}

		public abstract ValueTask DisposeAsync();

        public abstract void Dispose();

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                sqliteConnection.Dispose();
            }
        }

        public bool Equals(IPlugin? other) => (this as IPlugin).Equals(other);
	}
}
