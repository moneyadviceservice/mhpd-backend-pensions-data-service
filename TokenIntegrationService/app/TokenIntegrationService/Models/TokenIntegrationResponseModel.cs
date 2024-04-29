using System.Text.Json.Serialization;
namespace TokenIntegrationService.Models
{
    public class TokenIntegrationResponseModel
    {   
        [JsonPropertyName("rpt")]
        public string? Rpt { get; set; }

    }
}
