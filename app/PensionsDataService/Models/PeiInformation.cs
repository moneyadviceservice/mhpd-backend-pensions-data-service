using MhpdCommon.Models.MHPDModels;

namespace PensionsDataService.Models;

public class PeiInformation
{
    public bool PeiRetrievalComplete { get; set; }
    public List<PeiDataModel>? PeiData { get; set; }
}