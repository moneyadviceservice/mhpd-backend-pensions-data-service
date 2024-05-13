using System.Text.Json.Serialization;

namespace CDAService.Models
{
    public class RQPResponseModel
    {
        [JsonPropertyName("rqp")]
        public string? Rqp { get; set;}     
    }
}
