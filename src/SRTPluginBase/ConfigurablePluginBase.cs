using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization.Metadata;
using SRTPluginBase.Abstractions;

namespace SRTPluginBase;

/// <summary>
/// Base class for a plugin with user-editable settings. Handles loading, validating, persisting and
/// applying them; you react in <see cref="OnConfigurationChangedAsync"/>.
/// </summary>
/// <typeparam name="TConfiguration">The settings model.</typeparam>
public abstract class ConfigurablePluginBase<TConfiguration> : PluginBase, IConfigurablePlugin<TConfiguration>
    where TConfiguration : class, new()
{
    private TConfiguration configuration = new();

    /// <inheritdoc />
    public TConfiguration Configuration => configuration;

    /// <inheritdoc />
    object IConfigurablePlugin.Configuration => configuration;

    /// <inheritdoc />
    public Type ConfigurationType => typeof(TConfiguration);

    /// <summary>
    /// Source-generated serialisation metadata for <typeparamref name="TConfiguration"/>.
    /// </summary>
    protected abstract JsonTypeInfo<TConfiguration> ConfigurationTypeInfo { get; }

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

        configuration = await PluginConfigurationStore
            .LoadAsync(ConfigurationFilePath, ConfigurationTypeInfo, cancellationToken)
            .ConfigureAwait(false);

        await OnConfigurationChangedAsync(configuration, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async ValueTask ApplyConfigurationAsync(object newConfiguration, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(newConfiguration);

        if (newConfiguration is not TConfiguration typed)
        {
            throw new ArgumentException(
                $"Expected configuration of type {typeof(TConfiguration).FullName}, got {newConfiguration.GetType().FullName}.",
                nameof(newConfiguration));
        }

        // The runner is authoritative on validation. The host validates the same DataAnnotations
        // while the user types, but that is a convenience: settings can also arrive from a
        // hand-edited JSON file, which never passed through the form at all.
        List<ValidationResult> failures = [];
        if (!Validator.TryValidateObject(typed, new ValidationContext(typed), failures, validateAllProperties: true))
        {
            throw new ValidationException(
                "Configuration is invalid: " +
                string.Join("; ", failures.Select(failure => failure.ErrorMessage)));
        }

        configuration = typed;
        await OnConfigurationChangedAsync(typed, cancellationToken).ConfigureAwait(false);
        await PluginConfigurationStore
            .SaveAsync(ConfigurationFilePath, typed, ConfigurationTypeInfo, cancellationToken)
            .ConfigureAwait(false);
    }
}
