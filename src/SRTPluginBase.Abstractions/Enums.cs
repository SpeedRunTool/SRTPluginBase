namespace SRTPluginBase.Abstractions;

/// <summary>What role a plugin plays in the data flow.</summary>
public enum PluginKind
{
    /// <summary>Reads an external source (typically game memory) and publishes payloads.</summary>
    Producer,

    /// <summary>Subscribes to one or more channels and renders or forwards what it receives.</summary>
    Consumer,
}

/// <summary>
/// The process architecture a plugin requires. A producer must match the bitness of the process it
/// reads, which is why the host ships both a 64-bit and a 32-bit plugin runner.
/// </summary>
public enum PluginArchitecture
{
    /// <summary>No requirement; the host picks. Defaults to 64-bit.</summary>
    Any,

    /// <summary>Must run in the 32-bit runner.</summary>
    X86,

    /// <summary>Must run in the 64-bit runner.</summary>
    X64,
}

/// <summary>Where a plugin is in its lifecycle, as tracked by the host.</summary>
public enum PluginStatus
{
    /// <summary>Discovered on disk but not yet loaded into a runner.</summary>
    NotLoaded,

    /// <summary>A runner is starting and the assembly is being loaded.</summary>
    Loading,

    /// <summary>Loaded and initialised, but not started.</summary>
    Loaded,

    /// <summary><see cref="IPlugin.StartAsync"/> is in flight.</summary>
    Starting,

    /// <summary>Started and working.</summary>
    Running,

    /// <summary><see cref="IPlugin.StopAsync"/> is in flight.</summary>
    Stopping,

    /// <summary>Stopped, but still loaded and startable again.</summary>
    Stopped,

    /// <summary>The runner is shutting down and the plugin is being disposed.</summary>
    Unloading,

    /// <summary>The runner process died unexpectedly.</summary>
    Crashed,

    /// <summary>Load or initialisation failed; see the accompanying <see cref="PluginSubStatus"/>.</summary>
    Faulted,

    /// <summary>Declares a contract generation this host cannot load.</summary>
    Incompatible,
}

/// <summary>
/// Why a plugin is not running. Flags because a single failure can have more than one cause worth
/// reporting, and because the UI shows them together.
/// </summary>
[Flags]
public enum PluginSubStatus
{
    /// <summary>No fault.</summary>
    None = 0,

    /// <summary>An exception the host could not classify. See the log for the stack trace.</summary>
    UndefinedException = 1 << 0,

    /// <summary>Built for the other bitness - a 32-bit plugin in the 64-bit runner or vice versa.</summary>
    IncorrectArchitecture = 1 << 1,

    /// <summary><see cref="IPlugin.InitializeAsync"/> threw.</summary>
    InitializationFailure = 1 << 2,

    /// <summary>An assembly the plugin needs was not found alongside it.</summary>
    DependencyNotFound = 1 << 3,

    /// <summary>Built against a contract generation this host cannot load.</summary>
    ContractGenerationMismatch = 1 << 4,

    /// <summary>The stored configuration failed validation and was rejected.</summary>
    ConfigurationInvalid = 1 << 5,

    /// <summary>The producer's source (usually the game process) is not currently available.</summary>
    SourceUnavailable = 1 << 6,

    /// <summary>The runner process exited unexpectedly.</summary>
    RunnerCrashed = 1 << 7,

    /// <summary>Restarted too many times in too short a window; the host gave up.</summary>
    RestartLimitExceeded = 1 << 8,
}

/// <summary>
/// How a payload's bytes are encoded on the wire. The host never decodes a payload; this tells the
/// consumer how to.
/// </summary>
public enum PayloadCodec : byte
{
    /// <summary>UTF-8 JSON. The default, and the only one a plugin needs to think about.</summary>
    Json = 0,

    /// <summary>Opaque UTF-8 text.</summary>
    Utf8Raw = 1,

    /// <summary>
    /// Raw memory of an unmanaged, sequentially-laid-out struct. Zero serialisation cost; see
    /// <see cref="SrtBlittablePayloadAttribute"/> for the constraints and the layout-hash safeguard.
    /// </summary>
    Blittable = 2,

    /// <summary>Reserved; not implemented.</summary>
    MessagePack = 3,
}
