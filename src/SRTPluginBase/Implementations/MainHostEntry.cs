using System;
using System.Net.Http;
using System.Threading.Tasks;
using SRTPluginBase.Interfaces;

namespace SRTPluginBase.Implementations
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public class MainHostEntry : IMainHostEntry
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public Uri RepoURL { get; set; }
        public Uri ManifestURL { get; set; }

        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.Always)]
        public ManifestEntryJson? Manifest { get; private set; }
        public async Task SetManifestAsync(HttpClient client) => Manifest = await client.GetSRTJsonAsync<ManifestEntryJson>(ManifestURL);
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}
