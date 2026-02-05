namespace PensionsDataService.Models;

public class RecurringChartData : ChartData
{
    public decimal? MonthlyAmount { get; init; }
    public decimal? AnnualAmount { get; init; }
    public bool? Increasing { get; init; }
}
