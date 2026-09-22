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
        response.EnrichSummaryData(pensions, [pensionCategory], ruleEngine);
    }

    public static void EnrichSummaryData(
        this PensionData response,
        IReadOnlyList<RetrievedPensionRecord> pensions,
        IEnumerable<string> pensionCategories,
        ISummaryDataRuleEngine ruleEngine)
    {
        ArgumentNullException.ThrowIfNull(response);

        if (pensions == null || pensions.Count == 0)
        {
            return;
        }

        ArgumentNullException.ThrowIfNull(pensionCategories);

        var categorySet = pensionCategories.ToHashSet(StringComparer.OrdinalIgnoreCase);

        // Only enrich if a State pension exists
        var statePension = pensions.FirstOrDefault(p => p.PensionType == Constants.PensionTypes.SP);

        SummaryData summary = ruleEngine.Evaluate(statePension, pensions.Where(pension => categorySet.Contains(pension.Category)));

        response.SummaryData = summary;
    }
}
