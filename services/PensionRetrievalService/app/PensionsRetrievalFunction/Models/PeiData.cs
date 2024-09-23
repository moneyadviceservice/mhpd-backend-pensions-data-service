namespace PensionsRetrievalFunction.Models;
public class PeiData
{
    public string Pei { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string RetrievalStatus { get; set; } = string.Empty;
    public DateTime RetrievalRequestedTimestamp { get; set; }
}
