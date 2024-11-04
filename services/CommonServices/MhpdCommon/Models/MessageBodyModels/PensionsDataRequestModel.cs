namespace MhpdCommon.Models.MessageBodyModels;

public class PensionsDataRequestModel
{
    public string? ClientId { get; set; }
    
    public string? ClientSecret { get; set; }

    public string? AuthorisationCode { get; set; }

    public string? RedirectUrl { get; set; }

    public string? CodeVerifier { get; set; }
}