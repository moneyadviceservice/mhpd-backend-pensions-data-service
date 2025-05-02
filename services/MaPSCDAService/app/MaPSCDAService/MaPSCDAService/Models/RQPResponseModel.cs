using MhpdCommon.Constants;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace MaPSCDAService.Models
{
    public class RqpResponseModel
    {
        [RegularExpression(ApiConstants.JwtPattern)]
        [JsonPropertyName("rqp")]
        public string? Rqp { get; set;}     
    }
}
