namespace MhpdCommon.Models.MessageBodyModels;

public class TokenClientRequestModel
{
    public string Rqp { get; set; } = string.Empty;

    public string Ticket { get; set; } = string.Empty;

    public string As_Uri { get; set; } = string.Empty;

    public string CorrelationId { get; set; } = string.Empty;
}
