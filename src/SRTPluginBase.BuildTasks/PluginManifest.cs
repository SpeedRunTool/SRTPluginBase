using System.Globalization;
using System.Text;

namespace SRTPluginBase.BuildTasks;

/// <summary>
/// The shape of <c>srtplugin.json</c>: everything the host needs in order to describe a plugin, and
/// to decide which runner can execute it, <em>without loading the assembly</em>.
/// </summary>
/// <remarks>
/// That constraint is the whole reason this file exists. The host process is AnyCPU; loading an
/// x86 plugin into it to read its metadata is exactly the thing it must never do, and spawning a
/// probe runner per plugin just to populate a list is slow enough to be visible in the UI.
/// <para>
/// Every field here is derived from metadata that already exists in the compiled assembly - see
/// <see cref="ManifestReader"/> - and <c>PluginInfo.FromAssembly</c> reads the same sources at run
/// time, so the host's view of a plugin and the plugin's view of itself cannot drift apart.
/// </para>
/// <para>
/// Channels are deliberately absent. A producer's <c>PayloadChannelDescriptor</c> is a runtime
/// property, built in an object initialiser, so a metadata-only reader cannot see it - the same
/// limitation that moved plugin identity onto an attribute in the first place. The host learns the
/// channel list from the runner handshake, which is the only place it is knowable.
/// </para>
/// </remarks>
internal sealed class PluginManifest
{
    /// <summary>Manifest format version, so a future host can read an older manifest.</summary>
    public int SchemaVersion { get; set; } = 1;

    /// <summary>Reverse-DNS identity, from <c>[SrtPluginAssembly]</c>.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Display name, from <c>&lt;Product&gt;</c>.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>From <c>&lt;Description&gt;</c>.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>From <c>&lt;Authors&gt;</c>.</summary>
    public string Author { get; set; } = string.Empty;

    /// <summary>The assembly version.</summary>
    public string Version { get; set; } = "0.0.0.0";

    /// <summary>Contract generation the plugin was compiled against.</summary>
    public int ContractGeneration { get; set; }

    /// <summary><c>Producer</c> or <c>Consumer</c>, from the interface the entry type implements.</summary>
    public string Kind { get; set; } = string.Empty;

    /// <summary><c>Any</c>, <c>X86</c> or <c>X64</c> - the attribute override, else the PE header.</summary>
    public string Architecture { get; set; } = "Any";

    /// <summary>Whether the runner must start a UI thread and message pump for this plugin.</summary>
    public bool RequiresUiThread { get; set; }

    /// <summary>
    /// Reserved. Always <see langword="false"/> in generation 5: no runner declares
    /// <c>Microsoft.WindowsDesktop.App</c>, so a plugin needing WinForms or WPF cannot be hosted.
    /// The field is emitted now so a future desktop runner can be selected on it without a schema
    /// bump.
    /// </summary>
    public bool RequiresWindowsDesktop { get; set; }

    /// <summary>File name of the assembly holding the entry type.</summary>
    public string EntryAssembly { get; set; } = string.Empty;

    /// <summary>Full name of the type implementing <c>IPlugin</c>.</summary>
    public string EntryType { get; set; } = string.Empty;

    /// <summary>
    /// Serialises to indented JSON by hand.
    /// </summary>
    /// <remarks>
    /// Twelve flat scalar properties do not justify taking a dependency on System.Text.Json in a
    /// task assembly that has to load inside both the .NET Framework and the .NET MSBuild hosts;
    /// every assembly added here is one more that has to resolve in both.
    /// </remarks>
    public string ToJson()
    {
        StringBuilder sb = new StringBuilder()
            .AppendLine("{")
            .Append(Number("schemaVersion", SchemaVersion))
            .Append(Text("id", Id))
            .Append(Text("name", Name))
            .Append(Text("description", Description))
            .Append(Text("author", Author))
            .Append(Text("version", Version))
            .Append(Number("contractGeneration", ContractGeneration))
            .Append(Text("kind", Kind))
            .Append(Text("architecture", Architecture))
            .Append(Bool("requiresUiThread", RequiresUiThread))
            .Append(Bool("requiresWindowsDesktop", RequiresWindowsDesktop))
            .Append(Text("entryAssembly", EntryAssembly))
            .Append(Text("entryType", EntryType, last: true))
            .Append('}');

        return sb.ToString();
    }

    private static string Text(string name, string value, bool last = false)
        => Line(name, "\"" + Escape(value) + "\"", last);

    private static string Number(string name, int value)
        => Line(name, value.ToString(CultureInfo.InvariantCulture), last: false);

    private static string Bool(string name, bool value)
        => Line(name, value ? "true" : "false", last: false);

    private static string Line(string name, string literal, bool last)
        => "  \"" + name + "\": " + literal + (last ? string.Empty : ",") + Environment.NewLine;

    private static string Escape(string value)
    {
        StringBuilder sb = new(value.Length);

        foreach (char c in value)
        {
            switch (c)
            {
                case '"': sb.Append("\\\""); break;
                case '\\': sb.Append("\\\\"); break;
                case '\b': sb.Append("\\b"); break;
                case '\f': sb.Append("\\f"); break;
                case '\n': sb.Append("\\n"); break;
                case '\r': sb.Append("\\r"); break;
                case '\t': sb.Append("\\t"); break;
                default:
                    if (c < ' ')
                        sb.Append("\\u").Append(((int)c).ToString("x4", CultureInfo.InvariantCulture));
                    else
                        sb.Append(c);
                    break;
            }
        }

        return sb.ToString();
    }
}
