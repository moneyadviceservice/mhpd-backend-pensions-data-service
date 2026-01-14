using System.Text.Json.Serialization;

namespace PensionsDataService.Models;

public class SummaryData
{
    public decimal? MonthlyTotal { get; set; } = 0m;
    public decimal? AnnualTotal { get; set; } = 0m;
    public string? StatePensionDate { get; set; }

    [JsonIgnore]
    public bool HasAnyValue =>
        MonthlyTotal > 0 ||
        AnnualTotal > 0 ||
        StatePensionDate != null;
}
