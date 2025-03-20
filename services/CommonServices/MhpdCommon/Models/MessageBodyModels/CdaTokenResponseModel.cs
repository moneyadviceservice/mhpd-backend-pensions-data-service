using System.Net;
using System.Text.Json.Serialization;

namespace MhpdCommon.Models.MessageBodyModels
{
    public class CdaTokenResponseModel
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }

        [JsonPropertyName("id_token")]
        public string? IdToken { get; set; }

        [JsonPropertyName("pct")]
        public string? Pct { get; set; }

        public HttpStatusCode StatusCode { get; set; }

        public ClaimsGatheringResponseModel? UserRedirectDetails { get; set; }
    }
}
