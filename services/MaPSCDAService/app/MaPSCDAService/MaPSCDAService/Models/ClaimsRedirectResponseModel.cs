namespace MaPSCDAService.Models;

public class ClaimsRedirectResponseModel
{
    public string ClaimsRedirectUrl { get; set; } = string.Empty;

    public string Rqp { get; set; } = string.Empty;

    public string Ticket { get; set; } = string.Empty;

    public string RequestId { get; set; } = string.Empty;
}
