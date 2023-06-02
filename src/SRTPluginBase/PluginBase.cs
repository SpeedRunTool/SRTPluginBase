using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace SRTPluginBase
{
    public abstract class PluginBase<T> : IPlugin where T : class, IPlugin
	{
		private ConfigurationDB<T> pluginConfigDatabase;

        protected IDictionary<RegisteredPagesKey, Func<Controller, Task<IActionResult>>> registeredPages;
		[JsonIgnore(Condition = JsonIgnoreCondition.Always)]
		public IReadOnlyDictionary<RegisteredPagesKey, Func<Controller, Task<IActionResult>>> RegisteredPages => new ReadOnlyDictionary<RegisteredPagesKey, Func<Controller, Task<IActionResult>>>(registeredPages);

        public PluginBase()
        {
			pluginConfigDatabase = new ConfigurationDB<T>();
            registeredPages = new Dictionary<RegisteredPagesKey, Func<Controller, Task<IActionResult>>>(RegisteredPagesKeyComparer.DefaultComparer);
        }

		public abstract IPluginInfo Info { get; }

        public IPluginConfiguration? Configuration { get; protected set; }

        public IDictionary<string, string?> LoadConfiguration() => pluginConfigDatabase.DbLoadConfiguration();

        public void SaveConfiguration(IDictionary<string, string?> config) => pluginConfigDatabase.DbSaveConfiguration(config);

        public bool DbRecordExists(string tableName, string columnName, object? columnValue) => (long)(pluginConfigDatabase.DbScalar($"SELECT IIF(EXISTS(SELECT 1 FROM {tableName} WHERE [{columnName}] = @columnValue), 1, 0);", default, new SqliteParameter("@columnValue", columnValue)) ?? 0L) == 1L;

        public async Task<bool> DbRecordExistsAsync(string tableName, string columnName, object? columnValue, CancellationToken cancellationToken) => (long)(await pluginConfigDatabase.DbScalarAsync($"SELECT IIF(EXISTS(SELECT 1 FROM {tableName} WHERE [{columnName}] = @columnValue), 1, 0);", default, cancellationToken, new SqliteParameter("@columnValue", columnValue)).ConfigureAwait(false) ?? 0L) == 1L;

        public int DbNonQuery(string query, IDbTransaction? dbTransaction, params IDbDataParameter[] dbDataParameters) => pluginConfigDatabase.DbNonQuery(query, dbTransaction, dbDataParameters);

        public Task<int> DbNonQueryAsync(string query, IDbTransaction? dbTransaction, CancellationToken cancellationToken, params IDbDataParameter[] dbDataParameters) => pluginConfigDatabase.DbNonQueryAsync(query, dbTransaction, cancellationToken, dbDataParameters);

        public object? DbScalar(string query, IDbTransaction? dbTransaction, params IDbDataParameter[] dbDataParameters) => pluginConfigDatabase.DbScalar(query, dbTransaction, dbDataParameters);

        public Task<object?> DbScalarAsync(string query, IDbTransaction? dbTransaction, CancellationToken cancellationToken, params IDbDataParameter[] dbDataParameters) => pluginConfigDatabase.DbScalarAsync(query, dbTransaction, cancellationToken, dbDataParameters);

        public IDataReader? DbReader(string query, IDbTransaction? dbTransaction, CommandBehavior commandBehavior = CommandBehavior.Default, params IDbDataParameter[] dbDataParameters) => pluginConfigDatabase.DbReader(query, dbTransaction, commandBehavior, dbDataParameters);

        public Task<IDataReader?> DbReaderAsync(string query, IDbTransaction? dbTransaction, CancellationToken cancellationToken, CommandBehavior commandBehavior = CommandBehavior.Default, params IDbDataParameter[] dbDataParameters) => pluginConfigDatabase.DbReaderAsync(query, dbTransaction, cancellationToken, commandBehavior, dbDataParameters);

        public virtual async ValueTask DisposeAsync()
        {
            if (pluginConfigDatabase is not null)
                await pluginConfigDatabase.DisposeAsync();
        }

        public abstract void Dispose();

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                pluginConfigDatabase?.Dispose();
            }
        }

        public bool Equals(IPlugin? other) => (this as IPlugin).Equals(other);
	}
}
