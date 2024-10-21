namespace PensionRequestFunction.Models.CdaPeisServiceClient;

public class PdpServiceResponseModel
{
    public string? ViewDataToken { get; set; }

    public ResponseMessage ResponseMessage { get; set; } = new ResponseMessage();
}
