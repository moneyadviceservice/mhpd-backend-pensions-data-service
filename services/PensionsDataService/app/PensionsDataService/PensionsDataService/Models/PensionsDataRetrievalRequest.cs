namespace PensionsDataService.Models;

public class PensionsDataRetrievalRequest
{
    public string Ticket { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
}