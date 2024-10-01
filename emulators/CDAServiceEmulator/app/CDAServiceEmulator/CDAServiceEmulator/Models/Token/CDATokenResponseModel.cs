using System.Text.Json.Serialization;

namespace CDAServiceEmulator.Models.Token;

public class CdaTokenResponseModel
{
    [JsonPropertyName("access_token")]
    public required string AccessToken { get; set; }

    [JsonPropertyName("token_type")]
    public required string TokenType { get; set; }

    [JsonPropertyName("upgraded")]
    public bool Upgraded { get; set; }

    [JsonPropertyName("pct")]
    public string? Pct { get; set; }
    
    [JsonPropertyName("id_token")]
    public string? IdToken { get; set; }
}