namespace PensionsRetrievalFunction.Models;

public class PensionsRetrievalRecord
{
    public string Id { get; set; } = string.Empty;
    public string UserSessionId { get; set; } = string.Empty;
    public string Iss { get; set; } = string.Empty;
    public DateTime JobStartTimestamp { get; set; }
    public string? PeisRpt { get; set; }
    public string PeisId { get; set; } = string.Empty;
    public bool PeiRetrievalComplete { get; set; }
    public List<PeiData> PeiData { get; set; } = [];
}
