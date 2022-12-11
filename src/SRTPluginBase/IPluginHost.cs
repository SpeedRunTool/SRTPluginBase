using Microsoft.Extensions.Hosting;

namespace SRTPluginBase
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
        //{
        //    // If the plugin is not loaded, return default.
        //    if (!LoadedPlugins.ContainsKey(pluginName))
        //        return default;

        //    return LoadedPlugins[pluginName] as T;
        //}
    }
}
