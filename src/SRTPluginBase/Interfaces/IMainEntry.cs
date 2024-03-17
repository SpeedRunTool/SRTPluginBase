using System.Net.Http;
using System.Threading.Tasks;
using System;
using System.Text.Json.Serialization;
using SRTPluginBase.Implementations;

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

        #region Non-serialized Properties

        /// <summary>
        /// The manifest file's contents deserialized from the ManifestURL.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public ManifestEntryJson? Manifest { get; }

        #endregion

        /// <summary>
        /// Deserializes the manifest located at the Uri ManifestURL.
        /// </summary>
        /// <param name="client">An HttpClient instance to use when retrieving the manifest json.</param>
        /// <returns>A asynchronous Task instance for this request.</returns>
        public abstract Task SetManifestAsync(HttpClient client);
        
    }
}
