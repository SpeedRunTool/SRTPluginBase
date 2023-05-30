using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace SRTPluginBase
{
    public interface IPlugin : IDisposable, IAsyncDisposable, IEquatable<IPlugin>
    {
        /// <summary>
        /// Information about this plugin.
        /// </summary>
        IPluginInfo Info { get; }

        /// <summary>
        /// Registered pages for the plugin host to display and handle.
        /// </summary>
        IReadOnlyDictionary<RegisteredPagesKey, Func<Controller, Task<IActionResult>>> RegisteredPages { get; }

		virtual int DbNonQuery(string query, IDbTransaction? dbTransaction, params IDbDataParameter[] dbDataParameters) => default;
		virtual Task<int> DbNonQueryAsync(string query, IDbTransaction? dbTransaction, CancellationToken cancellationToken, params IDbDataParameter[] dbDataParameters) => Task.FromResult<int>(default);

		virtual object? DbScalar(string query, IDbTransaction? dbTransaction, params IDbDataParameter[] dbDataParameters) => default;
		virtual Task<object?> DbScalarAsync(string query, IDbTransaction? dbTransaction, CancellationToken cancellationToken, params IDbDataParameter[] dbDataParameters) => Task.FromResult<object?>(default);

		virtual IDataReader? DbReader(string query, IDbTransaction? dbTransaction, CommandBehavior commandBehavior = CommandBehavior.Default, params IDbDataParameter[] dbDataParameters) => default;
		virtual Task<IDataReader?> DbReaderAsync(string query, IDbTransaction? dbTransaction, CancellationToken cancellationToken, CommandBehavior commandBehavior = CommandBehavior.Default, params IDbDataParameter[] dbDataParameters) => Task.FromResult<IDataReader?>(default);

		public new bool Equals(IPlugin? other) => GetType().Name == other?.GetType().Name && Info.Name == other?.Info.Name;

        public bool Equals(object? obj) => Equals(obj as IPlugin);

        public int GetHashCode() => HashCode.Combine(GetType().Name, Info);
    }
}
