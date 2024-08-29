using System.Text.Json.Serialization;

namespace CDATokenServices.Models
{
    public class CDATokenResponseModel
    {
        [JsonPropertyName("access_token")]
        public required string AccessToken { get; set; }

        [JsonPropertyName("token_type")]
        public required string TokenType { get; set; }

        [JsonPropertyName("upgraded")]
        public bool Upgraded { get; set; }

        [JsonPropertyName("pct")]
        public string? Pct { get; set; }

    }
}
