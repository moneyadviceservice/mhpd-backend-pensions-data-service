using System.Text.Json.Serialization;
namespace TokenIntegrationService.Models
{
    public class RptsModel
    {
        [JsonPropertyName("rpt")]
        public string? Rpt { get; set; }
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }

        [JsonPropertyName("token_type")]
        public string? TokenType { get; set; }

        [JsonPropertyName("upgraded")]
        public bool Upgraded { get; set; }
        public string? RetrievalStatus { get; set; } = null;
        public DateTime? RetrievalRequestedTimestamp { get; set; } = null;
    }
}
