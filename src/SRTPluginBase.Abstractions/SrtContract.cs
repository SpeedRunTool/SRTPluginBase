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
}
