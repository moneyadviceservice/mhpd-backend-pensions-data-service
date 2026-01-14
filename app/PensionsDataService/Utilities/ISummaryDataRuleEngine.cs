using MhpdCommon.Models.MHPDModels;
using PensionsDataService.Models;

namespace PensionsDataService.Utilities;

public interface ISummaryDataRuleEngine
{
    SummaryData Evaluate(RetrievedPensionRecord statePension, IReadOnlyList<RetrievedPensionRecord> pensions);
}
