using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json.Serialization;

namespace SRTPluginBase
{
    public abstract class PluginInfoBase : IPluginInfo
    {
        public PluginInfoBase()
        {
            Version = GetProductVersion();
        }

        public abstract string Name { get; }
        public abstract string Description { get; }
        public abstract string Author { get; }
        public abstract Uri MoreInfoURL { get; }
        public abstract int VersionMajor { get; }
        public abstract int VersionMinor { get; }
        public abstract int VersionBuild { get; }
        public abstract int VersionRevision { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public Version Version { get; }

        protected string GetPluginLocation() => GetType().Assembly.Location;

        protected Version GetProductVersion() => new Version(FileVersionInfo.GetVersionInfo(GetPluginLocation()).ProductVersion);
        protected Version GetFileVersion() => new Version(FileVersionInfo.GetVersionInfo(GetPluginLocation()).FileVersion);
        protected Version GetAssemblyVersion() => System.Reflection.Assembly.ReflectionOnlyLoadFrom(GetPluginLocation()).GetName().Version;

        public virtual bool Equals(IPluginInfo? other) => Equals(this, other);
    }
}
