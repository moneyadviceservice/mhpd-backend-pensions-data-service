namespace PensionsDataService.Models;

public sealed class CardData
{
    public decimal? MonthlyAmount { get; init; }
    public string? UnavailableCode { get; init; }
    public DateTime? RetirementDate { get; init; }

    public bool HasAnyValue =>
        MonthlyAmount.HasValue ||
        !string.IsNullOrWhiteSpace(UnavailableCode) ||
        RetirementDate.HasValue;
}
