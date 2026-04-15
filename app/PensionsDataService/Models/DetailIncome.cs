namespace PensionsDataService.Models;

public class DetailIncome
{
    public List<IllustrationIncome>? StandardIncome { get; set; }
    public List<IllustrationIncome>? LegacyIncome { get; set; }
    public List<IllustrationIncome>? AlternativeIncome { get; set; }
}
