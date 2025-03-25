using System.Text.Json.Serialization;
using MhpdCommon.Constants.HttpClient;

namespace MhpdCommon.Models.MessageBodyModels;

public class TokenClientRequestModel
{
    public string Rqp { get; set; } = string.Empty;

    [JsonPropertyName(QueryParams.Cda.Token.Pct)]
    public string? Pct { get; set; } = string.Empty;
    
    public string Ticket { get; set; } = string.Empty;

    [JsonPropertyName(QueryParams.Cda.Token.AsUri)]
    public string? AsUri{ get; set; }
    public string CorrelationId { get; set; } = string.Empty;
}
