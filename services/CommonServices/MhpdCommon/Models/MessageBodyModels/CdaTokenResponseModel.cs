using MhpdCommon.Constants;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json.Serialization;

namespace MhpdCommon.Models.MessageBodyModels
{
    public class CdaTokenResponseModel
    {
        [RegularExpression(ApiConstants.JwtPattern)]
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }

        [RegularExpression(ApiConstants.JwtPattern)]
        [JsonPropertyName("id_token")]
        public string? IdToken { get; set; }

        [RegularExpression(ApiConstants.JwtPattern)]
        [JsonPropertyName("pct")]
        public string? Pct { get; set; }

        public HttpStatusCode StatusCode { get; set; }

        public ClaimsGatheringResponseModel? UserRedirectDetails { get; set; }
    }
}
