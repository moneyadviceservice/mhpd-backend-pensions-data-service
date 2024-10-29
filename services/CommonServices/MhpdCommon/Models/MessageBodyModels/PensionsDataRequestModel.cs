namespace MhpdCommon.Models.MessageBodyModels;

public class PensionsDataRequestModel
{
    public string? AuthorisationCode { get; set; }
    
    public string? RedirectUri { get; set; }
    
    public string? CodeVerifier { get; set; }
}