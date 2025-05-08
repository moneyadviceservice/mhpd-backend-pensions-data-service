namespace PensionsDataService.Models;

public class PensionsDataResponseModel
{
    public bool PensionsDataRetrievalComplete { get; set; }

    public int PredictedTotalDataRetrievalTime { get; set; }

    public int PredictedRemainingDataRetrievalTime { get; set; }

    public PeiInformation? PeiInformation { get; set; }

    public List<PensionPolicy>? PensionPolicies { get; set; }
}