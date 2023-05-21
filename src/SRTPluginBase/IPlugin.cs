using Microsoft.AspNetCore.Mvc;
using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace SRTPluginBase
{
    public interface IPlugin : IDisposable, IAsyncDisposable, IEquatable<IPlugin>
    {
        /// <summary>
        /// Gets the plugins type name.
        /// </summary>
        string TypeName => this.GetType().Name;

        /// <summary>
        /// Information about this plugin.
        /// </summary>
        IPluginInfo Info { get; }

        /// <summary>
        /// This method is called when an HTTP request comes in that is not automatically handled by the SRT framework.
        /// </summary>
        /// <param name="controller">The controller that received the request.</param>
        /// <returns>An Task&gt;IActionResult&lt; for the request.</returns>
        virtual Task<IActionResult> HttpHandlerAsync(ControllerBase controller)
        {
            return Task.FromResult<IActionResult>(controller.NoContent());
        }

		virtual Task<int> DbNonQuery(string query, IDbTransaction dbTransaction, CancellationToken cancellationToken, params IDbDataParameter[] dbDataParameters) => Task.FromResult<int>(default);

		virtual Task<object?> DbScalar(string query, IDbTransaction dbTransaction, CancellationToken cancellationToken, params IDbDataParameter[] dbDataParameters) => Task.FromResult<object?>(default);

		virtual Task<IDataReader?> DbReader(string query, IDbTransaction? dbTransaction, CancellationToken cancellationToken, CommandBehavior commandBehavior = CommandBehavior.Default, params IDbDataParameter[] dbDataParameters) => Task.FromResult<IDataReader?>(default);

		public new bool Equals(IPlugin? other) => TypeName == other?.TypeName && Info.Name == other?.Info.Name;

        public bool Equals(object? obj) => Equals(obj as IPlugin);

        public int GetHashCode() => HashCode.Combine(TypeName, Info);
    }
}
