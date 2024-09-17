using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CDAServiceEmulator.Models.Peis;

public class CdaPeisEmulatorScenarioModel
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
    
    [JsonPropertyName("peisIdStartCode")]
    public string? PeisIdStartCode { get; set; }
    
    [JsonPropertyName("dataPoints")]
    public List<DataPoint>? DataPoints { get; set; }
}

public class DataPoint
{
    [JsonProperty("availableAt")]
    public int AvailableAt { get; set; }

    [JsonProperty("responsePayload")]
    public ResponsePayload? ResponsePayload { get; set; }
}

public class ResponsePayload
{
    [JsonProperty("peiList")]
    public List<PeiItem>? PeiList { get; set; }
}

public class PeiItem
{
    [JsonProperty("pei")]
    public string? Pei { get; set; }
    
    [JsonProperty("description")]
    public string? Description { get; set; }
}