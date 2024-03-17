using System.Collections.Generic;

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
        public IEnumerable<IManifestReleaseJson> Releases { get; set; }

        #endregion
    }
}
