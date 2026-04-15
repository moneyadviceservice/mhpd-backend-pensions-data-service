using MhpdCommon.Models.MHPDModels;
using PensionsDataService.Models;

namespace PensionsDataService.Utilities;

public interface ITimelineArrangementFactory
{
    IEnumerable<TimelineArrangement> CreateAll(RetrievedPensionRecord pension, bool includeWithoutIncome);

    IEnumerable<TimelineArrangement> CreateAllLegacy(RetrievedPensionRecord pension, bool includeWithoutIncome);

    IEnumerable<TimelineArrangement> CreateAllAlternative(RetrievedPensionRecord pension, bool includeWithoutIncome);
}
