using System.Text.Json.Serialization;

namespace MhpdCommon.Models.MessageBodyModels;

public class PensionRetrievalPayload
{
    [JsonPropertyName("peisId")]
    public string? PeisId { get; set; }
        
    [JsonPropertyName("iss")]
    public string? Iss { get; set; }
        
    [JsonPropertyName("userSessionId")]
    public string? UserSessionId { get; set; }
}