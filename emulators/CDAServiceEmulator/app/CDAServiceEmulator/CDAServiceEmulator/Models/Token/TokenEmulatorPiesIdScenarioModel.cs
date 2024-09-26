using System.Text.Json.Serialization;

namespace CDAServiceEmulator.Models.Token;

public class TokenEmulatorPiesIdScenarioModel
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
    
    [JsonPropertyName("code")]
    public string? Code { get; set; }
    
    [JsonPropertyName("peisIdStartCode")]
    public string? PeisIdStartCode { get; set; }
}