using System.Collections.Generic;
using SRTPluginBase.Interfaces;

namespace SRTPluginBase.Implementations
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public class MainJson : IMainJson
    {
        public IEnumerable<MainHostEntry> Hosts { get; set; }
        public IEnumerable<MainPluginEntry> Plugins { get; set; }
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}
