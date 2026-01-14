namespace PensionsDataService.Models;

public class PensionData
{
    public bool IsPensionRetrievalComplete { get; set; }

    public int TotalContactPensions { get; set; }

    public SummaryData? SummaryData { get; set; }

    public List<dynamic> Arrangements { get; set; } = [];
}
