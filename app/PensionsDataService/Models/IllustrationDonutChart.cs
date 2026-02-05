namespace PensionsDataService.Models;

public class IllustrationDonutChart : IllustrationChart
{
    public LumpSumChartData Eri { get; set; } = new LumpSumChartData();
    public LumpSumChartData Ap { get; set; } = new LumpSumChartData();
}
