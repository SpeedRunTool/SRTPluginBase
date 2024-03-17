using System.Collections.Generic;

namespace SRTPluginBase.Interfaces
{
    public interface IMainJson
    {
        #region Serialized Properties

        /// <summary>
        /// An array of plugin hosts.
        /// </summary>
        public IEnumerable<IMainHostEntry> Hosts { get; set; }

        /// <summary>
        /// An array of plugins.
        /// </summary>
        public IEnumerable<IMainPluginEntry> Plugins { get; set; }

        #endregion
    }
}
