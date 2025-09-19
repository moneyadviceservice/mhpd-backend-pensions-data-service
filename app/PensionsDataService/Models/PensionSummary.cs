namespace PensionsDataService.Models;

public class PensionSummary
{
    public bool IsPensionRetrievalComplete { get; set; }

    public  int TotalPensionsFound { get; set; }

    public List<PensionItem> Pensions { get; set; } = [];
}
