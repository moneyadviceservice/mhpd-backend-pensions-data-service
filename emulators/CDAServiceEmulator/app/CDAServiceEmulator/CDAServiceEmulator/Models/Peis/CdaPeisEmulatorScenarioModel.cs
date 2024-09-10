using System.Text.Json.Serialization;
using Newtonsoft.Json.Linq;

namespace CDAServiceEmulator.Models.Peis;

public class CdaPeisEmulatorScenarioModel
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
    
    [JsonPropertyName("peisIdStartCode")]
    public string? PeisIdStartCode { get; set; }
    
    [JsonPropertyName("dataPoints")]
    public JArray? DataPoints { get; set; }
}