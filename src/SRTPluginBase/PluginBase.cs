using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace SRTPluginBase
{
    public abstract class PluginBase<T> : IPlugin where T : class, IPlugin
	{
		private const string DB_CONFIGURATION_TABLE_NAME = "Config";
        private readonly SqliteConnection sqliteConnection;

		protected IDictionary<string, Func<Controller, Task<IActionResult>>> registeredPages;
		public IReadOnlyDictionary<string, Func<Controller, Task<IActionResult>>> RegisteredPages => new ReadOnlyDictionary<string, Func<Controller, Task<IActionResult>>>(registeredPages);

        public PluginBase()
        {
            SqliteConnectionStringBuilder sqliteConnectionStringBuilder = new SqliteConnectionStringBuilder()
            {
                DataSource = Path.Combine(".plugindb", $"{typeof(T).Name}.sqlite"),
                Mode = SqliteOpenMode.ReadWriteCreate
			};
			sqliteConnection = new SqliteConnection(sqliteConnectionStringBuilder.ToString());
            sqliteConnection.Open();
			registeredPages = new Dictionary<string, Func<Controller, Task<IActionResult>>>(StringComparer.OrdinalIgnoreCase);

            if (!this.SqliteTableExists(DB_CONFIGURATION_TABLE_NAME))
			{
				DbNonQuery($"""
                    CREATE TABLE "{DB_CONFIGURATION_TABLE_NAME}" (
                    	"Index"	INTEGER,
                    	"Key"	TEXT NOT NULL UNIQUE,
                    	"Value"	TEXT,
                    	PRIMARY KEY("Index" AUTOINCREMENT)
                    );
                    """, default);
			}
        }

		public abstract IPluginInfo Info { get; }

		public DateTimeOffset LastConfigurationUpdate { get; protected set; }

		public string GetConfigFile(Assembly a) => a.GetConfigFile();

		public virtual T LoadConfiguration()
		{
            LastConfigurationUpdate = DateTimeOffset.UtcNow;
            return Extensions.LoadConfiguration<T>(null);
		}
		public T LoadConfiguration(string? configFile = null)
		{
            LastConfigurationUpdate = DateTimeOffset.UtcNow;
            return Extensions.LoadConfiguration<T>(configFile);
		}

		public virtual void SaveConfiguration(T configuration)
		{
            configuration.SaveConfiguration(null);
            LastConfigurationUpdate = DateTimeOffset.UtcNow;
        }
		public void SaveConfiguration(T configuration, string? configFile = null)
		{
            configuration.SaveConfiguration(configFile);
            LastConfigurationUpdate = DateTimeOffset.UtcNow;
        }

		public virtual IDictionary<string, string?> DbLoadConfiguration()
		{
			IDictionary<string, string?> returnValue = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

			using (IDataReader dataReader = DbReader($"SELECT [Key], [Value] FROM [{DB_CONFIGURATION_TABLE_NAME}];", default, CommandBehavior.SingleResult)!)
			{
				object[] values;
				while (dataReader.Read())
				{
					values = new object[dataReader.FieldCount];
					dataReader.GetValues(values);
					returnValue.TryAdd(Helpers.ConvertDBNullToNull(values[0])?.ToString() ?? string.Empty, Helpers.ConvertDBNullToNull(values[1])?.ToString());
				}
			}

            LastConfigurationUpdate = DateTimeOffset.UtcNow;
            return returnValue;
		}

		public virtual void DbSaveConfiguration(IDictionary<string, string?> config)
		{
			foreach (KeyValuePair<string, string?> kvp in config)
			{
				if (DbRecordExists(DB_CONFIGURATION_TABLE_NAME, "Key", kvp.Key))
					DbNonQuery($"UPDATE [{DB_CONFIGURATION_TABLE_NAME}] SET [Value] = @value WHERE [Key] = @key;", default, new SqliteParameter("@key", kvp.Key), new SqliteParameter("@value", kvp.Value));
				else
					DbNonQuery($"INSERT INTO [{DB_CONFIGURATION_TABLE_NAME}] ([Key], [Value]) VALUES (@key, @value);", default, new SqliteParameter("@key", kvp.Key), new SqliteParameter("@value", kvp.Value));
			}

            LastConfigurationUpdate = DateTimeOffset.UtcNow;
        }

		private bool DbRecordExists(string tableName, string columnName, object? columnValue) => (long)(DbScalar($"SELECT IIF(EXISTS(SELECT 1 FROM {tableName} WHERE [{columnName}] = @columnValue), 1, 0);", default, new SqliteParameter("@columnValue", columnValue)) ?? 0L) == 1L;
		private async Task<bool> DbRecordExistsAsync(string tableName, string columnName, object? columnValue, CancellationToken cancellationToken) => (long)(await DbScalarAsync($"SELECT IIF(EXISTS(SELECT 1 FROM {tableName} WHERE [{columnName}] = @columnValue), 1, 0);", default, cancellationToken, new SqliteParameter("@columnValue", columnValue)).ConfigureAwait(false) ?? 0L) == 1L;

		public virtual int DbNonQuery(string query, IDbTransaction? dbTransaction, params IDbDataParameter[] dbDataParameters)
		{
			using (SqliteCommand sqliteCommand = new SqliteCommand(query, sqliteConnection, dbTransaction as SqliteTransaction))
			{
				if (dbDataParameters is not null)
					sqliteCommand.Parameters.AddRange(dbDataParameters);

				return sqliteCommand.ExecuteNonQuery();
			}
		}
		public virtual async Task<int> DbNonQueryAsync(string query, IDbTransaction? dbTransaction, CancellationToken cancellationToken, params IDbDataParameter[] dbDataParameters)
		{
			using (SqliteCommand sqliteCommand = new SqliteCommand(query, sqliteConnection, dbTransaction as SqliteTransaction))
			{
				if (dbDataParameters is not null)
					sqliteCommand.Parameters.AddRange(dbDataParameters);

				return await sqliteCommand.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
			}
		}

		public virtual object? DbScalar(string query, IDbTransaction? dbTransaction, params IDbDataParameter[] dbDataParameters)
		{
			using (SqliteCommand sqliteCommand = new SqliteCommand(query, sqliteConnection, dbTransaction as SqliteTransaction))
			{
				if (dbDataParameters is not null)
					sqliteCommand.Parameters.AddRange(dbDataParameters);

				return sqliteCommand.ExecuteScalar();
			}
		}
		public virtual async Task<object?> DbScalarAsync(string query, IDbTransaction? dbTransaction, CancellationToken cancellationToken, params IDbDataParameter[] dbDataParameters)
		{
			using (SqliteCommand sqliteCommand = new SqliteCommand(query, sqliteConnection, dbTransaction as SqliteTransaction))
			{
				if (dbDataParameters is not null)
					sqliteCommand.Parameters.AddRange(dbDataParameters);

				return await sqliteCommand.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
			}
		}

		public virtual IDataReader? DbReader(string query, IDbTransaction? dbTransaction, CommandBehavior commandBehavior = CommandBehavior.Default, params IDbDataParameter[] dbDataParameters)
		{
			SqliteCommand sqliteCommand = new SqliteCommand(query, sqliteConnection, dbTransaction as SqliteTransaction);
			if (dbDataParameters is not null)
				sqliteCommand.Parameters.AddRange(dbDataParameters);

			return sqliteCommand.ExecuteReader(commandBehavior);
		}
		public virtual async Task<IDataReader?> DbReaderAsync(string query, IDbTransaction? dbTransaction, CancellationToken cancellationToken, CommandBehavior commandBehavior = CommandBehavior.Default, params IDbDataParameter[] dbDataParameters)
		{
			SqliteCommand sqliteCommand = new SqliteCommand(query, sqliteConnection, dbTransaction as SqliteTransaction);
			if (dbDataParameters is not null)
				sqliteCommand.Parameters.AddRange(dbDataParameters);

			return await sqliteCommand.ExecuteReaderAsync(commandBehavior, cancellationToken).ConfigureAwait(false);
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
