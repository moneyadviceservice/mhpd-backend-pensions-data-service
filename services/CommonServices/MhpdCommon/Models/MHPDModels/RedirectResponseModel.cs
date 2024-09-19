using Newtonsoft.Json;

namespace MhpdCommon.Models.MHPDModels;

public class RedirectResponseModel
{
    [JsonProperty("redirectTargetUrl")]
    public string? RedirectTargetUrl { get; set; }

    [JsonProperty("rqp")]
    public string? Rqp { get; set; }

    [JsonProperty("scope")]
    public string? Scope { get; set; }

    [JsonProperty("responseType")]
    public string? ResponseType { get; set; }

    [JsonProperty("prompt")]
    public string? Prompt { get; set; }

    [JsonProperty("service")]
    public string? Service { get; set; }

    [JsonProperty("codeChallengeMethod")]
    public string? CodeChallengeMethod { get; set; }

    [JsonProperty("codeChallenge")]
    public string? CodeChallenge { get; set; }

    [JsonProperty("codeVerifier")]
    public string? CodeVerifier { get; set; }

    [JsonProperty("requestId")]
    public string? RequestId { get; set; }
}

