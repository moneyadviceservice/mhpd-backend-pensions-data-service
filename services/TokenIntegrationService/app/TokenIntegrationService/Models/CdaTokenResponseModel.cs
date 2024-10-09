using System.Text.Json.Serialization;

namespace TokenIntegrationService.Models;

public class CdaTokenResponseModel
{        
    [JsonPropertyName("access_token")]
    public string? AccessToken { get; set; }          
        
    [JsonPropertyName("id_token")]
    public string? IdToken { get; set; }    
}