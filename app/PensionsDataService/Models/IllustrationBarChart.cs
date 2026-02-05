namespace PensionsDataService.Models;

public class IllustrationBarChart : IllustrationChart
{
    public RecurringChartData Eri { get; set; } = new RecurringChartData();
    public RecurringChartData Ap { get; set; } = new RecurringChartData();
}
