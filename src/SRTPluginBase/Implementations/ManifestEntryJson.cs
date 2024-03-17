using System.Collections.Generic;
using SRTPluginBase.Interfaces;

namespace SRTPluginBase.Implementations
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public class ManifestEntryJson : IManifestEntryJson
    {
        public IEnumerable<string> Contributors { get; set; }
        public IEnumerable<IManifestReleaseJson> Releases { get; set; }
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}
