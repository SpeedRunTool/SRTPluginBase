namespace SRTPluginBase.Abstractions;

/// <summary>
/// Everything the host hands a plugin at initialisation. Replaces the delegate bag
/// (<c>IPluginHostDelegates</c>) of earlier generations.
/// </summary>
public interface IPluginContext
{
    /// <summary>
    /// Host-provided services. Resolve <c>ILogger&lt;T&gt;</c> here.
    /// </summary>
    /// <remarks>
    /// Typed as the framework's <see cref="IServiceProvider"/> rather than a
    /// <c>Microsoft.Extensions.DependencyInjection</c> type so this assembly stays dependency-free;
    /// the SRTPluginBase package adds the ergonomic <c>GetRequiredService&lt;T&gt;()</c> on top.
    /// </remarks>
    IServiceProvider Services { get; }

    /// <summary>The plugin's own metadata, as the host resolved it.</summary>
    IPluginInfo Info { get; }

    /// <summary>The architecture of the runner process actually hosting this plugin.</summary>
    PluginArchitecture ProcessArchitecture { get; }

    /// <summary>
    /// The directory the plugin was loaded from. Treat as read-only: an in-app update replaces this
    /// directory wholesale, so anything written here is lost.
    /// </summary>
    string PluginDirectory { get; }

    /// <summary>
    /// A writable per-plugin directory that survives updates
    /// (<c>%LOCALAPPDATA%\SRTHost\state\&lt;id&gt;</c>). Use this for caches and scratch files.
    /// </summary>
    string StateDirectory { get; }

    /// <summary>Signals that the host is shutting the runner down.</summary>
    CancellationToken Stopping { get; }
}
