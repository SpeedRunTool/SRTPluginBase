using System.Collections.Generic;
using SRTPluginBase.Interfaces;

namespace SRTPluginBase.Implementations
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public class MainJson : IMainJson
    {
        public IEnumerable<IMainHostEntry> Hosts { get; set; }
        public IEnumerable<IMainPluginEntry> Plugins { get; set; }
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}
