using System;
using System.Text.Json.Serialization;

namespace SRTPluginBase.Interfaces
{
    public interface IPluginInfo : IEquatable<IPluginInfo>
    {
        /// <summary>
        /// The name of the plugin.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The description of what this plugin does.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// The author of the plugin.
        /// </summary>
        string Author { get; }

        /// <summary>
        /// A URI for the user to get more info.
        /// </summary>
        Uri MoreInfoURL { get; }

        /// <summary>
        /// The major version of this plugin.
        /// </summary>
        int VersionMajor { get; }

        /// <summary>
        /// The minor version of this plugin.
        /// </summary>
        int VersionMinor { get; }

        /// <summary>
        /// The build version of this plugin.
        /// </summary>
        int VersionBuild { get; }

        /// <summary>
        /// The revision version of this plugin.
        /// </summary>
        int VersionRevision { get; }

        /// <summary>
        /// The version of this plugin.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        Version Version { get; }

        new bool Equals(IPluginInfo? other) =>
            Name == other?.Name &&
            Description == other?.Description &&
            Author == other?.Author &&
            MoreInfoURL == other?.MoreInfoURL &&
            VersionMajor == other?.VersionMajor &&
            VersionMinor == other?.VersionMinor &&
            VersionBuild == other?.VersionBuild &&
            VersionRevision == other?.VersionRevision;

        bool Equals(object? obj) => Equals(obj as IPluginInfo);

        int GetHashCode() => HashCode.Combine(Name, Description, Author, MoreInfoURL, VersionMajor, VersionMinor, VersionBuild, VersionRevision);
    }
}
