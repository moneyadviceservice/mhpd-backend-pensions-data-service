namespace PensionsDataService.Models;

public class PensionsDataResponseModel
{
    public List<PensionPolicy>? PensionPolicies { get; set; }
    public PeiInformation? PeiInformation { get; set; }
    public bool PensionsDataRetrievalComplete { get; set; }
}