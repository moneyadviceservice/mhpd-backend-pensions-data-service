using MhpdCommon.Constants;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace MhpdCommon.Models.MHPDModels;

public class PeiDataModel
{
    [RegularExpression(ApiConstants.PeiPattern)]
    [JsonPropertyName("pei")]
    public string Pei { get; set; } = string.Empty;

    [MinLength(1)]
    [JsonPropertyName("description")]
    public string? Description { get; set; } = null;

    [MinLength(1)]
    [JsonPropertyName("retrievalStatus")]
    public string? RetrievalStatus { get; set; } = null;

    [MinLength(1)]
    [JsonPropertyName("retrievalRequestedTimestamp")]
    public DateTime? RetrievalRequestedTimestamp { get; set; } = null;
}
