using System.Text.Json.Serialization;

namespace PensionsDataService.Models;

public class PensionPayment
{
    public decimal? MonthlyAmount { get; set; }
    public decimal? AnnualAmount { get; set; }
    public decimal? PotValue { get; set; }
    public decimal? LumpSumAmount { get; set; }
    public string? PayableDate { get; set; }
    public string? LumpSumPayableDate { get; set; }
    public string? BenefitType { get; set; }

    [JsonIgnore]
    public bool HasAnyValues => MonthlyAmount.HasValue || PotValue.HasValue || LumpSumAmount.HasValue 
        || !string.IsNullOrEmpty(PayableDate) || !string.IsNullOrEmpty(LumpSumPayableDate) || !string.IsNullOrEmpty(BenefitType);
}
