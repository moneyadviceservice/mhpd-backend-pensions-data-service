using System.Text.Json.Serialization;
using MhpdCommon.Constants;
using MhpdCommon.Constants.HttpClient;

namespace MhpdCommon.Models.MHPDModels;

public class UserSessionData
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName(HeaderConstants.UserSessionId)]
    public required string UserSessionId { get; set; }
    
    [JsonPropertyName(HeaderConstants.PeisId)]
    public string? PeisId { get; set; }
    
    // Persisted claims token
    [JsonPropertyName(QueryParams.Cda.Token.Pct)]
    public string? Pct { get; set; }
    
    public string? AccessToken { get; set; }
}