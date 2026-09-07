namespace SRTPluginBase.Abstractions;

/// <summary>
/// The base contract every SRT Host plugin implements.
/// </summary>
/// <remarks>
/// Lifecycle is <see cref="InitializeAsync"/>, then <see cref="StartAsync"/>, then
/// <see cref="StopAsync"/>, then <see cref="IAsyncDisposable.DisposeAsync"/>. Report failure by
/// throwing; earlier generations returned <see langword="int"/> status codes that callers ignored,
/// so a plugin could fail silently and appear to be running.
/// </remarks>
public interface IPlugin : IAsyncDisposable
{
    /// <summary>Identity and metadata. Must be cheap and allocation-free to read repeatedly.</summary>
    IPluginInfo Info { get; }

    /// <summary>
    /// One-time setup. Capture <paramref name="context"/> here; it is the only handle on the host.
    /// Throw to fail the load.
    /// </summary>
    ValueTask InitializeAsync(IPluginContext context, CancellationToken cancellationToken);

    /// <summary>Begin work. For a producer, this is when attaching to the source becomes worthwhile.</summary>
    ValueTask StartAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Stop work but stay loadable - <see cref="StartAsync"/> may be called again. Release the
    /// external source (game handles and the like) here rather than in dispose.
    /// </summary>
    ValueTask StopAsync(CancellationToken cancellationToken);
}
