using System.Text.Json.Serialization;

namespace MhpdCommon.Models.MessageBodyModels;

public class PensionRequestPayload
{
    [JsonPropertyName("pensionRetrievalRecordId")]
    public string? PensionRetrievalRecordId { get; set; }

    [JsonPropertyName("pei")]
    public string? Pei { get; set; }

    [JsonPropertyName("iss")]
    public string? Iss { get; set; }

    [JsonPropertyName("userSessionId")]
    public string? UserSessionId { get; set; }
}
