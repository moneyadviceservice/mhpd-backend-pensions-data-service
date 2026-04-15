namespace PensionsDataService.Models;

public class PensionProperties
{
    public string PensionType { get; set; } = string.Empty;
    public bool? HasMultipleTranches { get; set; }
    public bool? HasMultipleIncomeOptions { get; set; }
    public bool? HasIncome { get; set; }
}
