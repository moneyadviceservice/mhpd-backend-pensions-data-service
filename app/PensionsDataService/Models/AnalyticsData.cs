namespace PensionsDataService.Models;

public class AnalyticsData
{
    public int TotalErrorPensions { get; set; }

    public int TotalPensions { get; set; }

    public IEnumerable<dynamic> Arrangements { get; set; } = [];
}
