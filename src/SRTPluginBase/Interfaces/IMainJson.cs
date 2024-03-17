using System.Collections.Generic;
using SRTPluginBase.Implementations;

namespace SRTPluginBase.Interfaces
{
    public interface IMainJson
    {
        #region Serialized Properties

        /// <summary>
        /// An array of plugin hosts.
        /// </summary>
        public IEnumerable<MainHostEntry> Hosts { get; set; }

        /// <summary>
        /// An array of plugins.
        /// </summary>
        public IEnumerable<MainPluginEntry> Plugins { get; set; }

        #endregion
    }
}
