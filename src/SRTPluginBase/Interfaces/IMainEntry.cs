using System;

namespace SRTPluginBase.Interfaces
{
    public interface IMainEntry
    {
        #region Serialized Properties

        /// <summary>
        /// The unique name for this entry. Must not contain characters not suitable for file and folder paths.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The user-friendly display name for this entry.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// A description of what this entry is for.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The URL to this entry's source control repository.
        /// </summary>
        public Uri RepoURL { get; set; }

        /// <summary>
        /// The URL to this entry's manifest json file.
        /// </summary>
        public Uri ManifestURL { get; set; }

        #endregion
    }
}
