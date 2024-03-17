using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using SRTPluginBase.Interfaces;

namespace SRTPluginBase.Implementations
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public class MainPluginEntry : IMainPluginEntry
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public MainPluginPlatformEnum Platform { get; set; }
        public MainPluginTypeEnum Type { get; set; }
        public IEnumerable<string> Tags { get; set; }
        public Uri RepoURL { get; set; }
        public Uri ManifestURL { get; set; }

        public async Task SetManifestAsync(HttpClient client) => Manifest = await client.GetSRTJsonAsync<ManifestEntryJson>(ManifestURL);
        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public ManifestEntryJson? Manifest { get; private set; }
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}
