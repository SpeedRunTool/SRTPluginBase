using Microsoft.Extensions.DependencyInjection;
using SRTPluginBase.Abstractions;

namespace SRTPluginBase;

/// <summary>
/// Conveniences over <see cref="IPluginContext.Services"/>.
/// </summary>
/// <remarks>
/// These live here rather than in the Abstractions package so that Abstractions can stay free of
/// package references; see the note in its project file.
/// </remarks>
public static class ServiceProviderExtensions
{
    /// <summary>Resolves a required service from the plugin context.</summary>
    public static T GetRequiredService<T>(this IPluginContext context)
        where T : notnull
    {
        ArgumentNullException.ThrowIfNull(context);
        return context.Services.GetRequiredService<T>();
    }

    /// <summary>Resolves an optional service from the plugin context.</summary>
    public static T? GetService<T>(this IPluginContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return context.Services.GetService<T>();
    }
}
