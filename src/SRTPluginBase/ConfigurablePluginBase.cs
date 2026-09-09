using System.Text.Json.Serialization.Metadata;
using SRTPluginBase.Abstractions;

namespace SRTPluginBase;

/// <summary>
/// Base class for a plugin with user-editable settings that is neither a producer nor a consumer.
/// Handles loading, validating and applying them; you react in
/// <see cref="OnConfigurationChangedAsync"/>.
/// </summary>
/// <remarks>
/// A type gets one base class, so the two combinations have their own:
/// <see cref="ConfigurableProducerPluginBase{TPayload, TConfiguration}"/> and
/// <see cref="ConfigurableConsumerPluginBase{TPayload, TConfiguration}"/>. All three share
/// <see cref="PluginConfiguration{TConfiguration}"/>, so the rules are written once.
/// </remarks>
/// <typeparam name="TConfiguration">The settings model.</typeparam>
public abstract class ConfigurablePluginBase<TConfiguration> : PluginBase, IConfigurablePlugin<TConfiguration>
    where TConfiguration : class, new()
{
    private readonly PluginConfiguration<TConfiguration> configuration = new();

    /// <inheritdoc />
    public TConfiguration Configuration => configuration.Current;

    /// <inheritdoc />
    object IConfigurablePlugin.Configuration => configuration.Current;

    /// <summary>
    /// Source-generated serialisation metadata for <typeparamref name="TConfiguration"/>.
    /// </summary>
    protected abstract JsonTypeInfo<TConfiguration> ConfigurationTypeInfo { get; }

    /// <inheritdoc />
    JsonTypeInfo IConfigurablePlugin.ConfigurationTypeInfo => ConfigurationTypeInfo;

    /// <summary>Where the settings file lives. Defaults to the host's conventional location.</summary>
    protected virtual string ConfigurationFilePath => PluginConfigurationStore.GetDefaultPath(Info.Id);

    /// <summary>
    /// Called after settings have been validated and applied, including once during initialisation
    /// with whatever was loaded from disk.
    /// </summary>
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
    /// <remarks>
    /// Adopting the settings does not write them anywhere. The host owns the settings file and
    /// persists it only once the runner has accepted the change - see
    /// <see cref="PluginConfiguration{TConfiguration}"/> for why there is exactly one writer.
    /// </remarks>
    public async ValueTask ApplyConfigurationAsync(object newConfiguration, CancellationToken cancellationToken)
        => await OnConfigurationChangedAsync(configuration.Adopt(newConfiguration), cancellationToken)
            .ConfigureAwait(false);
}
