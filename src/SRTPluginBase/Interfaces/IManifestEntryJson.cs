using System.Collections.Generic;
using SRTPluginBase.Implementations;

namespace SRTPluginBase.Interfaces
{
    public interface IManifestEntryJson
    {
        #region Serialized Properties

        /// <summary>
        /// The contributors to this entry.
        /// </summary>
        public IEnumerable<string> Contributors { get; set; }

        /// <summary>
        /// An array of releases for this entry.
        /// </summary>
        public IEnumerable<ManifestReleaseJson> Releases { get; set; }

        #endregion
    }
}
