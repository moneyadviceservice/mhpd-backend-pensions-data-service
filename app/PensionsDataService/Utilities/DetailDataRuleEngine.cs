using MhpdCommon.Constants;
using MhpdCommon.ViewData;
using PensionsDataService.Models;
using System.Text.Json.Nodes;

namespace PensionsDataService.Utilities;

public sealed class DetailDataRuleEngine(IPensionNavigator navigator)
    : IDetailDataRuleEngine
{
    public DetailData Evaluate(JsonNode retrievalResult)
    {
        ArgumentNullException.ThrowIfNull(retrievalResult);

        var illustration = navigator.SelectLatestIllustration(retrievalResult);
        var recurringComponent = navigator.SelectIllustrationComponent(retrievalResult);
        var accruedComponent = navigator.SelectEarliestComponent(illustration, EvaluationConstants.IllustrationType.Accrued);

        var lumpSumComponent =
            navigator.SelectEarliestLumpSumComponent(retrievalResult, EvaluationConstants.IllustrationType.Estimated);

        PayableDetails recurringPayableDetails = navigator.GetPayableDetails(recurringComponent);
        PayableDetails lumpSumPayableDetails = navigator.GetPayableDetails(lumpSumComponent);

        return new DetailData
        {
            RetirementDate = FormatDate(navigator.SelectRetirementDate(retrievalResult, recurringComponent)),
            IllustrationDate = FormatDate(illustration?[PensionConstants.IllustrationDate]?.GetValue<DateTime?>()),
            MonthlyAmount = navigator.SelectMonthlyAmount(recurringComponent),
            PayableDate = FormatDate(recurringPayableDetails.PayableDate),
            PotValue = accruedComponent?[PensionConstants.RetirementPot]?.GetValue<decimal?>(),
            LumpSumAmount = navigator.SelectLumpSumAmount(lumpSumComponent),
            LumpSumPayableDate = FormatDate((lumpSumPayableDetails.PayableDate)),
            BenefitType = recurringComponent?[PensionConstants.BenefitType]?.GetValue<string>(),
            UnavailableCode = navigator.SelectUnavailableCode(recurringComponent),
            Warnings = ExtractWarnings(recurringComponent, accruedComponent)
        };
    }

    private static string? FormatDate(DateTime? date)
    {
        return date?.ToString("yyyy-MM-dd");
    }

    private static List<string> ExtractWarnings(JsonNode? eriComponent, JsonNode? apComponent)
    {
        var eriWarnings = ExtractWarnings(eriComponent);
        var apWarnings = ExtractWarnings(apComponent);

        List<string> warnings = [.. eriWarnings, .. apWarnings];
        return [.. warnings.Distinct()];
    }

    private static List<string> ExtractWarnings(JsonNode? component)
    {
        if (component == null)
        {
            return [];
        }

        var warningsNode = component[PensionConstants.IllustrationWarnings];

        if (warningsNode is not JsonArray warningsArray)
        {
            return [];
        }

        return warningsArray
            .Select(w => w?.GetValue<string>())
            .Where(w => !string.IsNullOrWhiteSpace(w))
            .ToList()!;
    }
}

