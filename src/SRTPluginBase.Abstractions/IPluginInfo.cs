namespace SRTPluginBase.Abstractions;

/// <summary>
/// Identity and metadata for a plugin. The host reads this to decide which runner to start, whether
/// the plugin is loadable at all, and what to show in the UI.
/// </summary>
public interface IPluginInfo
{
    /// <summary>
    /// Stable, unique, immutable identity in reverse-DNS form, e.g.
    /// <c>com.speedruntool.re4r.producer</c>.
    /// </summary>
    /// <remarks>
    /// This must never change across versions of a plugin: it keys the plugin's configuration file,
    /// its catalog entry and its subscriptions.
    /// <para>
    /// Earlier contract generations keyed identity off <c>GetType().Name</c>, which was not unique in
    /// practice - the shipped RE8 overlay is a copy of the RE2 one and still declares both the
    /// namespace and the class as <c>SRTPluginUIRE2DirectXOverlay</c>, so the two collided whenever
    /// both were loaded. An explicit id removes the possibility.
    /// </para>
    /// </remarks>
    string Id { get; }

    /// <summary>Display name, shown in the plugin list.</summary>
    string Name { get; }

    /// <summary>One-line description of what the plugin does.</summary>
    string Description { get; }

    /// <summary>Who wrote it.</summary>
    string Author { get; }

    /// <summary>Project or documentation page, if there is one.</summary>
    Uri? MoreInfoUrl { get; }

    /// <summary>Plugin version, used for update checks and shown in the UI.</summary>
    Version Version { get; }

    /// <summary>Whether this plugin produces payloads or consumes them.</summary>
    PluginKind Kind { get; }

    /// <summary>Which runner architecture this plugin requires.</summary>
    PluginArchitecture Architecture { get; }

    /// <summary>
    /// The contract generation this plugin was built against. Compare with
    /// <see cref="SrtContract.Generation"/>.
    /// </summary>
    int ContractGeneration { get; }

    /// <summary>
    /// Whether the plugin needs a UI thread and message pump - true for anything that opens a window
    /// or draws an overlay. The runner starts a pump only when asked, so headless plugins pay nothing.
    /// </summary>
    bool RequiresUiThread { get; }
}
