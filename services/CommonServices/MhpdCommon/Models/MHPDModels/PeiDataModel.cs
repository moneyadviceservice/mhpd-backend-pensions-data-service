using System.Text.Json.Serialization;

namespace MhpdCommon.Models.MHPDModels;

public class PeiDataModel
{
    [JsonPropertyName("pei")]
    public string? Pei { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; } = null;

    [JsonPropertyName("retrievalStatus")]
    public string? RetrievalStatus { get; set; } = null;

    [JsonPropertyName("retrievalRequestedTimestamp")]
    public DateTime? RetrievalRequestedTimestamp { get; set; } = null;
}
