using System.Text.Json.Serialization.Metadata;

namespace SRTPluginBase.Abstractions;

/// <summary>
/// A plugin with user-editable settings.
/// </summary>
/// <remarks>
/// The host never loads plugin assemblies into its own process, so it cannot reflect over the
/// configuration type directly. Instead the runner reflects over
/// <see cref="ConfigurationTypeInfo"/> once at load, ships a schema plus UI hints across the wire,
/// and the host renders a form from that. Anything the schema cannot express degrades to a raw JSON
/// editor rather than being lost.
/// </remarks>
public interface IConfigurablePlugin : IPlugin
{
    /// <summary>
    /// Serialisation metadata for the settings model, which must be a class with a public
    /// parameterless constructor and settable properties.
    /// </summary>
    /// <remarks>
    /// The plugin's own <see cref="JsonTypeInfo"/> rather than the bare <see cref="Type"/>, so that
    /// exactly one serialiser governs the settings document. The runner reads it to describe the
    /// type, to serialise the current values and to deserialise what the user saved, which means the
    /// keys in the generated form are by construction the keys the plugin reads - naming policy,
    /// <c>[JsonPropertyName]</c>, converters and all. Reflecting the CLR type instead produced a form
    /// that wrote <c>Corner</c> into a document the plugin read as <c>corner</c>, and a setting that
    /// silently did nothing.
    /// <para>
    /// Source-generated, so a plugin's settings cost no reflection metadata at run time. Use
    /// <see cref="JsonTypeInfo.Type"/> where the type itself is wanted.
    /// </para>
    /// </remarks>
    JsonTypeInfo ConfigurationTypeInfo { get; }

    /// <summary>The current settings instance.</summary>
    object Configuration { get; }

    /// <summary>
    /// Apply settings edited by the user. Throw to reject them; the host surfaces the message
    /// against the form.
    /// </summary>
    /// <remarks>
    /// Applying does not persist. The host owns
    /// <c>%LOCALAPPDATA%\SRTHost\config\&lt;pluginId&gt;.json</c> and writes it once this has
    /// returned without throwing, so invalid settings never reach disk and there is only ever one
    /// writer of that file.
    /// </remarks>
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
