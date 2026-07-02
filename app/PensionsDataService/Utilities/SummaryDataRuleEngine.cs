using MhpdCommon.Models.MHPDModels;
using MhpdCommon.ViewData;
using PensionsDataService.Models;
using System.Text.Json.Nodes;
using static MhpdCommon.ViewData.EvaluationConstants;

namespace PensionsDataService.Utilities;

public class SummaryDataRuleEngine(IPensionNavigator navigator) : ISummaryDataRuleEngine
{
    public SummaryData Evaluate(RetrievedPensionRecord? statePension, IEnumerable<RetrievedPensionRecord> pensions)
    {
        var hasMcCloudPension = pensions.Any(p => p.IsMcCloudPension);

        SummaryData summary = CreateSummary(hasMcCloudPension);

        if (statePension == null)
        {
            return summary;
        }

        var stateArrangement = JsonNode.Parse(statePension.RetrievalResult!.GetRawText());
        var earliestComponent = navigator.SelectIllustrationComponent(stateArrangement, PayableDetailsType.Recurring);
        DateTime? spRetirementDate = navigator.SelectRetirementDate(stateArrangement, earliestComponent);

        if (spRetirementDate == null)
        {
            return summary;
        }

        summary.StatePensionDate = spRetirementDate.Value.ToString("yyyy-MM-dd");

        var activePayments = GetActivePayableDetails(pensions, spRetirementDate.Value).ToList();

        var standardPayment = new PensionPayment { MonthlyAmount = 0, AnnualAmount = 0 };
        var legacyPayment = new PensionPayment { MonthlyAmount = 0, AnnualAmount = 0 };
        var alternativePayment = new PensionPayment { MonthlyAmount = 0, AnnualAmount = 0 };
        var isMcCloudPensionInRange = false;

        foreach (var details in activePayments)
        {
            var type = details.PaymentType;

            if (type == PensionEnums.PayableDetailsType.Recurring)
            {
                Accumulate(standardPayment, details);
                Accumulate(legacyPayment, details);
                Accumulate(alternativePayment, details);
            }
            else if (type == PensionEnums.PayableDetailsType.RecurringLegacy)
            {
                isMcCloudPensionInRange = true;
                Accumulate(legacyPayment, details);
            }
            else if (type == PensionEnums.PayableDetailsType.RecurringNew)
            {
                isMcCloudPensionInRange = true;
                Accumulate(alternativePayment, details);
            }
        }

        if (isMcCloudPensionInRange)
        {
            summary.LegacyPayment = legacyPayment.HasAnyValues ? legacyPayment : null;
            summary.AlternativePayment = alternativePayment.HasAnyValues ? alternativePayment : null;
        }
        else
        {
            summary.StandardPayment = standardPayment.HasAnyValues ? standardPayment : null;
        }

        return summary;
    }

    private static SummaryData CreateSummary(bool hasMcCloudPension)
    {
        return new SummaryData
        {
            IsMcCloudPensionPresent = hasMcCloudPension,
            AlternativePayment = hasMcCloudPension ? new PensionPayment() : null,
            LegacyPayment = hasMcCloudPension ? new PensionPayment() : null,
            StandardPayment = hasMcCloudPension ? null : new PensionPayment()
        };
    }

    private IEnumerable<PayableDetails> GetActivePayableDetails(IEnumerable<RetrievedPensionRecord> pensions, DateTime spDate)
    {
        var targetTypes = new[] {
        PayableDetailsType.Recurring,
        PayableDetailsType.RecurringLegacy,
        PayableDetailsType.RecurringNew
    };

        return pensions
            .Where(p => p.HasIncome)
            .Select<RetrievedPensionRecord, JsonNode?>(p => JsonNode.Parse(p.RetrievalResult!.GetRawText()))
            .Where(node => node != null)
            .SelectMany(node => navigator.GetIllustrationsByType(node!, targetTypes))
            .Select(illustration => navigator.SelectEarliestIllustrationComponent(illustration))
            .Select(component => navigator.GetPayableDetails(component))
            .Where(details => IsActiveAtRetirement(details, spDate));
    }

    private static bool IsActiveAtRetirement(PayableDetails details, DateTime spDate)
    {
        return details.PayableDate?.Year <= spDate.Year &&
               (details.LastPaymentDate == null || details.LastPaymentDate?.Year >= spDate.Year);
    }

    private static void Accumulate(PensionPayment total, PayableDetails details)
    {
        total.MonthlyAmount += (details.MonthlyAmount ?? 0m);
        total.AnnualAmount += (details.AnnualAmount ?? 0m);
    }
}
