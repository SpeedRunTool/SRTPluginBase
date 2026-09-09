using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using SRTPluginBase.Abstractions;

namespace SRTPluginBase;

/// <summary>
/// Base class for a consumer that also has user-editable settings.
/// </summary>
/// <remarks>
/// This is the shape almost every real consumer wants: an overlay has a colour, a font, a corner and
/// a set of values to show, and it consumes a channel. C# gives a type one base class, so the
/// combination is its own rather than something an author assembles.
/// <para>
/// The payload logic is the same eight lines as
/// <see cref="ConsumerPluginBase{TPayload}.ConsumeAsync"/> - too small to be worth a helper - while
/// the configuration rules come from <see cref="PluginConfiguration{TConfiguration}"/>, which is
/// shared with the other two configurable bases.
/// </para>
/// </remarks>
/// <typeparam name="TPayload">The payload type, from the producer's contract assembly.</typeparam>
/// <typeparam name="TConfiguration">The settings model.</typeparam>
public abstract class ConfigurableConsumerPluginBase<TPayload, TConfiguration>
    : PluginBase, IConsumerPlugin, IConfigurablePlugin<TConfiguration>
    where TPayload : class
    where TConfiguration : class, new()
{
    private readonly PluginConfiguration<TConfiguration> configuration = new();

    /// <inheritdoc />
    public abstract IReadOnlyList<ChannelSubscription> Subscriptions { get; }

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

    /// <summary>Handle one decoded payload.</summary>
    protected abstract ValueTask OnPayloadAsync(TPayload payload, CancellationToken cancellationToken);

    /// <summary>Called after settings have been validated and applied.</summary>
    public virtual ValueTask OnConfigurationChangedAsync(TConfiguration newConfiguration, CancellationToken cancellationToken)
        => ValueTask.CompletedTask;

    /// <inheritdoc />
    public virtual ValueTask OnChannelClosedAsync(string channelId, CancellationToken cancellationToken)
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
    public async ValueTask ConsumeAsync(PayloadFrame frame, CancellationToken cancellationToken)
    {
        // frame.Payload points into a pooled buffer that is recycled as soon as this returns, so
        // deserialise before awaiting anything.
        TPayload? payload = JsonSerializer.Deserialize(frame.Payload.Span, PayloadTypeInfo);

        if (payload is null)
            return;

        await OnPayloadAsync(payload, cancellationToken).ConfigureAwait(false);
    }
}
