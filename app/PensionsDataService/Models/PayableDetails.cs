using static MhpdCommon.ViewData.PensionEnums;

namespace PensionsDataService.Models;

public class PayableDetails
{
    public decimal? MonthlyAmount { get; init; }

    public decimal? AnnualAmount { get; init; }

    public decimal? LumpSumAmount { get; init; }

    public decimal? PotValue { get; init; }

    public DateTime? PayableDate { get; init; }

    public DateTime? LastPaymentDate { get; init; }

    public string? AmountNotProvidedReason { get; init; }

    public string? BenefitType { get; init; }

    public bool? IsIncreasing { get; init; }

    public PayableDetailsType PaymentType {  get; init; }

    public bool HasAmount => MonthlyAmount.HasValue || AnnualAmount.HasValue || LumpSumAmount.HasValue;
}
