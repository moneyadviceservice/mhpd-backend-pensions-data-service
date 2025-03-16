using System.Text.Json.Serialization;
using MhpdCommon.Constants;

namespace MhpdCommon.Models.MHPDModels;

public class UserSessionData
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName(HeaderConstants.UserSessionId)]
    public required string UserSessionId { get; set; }
    
    [JsonPropertyName(HeaderConstants.PeisId)]
    public required string PeisId { get; set; }
}