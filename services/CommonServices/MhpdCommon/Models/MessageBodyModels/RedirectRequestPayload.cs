using MhpdCommon.Constants;
using MhpdCommon.Models.OpenApi;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace MhpdCommon.Models.MessageBodyModels;

public class RedirectRequestPayload
{
    [Required]
    [FixedValue(ApiConstants.RedirectPurpose)]
    [JsonProperty("redirectPurpose")]
    public string? RedirectPurpose { get; set; }

    [Required]
    [MinLength(1)]
    [MaxLength(200)]
    [JsonProperty("iss")]
    public string? Iss { get; set; }

    [Required]
    [RegularExpression(ApiConstants.GuidPattern)]
    [JsonProperty("userSessionId")]
    public string? UserSessionId { get; set; }
}
