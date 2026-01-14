using MhpdCommon.Models.MHPDModels;
using MhpdCommon.ViewData;
using PensionsDataService.Models;
using System.Text.Json.Nodes;

namespace PensionsDataService.Utilities;

public class SummaryDataRuleEngine(IPensionNavigator navigator) : ISummaryDataRuleEngine
{
    public SummaryData Evaluate(RetrievedPensionRecord statePension, IReadOnlyList<RetrievedPensionRecord> pensions)
    {
        var summary = new SummaryData();

        var stateArrangement = JsonNode.Parse(statePension.RetrievalResult!.GetRawText());

        var spIllustration = navigator.SelectLatestIllustration(stateArrangement);

        var earliestComponent = spIllustration == null
            ? null
            : navigator.SelectEarliestComponent(spIllustration, EvaluationConstants.IllustrationType.Estimated);

        DateTime? spRetirementDate = navigator.SelectRetirementDate(stateArrangement, earliestComponent);
        summary.StatePensionDate = spRetirementDate?.ToString("yyyy-MM-dd");

        if (summary.StatePensionDate == null)
        {
            return summary;
        }

        foreach (var pension in pensions)
        {
            JsonNode? arrangement = JsonNode.Parse(pension.RetrievalResult!.GetRawText());
            if (arrangement == null)
            {
                continue;
            }

            var illustration = navigator.SelectLatestIllustration(arrangement);

            var component = illustration == null
                ? null
                : navigator.SelectEarliestComponent(illustration, EvaluationConstants.IllustrationType.Estimated);

            PayableDetails payableDetails = navigator.GetPayableDetails(component);

            //if the payable date is on or before the state pension date, and the last payment date is on or after the state pension date
            if (payableDetails.PayableDate?.Year <= spRetirementDate?.Year &&
                (payableDetails.LastPaymentDate == null || payableDetails.LastPaymentDate?.Year >= spRetirementDate.Value.Year))
            {
                summary.MonthlyTotal += (payableDetails.MonthlyAmount ?? 0m);
                summary.AnnualTotal += (payableDetails.AnnualAmount ?? 0m);
            }
        }

        return summary;
    }
}
