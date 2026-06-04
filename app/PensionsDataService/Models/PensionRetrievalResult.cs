using MhpdCommon.Models.MHPDModels;
using Microsoft.AspNetCore.Mvc;

namespace PensionsDataService.Models;

public class PensionRetrievalResult
{
    public IActionResult? EarlyResponse { get; set; }
    public IDisposable? LogScope { get; set; }
    public PensionsRetrievalRecord? RetrievalRecord { get; set; }
}
