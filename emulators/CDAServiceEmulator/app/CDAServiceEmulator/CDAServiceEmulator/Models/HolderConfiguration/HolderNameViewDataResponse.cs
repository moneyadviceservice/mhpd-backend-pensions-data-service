using System.Text.Json.Serialization;

namespace CDAServiceEmulator.Models.HolderConfiguration;

public class HolderNameViewDataResponse
{
    [JsonPropertyName("holder_view_configurations")]
    public List<HolderNameConfigurationModel> Configurations { get; set; } = [];
}
