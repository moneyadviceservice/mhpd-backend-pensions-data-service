namespace PensionsDataService.Models;

public class PensionsStatusResponseModel
{
    public bool PensionsDataRetrievalComplete { get; set; }

    public int PredictedTotalDataRetrievalTime { get; set; }

    public int PredictedRemainingDataRetrievalTime { get; set; }
}
