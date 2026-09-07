using System.Reflection;
using SRTPluginBase.Abstractions;

namespace SRTPluginBase;

/// <summary>
/// A ready-made <see cref="IPluginInfo"/>. Construct one with an object initialiser and return the
/// same instance every time from <see cref="IPlugin.Info"/>.
/// </summary>
/// <remarks>
/// A record so equality is structural and it is cheap to hold. Several 3.x plugins allocated a fresh
/// info object on every property read, which happened once per frame in the overlays.
/// </remarks>
public sealed record PluginInfo : IPluginInfo
{
    /// <inheritdoc />
    public required string Id { get; init; }

    /// <inheritdoc />
    public required string Name { get; init; }

    /// <inheritdoc />
    public string Description { get; init; } = string.Empty;

    /// <inheritdoc />
    public string Author { get; init; } = string.Empty;

    /// <inheritdoc />
    public Uri? MoreInfoUrl { get; init; }

    /// <inheritdoc />
    public required Version Version { get; init; }

    /// <inheritdoc />
    public required PluginKind Kind { get; init; }

    /// <inheritdoc />
    public PluginArchitecture Architecture { get; init; } = PluginArchitecture.Any;

    /// <inheritdoc />
    public int ContractGeneration { get; init; } = SrtContract.Generation;

    /// <inheritdoc />
    public bool RequiresUiThread { get; init; }

    /// <summary>
    /// Reads the version from <paramref name="assembly"/> so it is declared once, in the project
    /// file, rather than duplicated in code and left to drift.
    /// </summary>
    public static Version VersionOf(Assembly assembly)
        => assembly.GetName().Version ?? new Version(0, 0, 0, 0);

    /// <summary>
    /// Builds a <see cref="PluginInfo"/> entirely from <paramref name="assembly"/>'s metadata and
    /// its <see cref="SrtPluginAssemblyAttribute"/>.
    /// </summary>
    /// <remarks>
    /// Prefer this over writing the properties out by hand. The host derives a plugin's manifest
    /// from exactly these same sources without executing any plugin code, so using this method
    /// guarantees that what the host believes about a plugin and what the plugin reports about
    /// itself cannot drift apart.
    /// <para>
    /// The values come from ordinary project properties: <c>&lt;Product&gt;</c> for the name,
    /// <c>&lt;Description&gt;</c>, <c>&lt;Authors&gt;</c>, and the assembly version.
    /// </para>
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// The assembly has no <see cref="SrtPluginAssemblyAttribute"/>.
    /// </exception>
    public static PluginInfo FromAssembly(Assembly assembly, Uri? moreInfoUrl = null)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        SrtPluginAssemblyAttribute marker =
            assembly.GetCustomAttribute<SrtPluginAssemblyAttribute>()
            ?? throw new InvalidOperationException(
                $"Assembly '{assembly.GetName().Name}' is missing [assembly: SrtPluginAssembly(id, type)].");

        bool isProducer = typeof(IProducerPlugin).IsAssignableFrom(marker.PluginType);

        return new PluginInfo
        {
            Id = marker.Id,
            Name = assembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product
                ?? assembly.GetName().Name
                ?? marker.Id,
            Description = assembly.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description ?? string.Empty,
            Author = assembly.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company ?? string.Empty,
            MoreInfoUrl = moreInfoUrl,
            Version = VersionOf(assembly),
            Kind = isProducer ? PluginKind.Producer : PluginKind.Consumer,
            Architecture = marker.Architecture,
            ContractGeneration = marker.Generation != 0 ? marker.Generation : SrtContract.GenerationOf(assembly),
            RequiresUiThread = marker.RequiresUiThread,
        };
    }
}
