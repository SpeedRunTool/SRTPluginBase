using System.Collections.Generic;

namespace SRTPluginBase
{
    internal class MasterJson
    {
        internal MasterHostEntry Host { get; set; }
        internal IEnumerable<MasterPluginEntry> Plugins { get; set; }
    }

    internal class MasterHostEntry
    {
        internal string ManifestURL { get; set; }
    }

    internal class MasterPluginEntry
    {
        internal string Name { get; set; }
        internal MasterPluginPlatformEnum Platform { get; set; }
        internal MasterPluginTypeEnum Type { get; set; }
        internal IEnumerable<string> Categories { get; set; }
        internal string ManifestURL { get; set; }
    }

    internal enum MasterPluginPlatformEnum : int
    {
        Any, // 0
        x86, // 1
        x64, // 2
    }

    internal enum MasterPluginTypeEnum : int
    {
        Producer, // 0
        Consumer, // 1
    }
}
