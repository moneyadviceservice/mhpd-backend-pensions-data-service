using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using MhpdCommon.Constants;
using MhpdCommon.Constants.HttpClient;

namespace MhpdCommon.Models.MessageBodyModels;

public class TokenClientRequestModel
{
    [Required]
    [RegularExpression(ApiConstants.AsciiPattern)]
    [JsonPropertyName(QueryParams.Cda.Token.ClientId)]
    public string ClientId { get; set; } = string.Empty;

    [Required]
    [RegularExpression(ApiConstants.JwtPattern)]
    public string Rqp { get; set; } = string.Empty;

    [Required]
    [RegularExpression(ApiConstants.JwtPattern)]
    [JsonPropertyName(QueryParams.Cda.Token.Pct)]
    public string? Pct { get; set; } = string.Empty;

    [Required]
    [RegularExpression(ApiConstants.JwePattern)]
    public string Ticket { get; set; } = string.Empty;

    [RegularExpression(ApiConstants.UrlPattern)]
    [JsonPropertyName(QueryParams.Cda.Token.AsUri)]
    public string? AsUri{ get; set; }
}
