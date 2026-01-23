using MhpdCommon.Models.MHPDModels;
using PensionsDataService.Models;

namespace PensionsDataService.Utilities;

public interface ITimelineArrangementFactory
{
    TimelineArrangement? Create(RetrievedPensionRecord pension);
}
