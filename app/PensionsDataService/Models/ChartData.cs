namespace PensionsDataService.Models;

public class ChartData
{
    public string? PayableDate { get; init; }
    public string? BenefitType { get; init; }
    public string? CalculationMethod { get; init; }
    public bool? SafeguardedBenefit { get; init; }
    public bool? SurvivorBenefit { get; init; }
    public IReadOnlyList<string>? Warnings { get; set; }
}
