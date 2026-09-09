using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization.Metadata;

namespace SRTPluginBase;

/// <summary>
/// Holds a plugin's settings and the rules for replacing them.
/// </summary>
/// <remarks>
/// A composed helper rather than a base class, because configuration has to be available to a plain
/// plugin, to a producer and to a consumer, and C# gives a type only one base. The three
/// <c>Configurable*PluginBase</c> classes each own one of these, so the loading, casting and
/// validation rules are stated exactly once and cannot drift between them.
/// <para>
/// It deliberately does <em>not</em> save. The host owns
/// <c>%LOCALAPPDATA%\SRTHost\config\&lt;pluginId&gt;.json</c>: it reads that file to fill
/// <c>LoadPlugin.ConfigurationJson</c>, and it writes it only after the runner has accepted the new
/// settings. A plugin that also wrote the file would be a second writer racing the first through
/// <see cref="File.Replace(string, string, string)"/> on the same path, and the loser of that race
/// is the user's settings.
/// </para>
/// </remarks>
/// <typeparam name="TConfiguration">The settings model.</typeparam>
public sealed class PluginConfiguration<TConfiguration>
    where TConfiguration : class, new()
{
    /// <summary>The settings currently in force. Never null; defaults until something replaces them.</summary>
    public TConfiguration Current { get; private set; } = new();

    /// <summary>
    /// Reads settings from <paramref name="filePath"/> and adopts them, or keeps defaults when the
    /// file is not there.
    /// </summary>
    /// <remarks>
    /// The host normally supplies settings through <c>LoadPlugin</c> before this matters, so this is
    /// the path taken by a plugin hosted some other way - and by the first run of a plugin whose
    /// file exists but whose host is older than the setting in it.
    /// </remarks>
    public async ValueTask<TConfiguration> LoadAsync(
        string filePath,
        JsonTypeInfo<TConfiguration> typeInfo,
        CancellationToken cancellationToken = default)
    {
        Current = await PluginConfigurationStore
            .LoadAsync(filePath, typeInfo, cancellationToken)
            .ConfigureAwait(false);

        return Current;
    }

    /// <summary>
    /// Checks a candidate configuration and adopts it.
    /// </summary>
    /// <remarks>
    /// The runner is authoritative on validation, and this is where that happens. The host validates
    /// the same DataAnnotations while the user types, but that is a convenience: settings can also
    /// arrive from a hand-edited JSON file, which never passed through the form at all.
    /// </remarks>
    /// <returns>The adopted settings, so a caller can pass them straight to its change handler.</returns>
    /// <exception cref="ArgumentException"><paramref name="candidate"/> is the wrong type.</exception>
    /// <exception cref="ValidationException">It does not satisfy its own annotations.</exception>
    public TConfiguration Adopt(object candidate)
    {
        ArgumentNullException.ThrowIfNull(candidate);

        if (candidate is not TConfiguration typed)
        {
            throw new ArgumentException(
                $"Expected configuration of type {typeof(TConfiguration).FullName}, got {candidate.GetType().FullName}.",
                nameof(candidate));
        }

        List<ValidationResult> failures = [];

        if (!Validator.TryValidateObject(typed, new ValidationContext(typed), failures, validateAllProperties: true))
        {
            throw new ValidationException(
                "Configuration is invalid: " +
                string.Join("; ", failures.Select(failure => failure.ErrorMessage)));
        }

        Current = typed;
        return typed;
    }
}
