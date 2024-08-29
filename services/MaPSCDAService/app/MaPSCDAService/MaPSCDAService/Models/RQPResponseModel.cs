using System.Text.Json.Serialization;

namespace MaPSCDAService.Models
{
    public class RQPResponseModel
    {
        [JsonPropertyName("rqp")]
        public string? Rqp { get; set;}     
    }
}
