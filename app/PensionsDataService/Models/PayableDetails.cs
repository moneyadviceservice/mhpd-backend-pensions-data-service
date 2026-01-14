namespace PensionsDataService.Models;

public class PayableDetails
{
    public decimal? MonthlyAmount { get; init; }

    public decimal? AnnualAmount { get; init; }

    public DateTime? PayableDate { get; init; }

    public DateTime? LastPaymentDate { get; init; }

    public string? AmountNotProvidedReason { get; set; }
}
