using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace SRTPluginBase;

/// <summary>
/// Reads and writes a plugin's settings as a JSON file.
/// </summary>
/// <remarks>
/// Replaces the SQLite <c>ConfigurationDB</c> of the previous generation. JSON is human-editable and
/// diffable, needs no native dependency, and matches the host's fallback settings editor, which
/// edits this exact file.
/// <para>
/// Files live under <c>%LOCALAPPDATA%\SRTHost\config\&lt;pluginId&gt;.json</c>, deliberately outside
/// the plugin directory: an in-app update replaces that directory wholesale, and settings stored
/// beside the DLL would be destroyed by every update.
/// </para>
/// <para>
/// <b>The host writes this file; a plugin normally only reads it.</b> The host reads it to fill
/// <c>LoadPlugin.ConfigurationJson</c> and rewrites it once the runner has accepted a change, which
/// keeps a single writer on a path that is replaced non-atomically-in-appearance through
/// <see cref="File.Replace(string, string, string)"/>. <see cref="SaveAsync"/> stays public for a
/// plugin hosted some other way, and for tests; the configurable base classes do not call it.
/// </para>
/// </remarks>
public static class PluginConfigurationStore
{
    private static readonly JsonWriterOptions WriterOptions = new() { Indented = true };

    /// <summary>
    /// Loads settings from <paramref name="filePath"/>, returning a default instance when the file
    /// does not exist yet.
    /// </summary>
    /// <remarks>
    /// A malformed file throws rather than being silently replaced with defaults - losing a user's
    /// settings without telling them is worse than failing the load, and the host reports it as
    /// <see cref="Abstractions.PluginSubStatus.ConfigurationInvalid"/>.
    /// </remarks>
    public static async ValueTask<TConfiguration> LoadAsync<TConfiguration>(
        string filePath,
        JsonTypeInfo<TConfiguration> typeInfo,
        CancellationToken cancellationToken = default)
        where TConfiguration : class, new()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        ArgumentNullException.ThrowIfNull(typeInfo);

        if (!File.Exists(filePath))
            return new TConfiguration();

        await using FileStream stream = new(
            filePath, FileMode.Open, FileAccess.Read, FileShare.Read,
            bufferSize: 4096, useAsync: true);

        if (stream.Length == 0)
            return new TConfiguration();

        return await JsonSerializer.DeserializeAsync(stream, typeInfo, cancellationToken).ConfigureAwait(false)
            ?? new TConfiguration();
    }

    /// <summary>
    /// Writes settings to <paramref name="filePath"/>.
    /// </summary>
    /// <remarks>
    /// Writes to a temporary file and then replaces the target, so an interrupted save - a crash, or
    /// the host being killed mid-write - leaves the previous settings intact rather than a truncated
    /// file that fails to parse on next launch.
    /// </remarks>
    public static async ValueTask SaveAsync<TConfiguration>(
        string filePath,
        TConfiguration configuration,
        JsonTypeInfo<TConfiguration> typeInfo,
        CancellationToken cancellationToken = default)
        where TConfiguration : class
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(typeInfo);

        string? directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        string temporaryPath = filePath + ".tmp";

        await using (FileStream stream = new(
            temporaryPath, FileMode.Create, FileAccess.Write, FileShare.None,
            bufferSize: 4096, useAsync: true))
        await using (Utf8JsonWriter writer = new(stream, WriterOptions))
        {
            JsonSerializer.Serialize(writer, configuration, typeInfo);
            await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
        }

        if (File.Exists(filePath))
            File.Replace(temporaryPath, filePath, destinationBackupFileName: null);
        else
            File.Move(temporaryPath, filePath);
    }

    /// <summary>
    /// The conventional settings path for a plugin id, under
    /// <c>%LOCALAPPDATA%\SRTHost\config</c>.
    /// </summary>
    public static string GetDefaultPath(string pluginId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pluginId);
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SRTHost", "config", pluginId + ".json");
    }
}
