using System.Text.Json.Serialization;

namespace CDAServiceEmulator.Models.Peis;

public class CdaPeisEmulatorTestInstanceDataModel
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
    
    [JsonPropertyName("peis_id")]
    public string? PeisId { get; set; }
    
    [JsonPropertyName("initialCallTimestamp")]
    public DateTimeOffset InitialCallTimestamp { get; set; }
}