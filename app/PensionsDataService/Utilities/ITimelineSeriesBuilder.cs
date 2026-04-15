using MhpdCommon.Models.MHPDModels;
using PensionsDataService.Models;

namespace PensionsDataService.Utilities;

public interface ITimelineSeriesBuilder
{
    TimelineSeries Build(IEnumerable<RetrievedPensionRecord> pensions, bool includeWithoutIncome);
    TimelineSeries BuildLegacy(IEnumerable<RetrievedPensionRecord> pensions, bool includeWithoutIncome);
    TimelineSeries BuildAlternative(IEnumerable<RetrievedPensionRecord> pensions, bool includeWithoutIncome);
}
