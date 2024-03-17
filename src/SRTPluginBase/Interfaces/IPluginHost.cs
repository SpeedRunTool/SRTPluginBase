using Microsoft.Extensions.Hosting;

namespace SRTPluginBase.Interfaces
{
    public interface IPluginHost : IHostedService
    {
        /// <summary>
        /// Retrieves a loaded plugin by name.
        /// </summary>
        /// <typeparam name="T">The type of plugin to retrieve.</typeparam>
        /// <param name="pluginName">The plugin's name.</param>
        /// <returns>The requested plugin, or null if not loaded.</returns>
        T? GetPluginReference<T>(string pluginName) where T : class, IPlugin;
    }
}
