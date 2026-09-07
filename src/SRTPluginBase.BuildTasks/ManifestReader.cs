using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

namespace SRTPluginBase.BuildTasks;

/// <summary>Raised when an assembly cannot be described as a plugin. Carries a diagnostic code.</summary>
internal sealed class ManifestException(string code, string message) : Exception(message)
{
    /// <summary>MSBuild diagnostic code, e.g. <c>SRT1001</c>.</summary>
    public string Code { get; } = code;
}

/// <summary>
/// Builds a <see cref="PluginManifest"/> from a compiled assembly using metadata only.
/// </summary>
/// <remarks>
/// <see cref="MetadataLoadContext"/> never runs a static constructor, a module initialiser or any
/// other plugin code, which is what makes this safe to do during a build - and what forced the
/// design: an <c>IPluginInfo</c> populated in an object initialiser is invisible here, so identity
/// moved to <c>[SrtPluginAssembly]</c> where it is a compile-time constant.
/// </remarks>
internal static class ManifestReader
{
    private const string AbstractionsNamespace = "SRTPluginBase.Abstractions";
    private const string MarkerAttribute = AbstractionsNamespace + ".SrtPluginAssemblyAttribute";

    /// <summary>
    /// Reads <paramref name="assemblyPath"/>, resolving its references from
    /// <paramref name="referencePaths"/>.
    /// </summary>
    /// <exception cref="ManifestException">The assembly is not a well-formed plugin.</exception>
    public static PluginManifest Read(string assemblyPath, IEnumerable<string> referencePaths)
    {
        List<string> paths = referencePaths
            .Concat([assemblyPath])
            .Where(static p => !string.IsNullOrEmpty(p) && File.Exists(p))
            // A PathAssemblyResolver throws on two paths carrying the same simple name, which the
            // reference set routinely contains (a facade and its implementation, most often).
            .GroupBy(static p => Path.GetFileNameWithoutExtension(p), StringComparer.OrdinalIgnoreCase)
            .Select(static g => g.First())
            .ToList();

        using MetadataLoadContext context = new(new PathAssemblyResolver(paths));

        Assembly assembly = context.LoadFromAssemblyPath(assemblyPath);

        CustomAttributeData marker =
            assembly.GetCustomAttributesData()
                .FirstOrDefault(a => a.AttributeType.FullName == MarkerAttribute)
            ?? throw new ManifestException(
                "SRT1001",
                $"'{Path.GetFileName(assemblyPath)}' has no [assembly: SrtPluginAssembly(id, type)], so no "
                + "manifest can be generated and the host will not recognise it as a plugin. Add the "
                + "attribute, or set <SrtGenerateManifest>false</SrtGenerateManifest> if this assembly is "
                + "a payload-contract or helper library rather than a plugin.");

        if (marker.ConstructorArguments.Count < 2
            || marker.ConstructorArguments[0].Value is not string id
            || marker.ConstructorArguments[1].Value is not Type entryType)
        {
            throw new ManifestException(
                "SRT1002",
                $"The [SrtPluginAssembly] on '{Path.GetFileName(assemblyPath)}' could not be read. It must "
                + "be applied as [assembly: SrtPluginAssembly(\"reverse.dns.id\", typeof(YourPlugin))].");
        }

        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ManifestException(
                "SRT1003",
                "The plugin id on [SrtPluginAssembly] is empty. It keys the plugin's configuration file, "
                + "its catalog entry and its subscriptions, and must be a stable "
                + "<Author>.<Subject>.<Name> string, e.g. \"SpeedRunTool.RE4R.Producer\".");
        }

        if (!IsWellFormedId(id))
        {
            throw new ManifestException(
                "SRT1007",
                $"The plugin id '{id}' is not usable. It becomes a file name - the host stores a "
                + "plugin's settings at config\\<id>.json - so it is limited to letters, digits, '.', "
                + "'_' and '-', and no segment between dots may be empty. Use the "
                + "<Author>.<Subject>.<Name> form, e.g. \"SpeedRunTool.RE4R.Producer\".");
        }

        AssemblyName name = assembly.GetName();

