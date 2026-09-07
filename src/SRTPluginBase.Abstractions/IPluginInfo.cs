namespace SRTPluginBase.Abstractions;

/// <summary>
/// Identity and metadata for a plugin. The host reads this to decide which runner to start, whether
/// the plugin is loadable at all, and what to show in the UI.
/// </summary>
public interface IPluginInfo
{
    /// <summary>
    /// Stable, unique, immutable identity, shaped like a NuGet package id -
    /// <c>&lt;Author&gt;.&lt;Subject&gt;.&lt;Name&gt;</c>, e.g. <c>SpeedRunTool.RE4R.Producer</c>.
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
    /// <para>
    /// The convention is the one .NET already uses for assemblies, namespaces and package ids, rather
    /// than the reverse-DNS form Java and Apple settled on. Uniqueness comes from the leading author
    /// segment, so that segment names <em>you</em>: a third-party plugin is
    /// <c>JohnDoe.RE4R.Overlay</c>, never <c>SRT.</c>-prefixed, which would read as first-party.
    /// </para>
    /// <para>
    /// Nothing parses this. The trailing segment in particular is a free-form distinguisher and not a
    /// declaration of <see cref="Kind"/>, which is derived from the interface the plugin implements
    /// precisely so it cannot be stated wrongly - and an id that never changes is the last place that
    /// should restate a derived fact. Naming a plugin <c>...Producer</c> because it is one is fine;
    /// anything relying on that being true is not.
    /// </para>
    /// <para>
    /// It also becomes a file name, so it is limited to letters, digits, <c>.</c>, <c>_</c> and
    /// <c>-</c>, with no empty segments.
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
