using MAPS.Core.Models.MHPDModels;
using System.Text.Json.Serialization;

namespace CommonServices.Models
{
    public class PensionAdministrator
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("contactMethods")]
        public List<ContactMethods>? ContactMethods { get; set; }
        
    }
}