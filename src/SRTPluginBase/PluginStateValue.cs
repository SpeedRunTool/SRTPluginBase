using System;
using System.Diagnostics;
using System.Runtime.Loader;

namespace SRTPluginBase
{
    [DebuggerDisplay("[{Plugin.TypeName,nq}]")]
    public class PluginStateValue<T> : IPluginStateValue<T>, IEquatable<PluginStateValue<T>> where T : IPlugin
    {
        public AssemblyLoadContext LoadContext { get; init; }
        public T Plugin { get; init; }

        public PluginStateValue(AssemblyLoadContext loadContext, T plugin)
        {
            LoadContext = loadContext;
            Plugin = plugin;
        }

        public bool Equals(PluginStateValue<T>? other) => Plugin.Info.Name == other?.Plugin.Info.Name;

        public override bool Equals(object? obj) => Equals(obj as PluginStateValue<T>);

        public override int GetHashCode() => HashCode.Combine(LoadContext, Plugin);
    }
}
