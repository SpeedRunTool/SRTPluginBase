using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SRTPluginBase
{
    public abstract class PluginHostBase : BackgroundService, IHostedService, IPluginHost
    {
        protected IDictionary<string, IPluginStateValue<IPlugin>> loadedPlugins = new Dictionary<string, IPluginStateValue<IPlugin>>(StringComparer.OrdinalIgnoreCase);
        public IReadOnlyDictionary<string, IPluginStateValue<IPlugin>> LoadedPlugins => loadedPlugins.AsReadOnly();

        public abstract Task StartAsync(CancellationToken cancellationToken);
        public abstract Task StopAsync(CancellationToken cancellationToken);

        public T? GetPluginReference<T>(string pluginName) where T : class, IPlugin
        {
            // If the plugin is not loaded, return default.
            if (!LoadedPlugins.ContainsKey(pluginName))
                return default;

            return LoadedPlugins[pluginName].Plugin as T;
        }
    }
}
