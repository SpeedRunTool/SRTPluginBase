namespace SRTPluginBase;

/// <summary>Thrown when a plugin fails to initialise.</summary>
public class PluginInitializationException : Exception
{
    /// <summary>The plugin that failed.</summary>
    public string PluginId { get; }

    /// <summary>Creates the exception.</summary>
    public PluginInitializationException(string pluginId, string? message = null, Exception? innerException = null)
        : base(message ?? $"Plugin '{pluginId}' failed to initialize.", innerException)
        => PluginId = pluginId;
}

/// <summary>Thrown when a plugin cannot be found on disk.</summary>
public class PluginNotFoundException : Exception
{
    /// <summary>The plugin that was looked for.</summary>
    public string PluginId { get; }

    /// <summary>Creates the exception.</summary>
    public PluginNotFoundException(string pluginId, string? message = null, Exception? innerException = null)
        : base(message ?? $"Plugin '{pluginId}' was not found.", innerException)
        => PluginId = pluginId;
}

/// <summary>
/// Thrown when a consumer requires a channel that no running producer provides.
/// </summary>
public class ChannelUnavailableException : Exception
{
    /// <summary>The channel that was required.</summary>
    public string ChannelId { get; }

    /// <summary>Creates the exception.</summary>
    public ChannelUnavailableException(string channelId, string? message = null, Exception? innerException = null)
        : base(message ?? $"No producer is publishing channel '{channelId}'.", innerException)
        => ChannelId = channelId;
}
