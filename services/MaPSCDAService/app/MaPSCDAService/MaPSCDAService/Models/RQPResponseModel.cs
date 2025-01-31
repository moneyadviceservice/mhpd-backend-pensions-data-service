using System.Text.Json.Serialization;

namespace MaPSCDAService.Models
{
    public class RqpResponseModel
    {
        [JsonPropertyName("rqp")]
        public string? Rqp { get; set;}     
    }
}
