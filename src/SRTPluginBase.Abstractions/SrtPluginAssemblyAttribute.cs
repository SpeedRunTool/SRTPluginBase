namespace SRTPluginBase.Abstractions;

/// <summary>
/// Marks the assembly's plugin entry point.
/// </summary>
/// <remarks>
/// Lets the runner find the plugin type directly instead of scanning every exported type for one
/// that implements <see cref="IPlugin"/>, and - more importantly - lets the host read a plugin's
/// identity, kind and architecture from metadata alone, without loading the assembly to run it.
/// That matters because the host process is AnyCPU and must never load a plugin built for a
/// specific bitness just to find out which runner to start.
/// <para>
/// Everything else the host needs is already in assembly metadata, so nothing is declared twice:
/// the display name comes from <c>&lt;Product&gt;</c>, the description from <c>&lt;Description&gt;</c>,
/// the author from <c>&lt;Authors&gt;</c>, the version from the assembly version, the kind from which
/// of <see cref="IProducerPlugin"/> or <see cref="IConsumerPlugin"/> the type implements, and the
/// architecture from the PE header. Only the id has to be stated, because an identity should never
/// be inferred.
/// </para>
/// Apply it once, in the plugin assembly:
/// <code>
/// [assembly: SrtPluginAssembly("com.example.mygame.producer", typeof(MyProducer))]
/// </code>
/// </remarks>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
public sealed class SrtPluginAssemblyAttribute(string id, Type pluginType) : Attribute
{
    /// <summary>
    /// The plugin's stable identity, in reverse-DNS form. See <see cref="IPluginInfo.Id"/>.
    /// </summary>
    /// <remarks>
    /// Declared here, as a compile-time constant, rather than only on <see cref="IPluginInfo"/>,
    /// because the host must be able to read it without loading the assembly for execution - see the
    /// remarks on the attribute itself.
    /// </remarks>
    public string Id { get; } = id;

    /// <summary>The type implementing <see cref="IPlugin"/>.</summary>
    public Type PluginType { get; } = pluginType;

    /// <summary>
    /// The contract generation this plugin targets. Defaults to whatever it compiled against, which
    /// is nearly always what you want.
    /// </summary>
    public int Generation { get; init; } = SrtContract.Generation;

    /// <summary>
    /// Overrides the architecture inferred from the assembly's PE header. Only needed when a plugin
    /// is compiled AnyCPU but still requires a particular bitness - for example a producer that
    /// reads a 32-bit game through an AnyCPU helper library.
    /// </summary>
    public PluginArchitecture Architecture { get; init; } = PluginArchitecture.Any;

    /// <summary>
    /// Whether the plugin needs a UI thread and message pump. See
    /// <see cref="IPluginInfo.RequiresUiThread"/>.
    /// </summary>
    public bool RequiresUiThread { get; init; }
}

/// <summary>
/// Marks a payload type as safe to send with <see cref="PayloadCodec.Blittable"/> - copied as raw
/// memory, with no serialisation at all.
/// </summary>
/// <remarks>
/// The type must be an unmanaged struct with <see cref="System.Runtime.InteropServices.LayoutKind.Sequential"/>
/// layout: no references, no strings, no variable-length arrays. The base classes verify this at
/// startup and refuse to publish otherwise, and each frame carries a layout hash so a consumer built
/// against a different version of the struct fails loudly instead of reading misaligned garbage.
/// <para>Reach for this only if profiling says JSON is too slow; JSON is the sane default.</para>
/// </remarks>
[AttributeUsage(AttributeTargets.Struct, AllowMultiple = false)]
public sealed class SrtBlittablePayloadAttribute : Attribute;
