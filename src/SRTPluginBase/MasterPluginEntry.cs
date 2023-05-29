using System;

namespace SRTPluginBase
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public class MasterPluginEntry
    {
        public string Name { get; set; }
        public MasterPluginPlatformEnum Platform { get; set; }
        public MasterPluginTypeEnum Type { get; set; }
        public Uri ManifestURL { get; set; }
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}
