using MhpdCommon.Models.MHPDModels;
using PensionsDataService.Models;
using PensionsDataService.Utilities;

namespace PensionsDataService.Extensions;

public static class PensionDataExtensions
{
    public static void EnrichSummaryData(
        this PensionData response,
        IReadOnlyList<RetrievedPensionRecord> pensions,
        string pensionCategory,
        ISummaryDataRuleEngine ruleEngine)
    {
        ArgumentNullException.ThrowIfNull(response);

        if (pensions == null || pensions.Count == 0)
        {
            return;
        }

        // Only enrich if a State pension exists
        var statePension = pensions.FirstOrDefault(p => p.PensionType == Constants.PensionTypes.SP);

        if (statePension == null)
        {
            return;
        }

        SummaryData summary = ruleEngine.Evaluate(statePension, pensions.Where(pension => pension.Category == pensionCategory));

        if (!summary.HasAnyValue)
        {
            return;
        }

        response.SummaryData = summary;
    }
}
