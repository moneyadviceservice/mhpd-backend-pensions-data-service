using System.Text.Json.Serialization;

namespace PensionsDataService.Models;

public class SummaryData
{
    public string? StatePensionDate { get; set; }
    public bool IsMcCloudPensionPresent { get; set; }
    public PensionPayment? StandardPayment {  get; set; }
    public PensionPayment? LegacyPayment { get; set; }
    public PensionPayment? AlternativePayment { get; set; }

    [JsonIgnore]
    public bool HasAnyValue =>
        (StandardPayment?.HasAnyValues ?? false) ||
        (LegacyPayment?.HasAnyValues ?? false) ||
        (AlternativePayment?.HasAnyValues ?? false) ||
        StatePensionDate != null;
}
