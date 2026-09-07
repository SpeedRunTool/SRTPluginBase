namespace SRTPluginBase.Abstractions;

/// <summary>
/// Identifies the plugin contract generation this assembly defines.
/// </summary>
public static class SrtContract
{
    /// <summary>
    /// The contract generation. A host refuses to load a plugin declaring a different value.
    /// </summary>
    /// <remarks>
    /// Deliberately equal to the SRT Host major version, so "SRT Host 5" and "contract generation 5"
    /// are the same statement. Generations 1 to 4 were the vendored <c>IPluginProvider</c>/<c>IPluginUI</c>
    /// contracts and the never-stabilised 4.x/5.x pre-releases; none of them are loadable here.
    /// </remarks>
    public const int Generation = 5;

    /// <summary>The simple name of the assembly whose version pins a plugin's generation.</summary>
    public const string AbstractionsAssemblyName = "SRTPluginBase.Abstractions";

    /// <summary>
    /// The generation <paramref name="assembly"/> was compiled against, taken from the major version
    /// of its reference to this assembly.
    /// </summary>
    /// <remarks>
    /// It has to be read this way rather than from a constant, and the reason is worth stating
    /// because the obvious alternative is wrong in a way that is invisible.
    /// <para>
    /// A default of <see cref="Generation"/> written on a property of
    /// <see cref="SrtPluginAssemblyAttribute"/> is compiled into <em>this</em> assembly, not into the
    /// plugin, so it is evaluated when the host constructs the attribute and yields the <em>host's</em>
    /// generation for every plugin - which makes a mismatch report itself as a match. It is also
    /// absent from metadata entirely, so the build-time manifest generator cannot see it at all.
    /// </para>
    /// <para>
    /// The assembly reference, by contrast, is baked into the plugin at compile time, is readable
    /// without executing anything, and reads the same here as it does through a
    /// <c>MetadataLoadContext</c> during the build - which is what keeps the manifest and the running
    /// plugin from disagreeing. It is exact because <c>AssemblyVersion</c> is pinned to
    /// <c>&lt;major&gt;.0.0.0</c> for the life of a generation, by the same decision that makes host
    /// major, package major and contract generation one number.
    /// </para>
    /// </remarks>
    /// <returns>The generation, or <see cref="Generation"/> if no such reference is present.</returns>
    public static int GenerationOf(System.Reflection.Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        int? viaBase = null;

        foreach (System.Reflection.AssemblyName reference in assembly.GetReferencedAssemblies())
        {
            if (reference.Version is null)
                continue;

            if (string.Equals(reference.Name, AbstractionsAssemblyName, StringComparison.OrdinalIgnoreCase))
                return reference.Version.Major;

            // Fallback for the rare plugin that touches no Abstractions type directly and so has no
            // reference to it: the two packages ship in lockstep on one version.
            if (string.Equals(reference.Name, "SRTPluginBase", StringComparison.OrdinalIgnoreCase))
                viaBase = reference.Version.Major;
        }

        return viaBase ?? Generation;
    }
}
