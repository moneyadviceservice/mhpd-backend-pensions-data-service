using System.Text.Json.Serialization;
using MhpdCommon.Constants.HttpClient;

namespace MhpdCommon.Models.MessageBodyModels;

public class PensionsDataRequestModel
{
    [JsonPropertyName(QueryParams.AuthorisationCode)]
    public string? AuthorisationCode { get; set; }
    
    [JsonPropertyName(QueryParams.RedirectUri)]
    public string? RedirectUri { get; set; }
    
    [JsonPropertyName(QueryParams.CodeVerifier)]
    public string? CodeVerifier { get; set; }
}