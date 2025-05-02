using MhpdCommon.Constants;
using MhpdCommon.Models.OpenApi;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace MhpdCommon.Models.MHPDModels;

public class RedirectResponseModel
{
    [RegularExpression(ApiConstants.UrlPattern)]
    [JsonProperty("redirectTargetUrl")]
    public string? RedirectTargetUrl { get; set; }

    [RegularExpression(ApiConstants.JwtPattern)]
    [JsonProperty("rqp")]
    public string? Rqp { get; set; }

    [MinLength(1)]
    [JsonProperty("scope")]
    public string? Scope { get; set; }

    [FixedValue(ApiConstants.ResponseType)]
    [JsonProperty("responseType")]
    public string? ResponseType { get; set; }

    [FixedValue(ApiConstants.Prompt)]
    [JsonProperty("prompt")]
    public string? Prompt { get; set; }

    [MinLength(1)]
    [JsonProperty("service")]
    public string? Service { get; set; }

    [FixedValue(ApiConstants.CodeChallengeMethod)]
    [JsonProperty("codeChallengeMethod")]
    public string? CodeChallengeMethod { get; set; }

    [MinLength(1)]
    [JsonProperty("codeChallenge")]
    public string? CodeChallenge { get; set; }

    [MinLength(1)]
    [JsonProperty("codeVerifier")]
    public string? CodeVerifier { get; set; }

    [RegularExpression(ApiConstants.GuidPattern)]
    [JsonProperty("requestId")]
    public string? RequestId { get; set; }
}

