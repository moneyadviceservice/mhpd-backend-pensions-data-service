using Newtonsoft.Json;

namespace MhpdCommon.Models.MessageBodyModels;

public class RedirectRequestPayload
{
    [JsonProperty("redirectPurpose")]
    public string? RedirectPurpose { get; set; }

    [JsonProperty("iss")]
    public string? Iss { get; set; }

    [JsonProperty("userSessionId")]
    public string? UserSessionId { get; set; }
}
