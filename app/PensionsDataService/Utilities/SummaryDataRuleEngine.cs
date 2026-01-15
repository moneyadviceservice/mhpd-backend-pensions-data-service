using MhpdCommon.Models.MHPDModels;
using MhpdCommon.ViewData;
using PensionsDataService.Models;
using System.Text.Json.Nodes;

namespace PensionsDataService.Utilities;

public class SummaryDataRuleEngine(IPensionNavigator navigator) : ISummaryDataRuleEngine
{
    public SummaryData Evaluate(RetrievedPensionRecord statePension, IEnumerable<RetrievedPensionRecord> pensions)
    {
        var summary = new SummaryData();

        var stateArrangement = JsonNode.Parse(statePension.RetrievalResult!.GetRawText());

        var earliestComponent = navigator.SelectIllustrationComponent(stateArrangement);

        DateTime? spRetirementDate = navigator.SelectRetirementDate(stateArrangement, earliestComponent);
        summary.StatePensionDate = spRetirementDate?.ToString("yyyy-MM-dd");

        if (summary.StatePensionDate == null)
        {
            return summary;
        }

        foreach (var pension in pensions)
        {
            if(pension.HasIncome == Boolean.FalseString)
            {
                continue;
            }

            JsonNode? arrangement = JsonNode.Parse(pension.RetrievalResult!.GetRawText());
            if (arrangement == null)
            {
                continue;
            }

            var component = navigator.SelectIllustrationComponent(arrangement);

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
