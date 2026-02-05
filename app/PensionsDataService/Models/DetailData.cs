namespace PensionsDataService.Models;

public sealed class DetailData
{
    public string? RetirementDate { get; set; }
    public string? IllustrationDate { get; set; }
    public decimal? MonthlyAmount { get; set; }
    public string? PayableDate { get; set; }
    public decimal? PotValue { get; set; }
    public decimal? LumpSumAmount { get; set; }
    public string? LumpSumPayableDate { get; set; }
    public string? BenefitType { get; set; }
    public string? UnavailableCode { get; set; }
    public IReadOnlyList<string>? Warnings { get; set; }
    public List<IllustrationIncome> IncomeAndValues { get; set; } = [];
}