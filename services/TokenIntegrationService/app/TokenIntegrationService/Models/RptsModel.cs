using System.Text.Json.Serialization;

namespace TokenIntegrationService.Models
{
    public class RptsModel
    {        
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }               
    }
}
