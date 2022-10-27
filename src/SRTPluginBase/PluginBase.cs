using System.Reflection;
using System.Threading.Tasks;

namespace SRTPluginBase
{
    public abstract class PluginBase : IPlugin
    {
        public abstract IPluginInfo Info { get; }

        public abstract IPluginHost? Host { get; set; }

        public string GetConfigFile(Assembly a) => a.GetConfigFile();

        public virtual T LoadConfiguration<T>() where T : class, new() => Extensions.LoadConfiguration<T>(null);
        public T LoadConfiguration<T>(string? configFile = null) where T : class, new() => Extensions.LoadConfiguration<T>(configFile);

        public virtual void SaveConfiguration<T>(T configuration) where T : class, new() => configuration.SaveConfiguration(null);
        public void SaveConfiguration<T>(T configuration, string? configFile = null) where T : class, new() => configuration.SaveConfiguration(configFile);

        public abstract ValueTask DisposeAsync();

        public abstract void Dispose();

        protected abstract void Dispose(bool disposing);

        public bool Equals(IPlugin? other) => (this as IPlugin).Equals(other);
    }
}
