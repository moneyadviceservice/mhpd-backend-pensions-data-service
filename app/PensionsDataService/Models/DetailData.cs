namespace PensionsDataService.Models;

public sealed class DetailData
{
    public string? RetirementDate { get; set; }
    public StatePayment? StatePayment { get; set; }
    public PensionPayment? StandardPayment { get; set; }
    public PensionPayment? LegacyPayment { get; set; }
    public PensionPayment? AlternativePayment { get; set; }
    public IReadOnlyList<string>? Warnings { get; set; }
    public IReadOnlyList<string>? UnavailableCodes { get; set; }
    public DetailIncome? IncomeAndValues { get; set; }
}