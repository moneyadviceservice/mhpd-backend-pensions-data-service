namespace PensionsDataService.Models;

public class TransformRule
{
    public string SourceProperty { get; set; } = string.Empty;
    public string TargetProperty { get; set; } = string.Empty;
    public string ExtractProperty { get; set; } = string.Empty;
}
