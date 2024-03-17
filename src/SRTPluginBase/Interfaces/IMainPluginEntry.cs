using System.Collections.Generic;
using SRTPluginBase.Implementations;

namespace SRTPluginBase.Interfaces
{
    public interface IMainPluginEntry : IMainEntry
    {
        #region Serialized Properties

        /// <summary>
        /// A enumeration value indicating what platform architecture this plugin targets.
        /// </summary>
        public MainPluginPlatformEnum Platform { get; set; }

        /// <summary>
        /// A enumeration value indicating which type of plugin this is.
        /// </summary>
        public MainPluginTypeEnum Type { get; set; }

        /// <summary>
        /// A list of tags to assist in filtering and sorting. Examples for a DirectX Consumer plugin might include { "Consumer", "UI", "Overlay", "DirectX" }.
        /// </summary>
        public IEnumerable<string> Tags { get; set; }

        #endregion
    }
}
