using Microsoft.AspNetCore.Mvc;
using System;
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

        public new bool Equals(IPlugin? other) => TypeName == other?.TypeName && Info.Name == other?.Info.Name;

        public bool Equals(object? obj) => Equals(obj as IPlugin);

        public int GetHashCode() => HashCode.Combine(TypeName, Info);
    }
}
