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
/// [assembly: SrtPluginAssembly("JohnDoe.MyGame.Producer", typeof(MyProducer))]
/// </code>
/// </remarks>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
public sealed class SrtPluginAssemblyAttribute(string id, Type pluginType) : Attribute
{
    /// <summary>
    /// The plugin's stable identity, shaped <c>&lt;Author&gt;.&lt;Subject&gt;.&lt;Name&gt;</c>. See
    /// <see cref="IPluginInfo.Id"/> for the convention and the constraints.
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
    /// Overrides the contract generation. Leave it alone: zero means "whatever this plugin compiled
    /// against", which <see cref="SrtContract.GenerationOf"/> reads from the assembly reference.
    /// </summary>
    /// <remarks>
    /// Deliberately <em>not</em> defaulted to <see cref="SrtContract.Generation"/>. That default is
    /// compiled into the Abstractions assembly rather than into the plugin, so it would be evaluated
    /// against whichever copy the host had loaded and every plugin would report the host's own
    /// generation - turning the compatibility check into a tautology. It would also be invisible to
    /// the build-time manifest generator, which reads metadata and never runs a constructor.
    /// </remarks>
    public int Generation { get; init; }

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
/// layout: no references, no strings, no variable-length arrays.
/// <para>
/// <strong>Reserved: nothing consumes this attribute yet.</strong> <see cref="PayloadCodec.Blittable"/>
/// travels on the wire and a consumer is told which codec a frame carries, but the producer base
/// classes publish JSON only - a producer wanting raw memory implements
/// <see cref="IProducerPlugin.TryProduceAsync"/> itself and takes responsibility for layout
/// agreement between the two sides. The intended safeguard is a per-frame layout hash, so that a
/// consumer built against a different version of the struct fails loudly instead of reading
/// misaligned garbage; until that exists, a version skew here is silent corruption.
/// </para>
/// <para>Reach for this only if profiling says JSON is too slow; JSON is the sane default.</para>
/// </remarks>
[AttributeUsage(AttributeTargets.Struct, AllowMultiple = false)]
public sealed class SrtBlittablePayloadAttribute : Attribute;
