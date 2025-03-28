using System.Text.Json.Serialization;
using MhpdCommon.Models.MHPDModels;

namespace PensionRequestFunction.Models.CdaPeisServiceClient;

public class PdpServiceResponseModel
{
    [JsonPropertyName("view_data_token")]
    public string? ViewDataToken { get; set; }

    public ResponseMessage ResponseMessage { get; set; } = new();
}
