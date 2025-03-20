namespace MhpdCommon.Models.MessageBodyModels;

public class PeiRequestModel
{
    public string Rpt { get; set; } = string.Empty;

    public string Iss { get; set; } = string.Empty;

    public string PeisId { get; set; } = string.Empty;

    public string UserSessionId { get; set; } = string.Empty;

    public string CorrelationId { get; set; } = string.Empty;
}
