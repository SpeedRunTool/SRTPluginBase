using System.Buffers;
using System.Text.Json.Serialization.Metadata;
using SRTPluginBase.Abstractions;

namespace SRTPluginBase;

/// <summary>
/// Base class for a producer that also has user-editable settings.
/// </summary>
/// <remarks>
/// The combination needs its own base because C# gives a type one base class, and a producer that
/// the user can configure - a poll rate, a process name, which values to publish - is the ordinary
/// case rather than an exotic one. Everything here is delegated:
/// <see cref="PayloadPublisher{TPayload}"/> does the serialising and change detection, and
/// <see cref="PluginConfiguration{TConfiguration}"/> the loading and validation, exactly as they do
/// for <see cref="ProducerPluginBase{TPayload}"/> and
/// <see cref="ConfigurablePluginBase{TConfiguration}"/>.
/// </remarks>
/// <typeparam name="TPayload">The payload type, from a dependency-free contract assembly.</typeparam>
/// <typeparam name="TConfiguration">The settings model.</typeparam>
public abstract class ConfigurableProducerPluginBase<TPayload, TConfiguration>
    : PluginBase, IProducerPlugin, IConfigurablePlugin<TConfiguration>
    where TPayload : class
    where TConfiguration : class, new()
{
    private readonly PayloadPublisher<TPayload> publisher = new();
    private readonly PluginConfiguration<TConfiguration> configuration = new();

    /// <inheritdoc />
    public abstract PayloadChannelDescriptor Channel { get; }

    /// <inheritdoc />
    public abstract bool IsSourceAvailable { get; }

    /// <inheritdoc />
    public TConfiguration Configuration => configuration.Current;

    /// <inheritdoc />
    object IConfigurablePlugin.Configuration => configuration.Current;

    /// <summary>Source-generated serialisation metadata for <typeparamref name="TPayload"/>.</summary>
    protected abstract JsonTypeInfo<TPayload> PayloadTypeInfo { get; }

    /// <summary>Source-generated serialisation metadata for <typeparamref name="TConfiguration"/>.</summary>
    protected abstract JsonTypeInfo<TConfiguration> ConfigurationTypeInfo { get; }

    /// <inheritdoc />
    JsonTypeInfo IConfigurablePlugin.ConfigurationTypeInfo => ConfigurationTypeInfo;

    /// <summary>Where the settings file lives. Defaults to the host's conventional location.</summary>
    protected virtual string ConfigurationFilePath => PluginConfigurationStore.GetDefaultPath(Info.Id);

    /// <summary>
    /// Whether to skip publishing when the serialised payload is identical to the previous one.
    /// </summary>
    protected virtual bool SuppressUnchangedPayloads => true;

    /// <summary>Read the current state of the source.</summary>
    /// <returns><see langword="null"/> to skip this tick.</returns>
    protected abstract ValueTask<TPayload?> RefreshAsync(CancellationToken cancellationToken);

    /// <summary>Called after settings have been validated and applied.</summary>
    public virtual ValueTask OnConfigurationChangedAsync(TConfiguration newConfiguration, CancellationToken cancellationToken)
        => ValueTask.CompletedTask;

    /// <inheritdoc />
    public override async ValueTask InitializeAsync(IPluginContext pluginContext, CancellationToken cancellationToken)
    {
        await base.InitializeAsync(pluginContext, cancellationToken).ConfigureAwait(false);

        TConfiguration loaded = await configuration
            .LoadAsync(ConfigurationFilePath, ConfigurationTypeInfo, cancellationToken)
            .ConfigureAwait(false);

        await OnConfigurationChangedAsync(loaded, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async ValueTask ApplyConfigurationAsync(object newConfiguration, CancellationToken cancellationToken)
        => await OnConfigurationChangedAsync(configuration.Adopt(newConfiguration), cancellationToken)
            .ConfigureAwait(false);

    /// <inheritdoc />
    public async ValueTask<bool> TryProduceAsync(IBufferWriter<byte> destination, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(destination);

        TPayload? payload = await RefreshAsync(cancellationToken).ConfigureAwait(false);

        return publisher.TryWrite(payload, PayloadTypeInfo, destination, SuppressUnchangedPayloads);
    }
}
