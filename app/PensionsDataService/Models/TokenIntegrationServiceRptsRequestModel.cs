namespace PensionsDataService.Models;

public class TokenIntegrationServiceRptsRequestModel
{
    public string? Rqp { get; set; }

    public string? Ticket { get; set; }

    public string? AsUri { get; set; }

    public string CorrelationId { get; set; } = string.Empty;
}