namespace PensionsDataService.Models;

public class PensionsDataResponseModel : PensionsStatusResponseModel
{
    public PeiInformation? PeiInformation { get; set; }

    public List<PensionPolicy>? PensionPolicies { get; set; }
}