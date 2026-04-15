namespace PensionsDataService.Models;

public class StatePayment
{
    public decimal? EstimatedMonthlyAmount { get; set; }
    public decimal? EstimatedAnnualAmount { get; set; }
    public decimal? AccruedMonthlyAmount { get; set; }
    public decimal? AccruedAnnualAmount { get; set; }
    public string? IllustrationDate { get; init; }
    public string? BenefitType { get; init; }

    public bool HasAnyValues => EstimatedMonthlyAmount.HasValue || EstimatedAnnualAmount.HasValue 
            || AccruedMonthlyAmount.HasValue || AccruedAnnualAmount.HasValue || !string.IsNullOrEmpty(IllustrationDate) 
            || !string.IsNullOrEmpty(BenefitType);
}
