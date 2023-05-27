using System;
using System.Collections.Generic;

namespace SRTPluginBase
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    internal class MasterJson
    {
        internal MasterHostEntry Host { get; set; }
        internal IEnumerable<MasterPluginEntry> Plugins { get; set; }
    }

    internal class MasterHostEntry
    {
        internal Uri ManifestURL { get; set; }
    }

    internal class MasterPluginEntry
    {
        internal string Name { get; set; }
        internal MasterPluginPlatformEnum Platform { get; set; }
        internal MasterPluginTypeEnum Type { get; set; }
        internal Uri ManifestURL { get; set; }
    }

    internal enum MasterPluginPlatformEnum : int
    {
        Any = 0, // AnyCPU
        x86 = 1, // x86 (32-bit)
        x64 = 2, // x64 (64-bit)
    }

    internal enum MasterPluginTypeEnum : int
    {
        Producer = 0,
        Consumer = 1,
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}
