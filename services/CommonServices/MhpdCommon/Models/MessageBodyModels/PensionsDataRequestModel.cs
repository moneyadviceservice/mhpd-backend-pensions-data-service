using System.Text.Json.Serialization;
using MhpdCommon.Constants.HttpClient;

namespace MhpdCommon.Models.MessageBodyModels;

public class PensionsDataRequestModel
{
    [JsonPropertyName(QueryParams.Cda.Token.AuthorisationCode)]
    public string? AuthorisationCode { get; set; }
    
    [JsonPropertyName(QueryParams.Cda.Token.RedirectUri)]
    public string? RedirectUri { get; set; }
    
    [JsonPropertyName(QueryParams.Cda.Token.CodeVerifier)]
    public string? CodeVerifier { get; set; }
}