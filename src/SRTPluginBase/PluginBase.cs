using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SRTPluginBase.Abstractions;

namespace SRTPluginBase;

/// <summary>
/// Base class for a plugin. Handles context capture, logger resolution and disposal so a plugin only
/// implements what it actually does.
/// </summary>
public abstract class PluginBase : IPlugin
{
    private IPluginContext? context;
    private ILogger? logger;
    private bool disposed;

    /// <inheritdoc />
    public abstract IPluginInfo Info { get; }

    /// <summary>
    /// The host context. Throws if read before <see cref="InitializeAsync"/> has run, which is a
    /// programming error rather than a runtime condition worth handling.
    /// </summary>
    protected IPluginContext Context
        => context ?? throw new InvalidOperationException(
            $"{nameof(Context)} is not available until {nameof(InitializeAsync)} has been called.");

    /// <summary>A logger for this plugin. Output is forwarded to the host's log and its log viewer.</summary>
    protected ILogger Logger
        => logger ??= Context.Services.GetService<ILoggerFactory>()?.CreateLogger(Info.Id)
            ?? NullLogger.Instance;

    /// <inheritdoc />
    public virtual ValueTask InitializeAsync(IPluginContext pluginContext, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(pluginContext);
        context = pluginContext;
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public virtual ValueTask StartAsync(CancellationToken cancellationToken) => ValueTask.CompletedTask;

    /// <inheritdoc />
    public virtual ValueTask StopAsync(CancellationToken cancellationToken) => ValueTask.CompletedTask;

    /// <summary>
    /// Release resources. Override this rather than <see cref="DisposeAsync"/>; the base class
    /// guarantees it runs exactly once.
    /// </summary>
    protected virtual ValueTask DisposeAsyncCore() => ValueTask.CompletedTask;

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        // Sealed against the override-and-forget-to-chain bug in the previous generation, where
        // Dispose() was abstract and Dispose(bool) was never called, so every derived plugin leaked
        // its database connection.
        if (disposed)
            return;

        disposed = true;
        await DisposeAsyncCore().ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }
}
