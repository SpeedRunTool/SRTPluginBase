using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace SRTPluginBase
{
    public sealed class ConfigurationDB<T> : IDisposable, IAsyncDisposable
    {
        public const string DB_CONFIGURATION_FOLDER_NAME = ".db";
        public const string DB_CONFIGURATION_TABLE_NAME = "Config";

        private readonly SqliteConnection sqliteConnection;

        public ConfigurationDB() : this(typeof(T).Name) { }

        public ConfigurationDB(string databaseName)
        {
            SqliteConnectionStringBuilder sqliteConnectionStringBuilder = new SqliteConnectionStringBuilder()
            {
                DataSource = Path.Combine(DB_CONFIGURATION_FOLDER_NAME, $"{databaseName}.db"),
                Mode = SqliteOpenMode.ReadWriteCreate
            };
            sqliteConnection = new SqliteConnection(sqliteConnectionStringBuilder.ToString());
            sqliteConnection.Open();

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

        public IDictionary<string, string?> DbLoadConfiguration()
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

            return returnValue;
        }

        public void DbSaveConfiguration(IDictionary<string, string?> config)
        {
            foreach (KeyValuePair<string, string?> kvp in config)
            {
                if (DbRecordExists(DB_CONFIGURATION_TABLE_NAME, "Key", kvp.Key))
                    DbNonQuery($"UPDATE [{DB_CONFIGURATION_TABLE_NAME}] SET [Value] = @value WHERE [Key] = @key;", default, new SqliteParameter("@key", kvp.Key), new SqliteParameter("@value", kvp.Value));
                else
                    DbNonQuery($"INSERT INTO [{DB_CONFIGURATION_TABLE_NAME}] ([Key], [Value]) VALUES (@key, @value);", default, new SqliteParameter("@key", kvp.Key), new SqliteParameter("@value", kvp.Value));
            }
        }

        public bool DbRecordExists(string tableName, string columnName, object? columnValue) => (long)(DbScalar($"SELECT IIF(EXISTS(SELECT 1 FROM {tableName} WHERE [{columnName}] = @columnValue), 1, 0);", default, new SqliteParameter("@columnValue", columnValue)) ?? 0L) == 1L;

        public async Task<bool> DbRecordExistsAsync(string tableName, string columnName, object? columnValue, CancellationToken cancellationToken) => (long)(await DbScalarAsync($"SELECT IIF(EXISTS(SELECT 1 FROM {tableName} WHERE [{columnName}] = @columnValue), 1, 0);", default, cancellationToken, new SqliteParameter("@columnValue", columnValue)).ConfigureAwait(false) ?? 0L) == 1L;

        public int DbNonQuery(string query, IDbTransaction? dbTransaction, params IDbDataParameter[] dbDataParameters)
        {
            using (SqliteCommand sqliteCommand = new SqliteCommand(query, sqliteConnection, dbTransaction as SqliteTransaction))
            {
                if (dbDataParameters is not null)
                    sqliteCommand.Parameters.AddRange(dbDataParameters);

                return sqliteCommand.ExecuteNonQuery();
            }
        }

        public async Task<int> DbNonQueryAsync(string query, IDbTransaction? dbTransaction, CancellationToken cancellationToken, params IDbDataParameter[] dbDataParameters)
        {
            using (SqliteCommand sqliteCommand = new SqliteCommand(query, sqliteConnection, dbTransaction as SqliteTransaction))
            {
                if (dbDataParameters is not null)
                    sqliteCommand.Parameters.AddRange(dbDataParameters);

                return await sqliteCommand.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        public object? DbScalar(string query, IDbTransaction? dbTransaction, params IDbDataParameter[] dbDataParameters)
        {
            using (SqliteCommand sqliteCommand = new SqliteCommand(query, sqliteConnection, dbTransaction as SqliteTransaction))
            {
                if (dbDataParameters is not null)
                    sqliteCommand.Parameters.AddRange(dbDataParameters);

                return sqliteCommand.ExecuteScalar();
            }
        }

        public async Task<object?> DbScalarAsync(string query, IDbTransaction? dbTransaction, CancellationToken cancellationToken, params IDbDataParameter[] dbDataParameters)
        {
            using (SqliteCommand sqliteCommand = new SqliteCommand(query, sqliteConnection, dbTransaction as SqliteTransaction))
            {
                if (dbDataParameters is not null)
                    sqliteCommand.Parameters.AddRange(dbDataParameters);

                return await sqliteCommand.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        public IDataReader? DbReader(string query, IDbTransaction? dbTransaction, CommandBehavior commandBehavior = CommandBehavior.Default, params IDbDataParameter[] dbDataParameters)
        {
            SqliteCommand sqliteCommand = new SqliteCommand(query, sqliteConnection, dbTransaction as SqliteTransaction);
            if (dbDataParameters is not null)
                sqliteCommand.Parameters.AddRange(dbDataParameters);

            return sqliteCommand.ExecuteReader(commandBehavior);
        }
        public async Task<IDataReader?> DbReaderAsync(string query, IDbTransaction? dbTransaction, CancellationToken cancellationToken, CommandBehavior commandBehavior = CommandBehavior.Default, params IDbDataParameter[] dbDataParameters)
        {
            SqliteCommand sqliteCommand = new SqliteCommand(query, sqliteConnection, dbTransaction as SqliteTransaction);
            if (dbDataParameters is not null)
                sqliteCommand.Parameters.AddRange(dbDataParameters);

            return await sqliteCommand.ExecuteReaderAsync(commandBehavior, cancellationToken).ConfigureAwait(false);
        }

        private bool disposedValue;
        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    sqliteConnection?.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~ConfigurationDB()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public async ValueTask DisposeAsync()
        {
            if (sqliteConnection is not null)
                await sqliteConnection.DisposeAsync();
        }
    }
}
