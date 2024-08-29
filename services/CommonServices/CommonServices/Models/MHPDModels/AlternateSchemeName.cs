using System.Text.Json.Serialization;

namespace CommonServices.Models
{
    public class AlternateSchemeName
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("alternateNameType")]
        public string? AlternateNameType { get; set; }
    }
}