using System.Text.Json.Serialization;
using MhpdCommon.Constants.HttpClient;

namespace MhpdCommon.Models.MessageBodyModels;

public class TokenIntegrationRequestModel
{
    public string? Rqp { get; set; }

    public string? Ticket { get; set; }    

    [JsonPropertyName(QueryParams.Cda.Token.AsUri)]
    public string? AsUri{ get; set; }
}