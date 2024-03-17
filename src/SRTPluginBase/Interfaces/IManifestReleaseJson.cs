using System;

namespace SRTPluginBase.Interfaces
{
    public interface IManifestReleaseJson
    {
        #region Serialized Properties

        /// <summary>
        /// The version of this entry.
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// The download Uri for this version of the entry.
        /// </summary>
        public Uri DownloadURL { get; set; }

        #endregion
    }
}
