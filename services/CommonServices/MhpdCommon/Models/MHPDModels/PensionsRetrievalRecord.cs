using System.Text.Json.Serialization;

namespace MhpdCommon.Models.MHPDModels;

public class PensionsRetrievalRecord
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("userSessionId")]
    public string UserSessionId { get; set; } = string.Empty;

    [JsonPropertyName("iss")]
    public string Iss { get; set; } = string.Empty;

    [JsonPropertyName("jobStartTimestamp")]
    public DateTime JobStartTimestamp { get; set; }

    [JsonIgnore]
    public string? PeisRpt { get; set; }

    [JsonPropertyName("peisId")]
    public string PeisId { get; set; } = string.Empty;

    [JsonPropertyName("peiRetrievalComplete")]
    public bool PeiRetrievalComplete { get; set; }

    [JsonPropertyName("peiData")]
    public List<PeiDataModel> PeiData { get; set; } = [];
}
