using System.Text.Json.Serialization;

namespace PeiIntegrationService.Models.CdaPiesService
{
    public class PeiModel
    {
        [JsonPropertyName("pei")]
        public string? Pei { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; } = null;

        public string? RetrievalStatus { get; set; } = null;

        public DateTime? RetrievalRequestedTimestamp { get; set; } = null;
    }
}
