using System.Text.Json.Serialization;

namespace PeIsServiceEmulator.Models.Peis
{
    public class PeiModel
    {
        [JsonPropertyName("pei")]
        public string? Pei { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

    }
}
