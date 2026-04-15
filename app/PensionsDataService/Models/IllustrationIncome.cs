namespace PensionsDataService.Models;

public class IllustrationIncome
{
    public int Year { get; init; }
    public decimal MonthlyAmount { get; set; }
    public decimal AnnualAmount { get; set; }
    public decimal LumpSumAmount { get; set; }
    public decimal? Difference { get; set; }
}
