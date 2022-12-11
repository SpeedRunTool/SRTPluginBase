using System;
using System.Runtime.Loader;

namespace SRTPluginBase
{
    public interface IPluginStateValue<T> : IEquatable<PluginStateValue<T>> where T : IPlugin
    {
        AssemblyLoadContext LoadContext { get; init; }
        T Plugin { get; init; }
    }
}
