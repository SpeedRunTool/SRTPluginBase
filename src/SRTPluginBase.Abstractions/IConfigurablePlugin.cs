namespace SRTPluginBase.Abstractions;

/// <summary>
/// A plugin with user-editable settings.
/// </summary>
/// <remarks>
/// The host never loads plugin assemblies into its own process, so it cannot reflect over the
/// configuration type directly. Instead the runner reflects over <see cref="ConfigurationType"/>
/// once at load, ships a schema plus UI hints across the wire, and the host renders a form from
/// that. Anything the schema cannot express degrades to a raw JSON editor rather than being lost.
/// </remarks>
public interface IConfigurablePlugin : IPlugin
{
    /// <summary>
    /// The settings model. Must be a class with a public parameterless constructor and settable
    /// properties.
    /// </summary>
    Type ConfigurationType { get; }

    /// <summary>The current settings instance.</summary>
    object Configuration { get; }

    /// <summary>
    /// Apply settings edited by the user. Throw to reject them; the host surfaces the message
    /// against the form.
    /// </summary>
    ValueTask ApplyConfigurationAsync(object configuration, CancellationToken cancellationToken);
}

/// <summary>
/// Strongly-typed <see cref="IConfigurablePlugin"/>. Prefer this; the non-generic form exists for
/// the runner, which handles configuration without knowing the type.
/// </summary>
/// <typeparam name="TConfiguration">The settings model.</typeparam>
public interface IConfigurablePlugin<TConfiguration> : IConfigurablePlugin
    where TConfiguration : class, new()
{
    /// <summary>The current settings.</summary>
    new TConfiguration Configuration { get; }

    /// <summary>
    /// Called after new settings have been validated and applied. React to changes here - restart a
    /// render loop, recolour an overlay - rather than polling <see cref="Configuration"/>.
    /// </summary>
    ValueTask OnConfigurationChangedAsync(TConfiguration configuration, CancellationToken cancellationToken);
}
