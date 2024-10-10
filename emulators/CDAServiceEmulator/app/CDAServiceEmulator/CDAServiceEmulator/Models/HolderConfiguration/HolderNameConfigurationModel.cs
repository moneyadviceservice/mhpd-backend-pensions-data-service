using System.Text.Json.Serialization;

namespace CDAServiceEmulator.Models.HolderConfiguration
{
    public class HolderNameConfigurationModel
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("holdernameGuid")]
        public string? HolderNameGuid { get; set; }

        [JsonPropertyName("viewDataUrl")]
        public string? ViewDataUrl { get; set; }
    }
}