        return new PluginManifest
        {
            Id = id,
            Name = AttributeText(assembly, "System.Reflection.AssemblyProductAttribute")
                ?? name.Name
                ?? id,
            Description = AttributeText(assembly, "System.Reflection.AssemblyDescriptionAttribute") ?? string.Empty,
            Author = AttributeText(assembly, "System.Reflection.AssemblyCompanyAttribute") ?? string.Empty,
            Version = (name.Version ?? new Version(0, 0, 0, 0)).ToString(),
            ContractGeneration = GenerationOf(assembly, marker),
            Kind = KindOf(entryType, assemblyPath),
            Architecture = ArchitectureOf(marker, assemblyPath),
            RequiresUiThread = NamedArgument(marker, "RequiresUiThread") as bool? ?? false,
            EntryAssembly = Path.GetFileName(assemblyPath),
            EntryType = entryType.FullName ?? entryType.Name,
        };
    }

    /// <summary>
    /// Whether an id is safe to use as the file name it becomes.
    /// </summary>
    /// <remarks>
    /// The host keys a plugin's settings file on this id, so an id containing a separator or a
    /// <c>..</c> segment would let a plugin choose where its configuration is written. Catching it
    /// here fails the plugin author's own build with something they can act on, rather than the
    /// host's - though the host must still validate what it reads off disk, since a manifest beside
    /// a downloaded plugin is not something this generator ever saw.
    /// </remarks>
    private static bool IsWellFormedId(string id)
    {
        foreach (string segment in id.Split('.'))
        {
            if (segment.Length == 0)
                return false;

            foreach (char c in segment)
            {
                if (!char.IsLetterOrDigit(c) && c != '_' && c != '-')
                    return false;
            }
        }

        return true;
    }

    /// <summary>
    /// The explicit <c>Generation</c> override when there is one, otherwise the major version of the
    /// plugin's reference to the contracts assembly.
    /// </summary>
    /// <remarks>
    /// The mirror image of <c>SrtContract.GenerationOf</c>, which does exactly this at run time - see
    /// its remarks for why the generation cannot simply be a defaulted attribute property. Both read
    /// the same assembly reference, so a manifest and the plugin it describes cannot disagree.
    /// </remarks>
    private static int GenerationOf(Assembly assembly, CustomAttributeData marker)
    {
        if (NamedArgument(marker, "Generation") is int declared && declared != 0)
            return declared;

        int? viaBase = null;

        foreach (AssemblyName reference in assembly.GetReferencedAssemblies())
        {
            if (reference.Version is null)
                continue;

            if (string.Equals(reference.Name, "SRTPluginBase.Abstractions", StringComparison.OrdinalIgnoreCase))
                return reference.Version.Major;

            if (string.Equals(reference.Name, "SRTPluginBase", StringComparison.OrdinalIgnoreCase))
                viaBase = reference.Version.Major;
        }

        return viaBase ?? 0;
    }

    /// <summary>
    /// Producer or consumer, decided by which contract interface the entry type implements - so the
    /// author cannot declare one and implement the other.
    /// </summary>
    private static string KindOf(Type entryType, string assemblyPath)
    {
        bool producer = Implements(entryType, "IProducerPlugin");
        bool consumer = Implements(entryType, "IConsumerPlugin");

        if (producer && consumer)
        {
            throw new ManifestException(
                "SRT1004",
                $"'{entryType.FullName}' implements both IProducerPlugin and IConsumerPlugin. A plugin is "
                + "one or the other; split it into two plugins that share a payload-contract assembly.");
        }

        if (!producer && !consumer)
        {
            throw new ManifestException(
                "SRT1005",
                $"'{entryType.FullName}' in '{Path.GetFileName(assemblyPath)}' implements neither "
                + "IProducerPlugin nor IConsumerPlugin. Derive from ProducerPluginBase<T> or "
                + "ConsumerPluginBase, or point [SrtPluginAssembly] at the type that does.");
        }

        return producer ? "Producer" : "Consumer";
    }

    private static bool Implements(Type type, string interfaceName)
        => type.GetInterfaces().Any(i => i.Namespace == AbstractionsNamespace && i.Name == interfaceName);

    /// <summary>
    /// The attribute's <c>Architecture</c> override when it is not <c>Any</c>, otherwise whatever
    /// the PE header says.
    /// </summary>
    /// <remarks>
    /// Read straight from the header rather than through <see cref="Module.GetPEKind"/> so the
    /// answer does not depend on how a metadata-only reader chooses to summarise it. An AnyCPU
    /// assembly reports <see cref="Machine.I386"/> too, and is told apart by the absence of
    /// <see cref="CorFlags.Requires32Bit"/> - the distinction runner selection turns on.
    /// </remarks>
    private static string ArchitectureOf(CustomAttributeData marker, string assemblyPath)
    {
        // PluginArchitecture: 0 = Any, 1 = X86, 2 = X64.
        if (NamedArgument(marker, "Architecture") is int declared && declared != 0)
            return declared == 1 ? "X86" : "X64";

        using FileStream stream = File.OpenRead(assemblyPath);
        using PEReader reader = new(stream);

        PEHeaders headers = reader.PEHeaders;
        Machine machine = headers.CoffHeader.Machine;
        CorFlags flags = headers.CorHeader?.Flags ?? CorFlags.ILOnly;

        return machine switch
        {
            Machine.Amd64 or Machine.IA64 => "X64",
            Machine.I386 when (flags & CorFlags.Requires32Bit) != 0 => "X86",
            _ => "Any",
        };
    }

    private static string? AttributeText(Assembly assembly, string attributeFullName)
    {
        CustomAttributeData? data = assembly.GetCustomAttributesData()
            .FirstOrDefault(a => a.AttributeType.FullName == attributeFullName);

        string? value = data?.ConstructorArguments.Count > 0
            ? data.ConstructorArguments[0].Value as string
            : null;

        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private static object? NamedArgument(CustomAttributeData data, string name)
        => data.NamedArguments
            .Where(a => a.MemberName == name)
            .Select(a => a.TypedValue.Value)
            .FirstOrDefault();
}
