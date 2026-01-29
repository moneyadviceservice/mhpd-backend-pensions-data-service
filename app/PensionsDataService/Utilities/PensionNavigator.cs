using MhpdCommon.Constants;
using MhpdCommon.ViewData;
using PensionsDataService.Models;
using System.Globalization;
using System.Text.Json.Nodes;

namespace PensionsDataService.Utilities;

public class PensionNavigator : IPensionNavigator
{
    public JsonNode? SelectIllustrationComponent(JsonNode retrievalResult)
    {
        var illustration = SelectLatestIllustration(retrievalResult);

        if(illustration == null)
        {
            return null;
        }

        var earliestEriComponent = SelectEarliestComponent(illustration, EvaluationConstants.IllustrationType.Estimated);

        if (earliestEriComponent?[PensionConstants.UnavailableReason]?.GetValue<string>() == Constants.UnavailableCodes.DB)
        {
            var earliestApComponent = SelectEarliestComponent(illustration, EvaluationConstants.IllustrationType.Accrued);

            if (earliestApComponent?[PensionConstants.PayableDetails]?[PensionConstants.MonthlyAmount]?.GetValue<decimal>() > 0)
            {
                return earliestApComponent;
            }
        }

        return earliestEriComponent;
    }

    public JsonNode? SelectLatestIllustration(JsonNode retrievalResult)
    {
        var illustrationsNode = retrievalResult[PensionConstants.BenefitIllustrations];

        if (illustrationsNode is not JsonArray illustrations || illustrations.Count == 0)
        {
            return null;
        }

        JsonNode? fallback = illustrations[0];
        DateTime? latestDate = null;
        JsonNode? latestNode = null;

        foreach (var illustration in illustrations)
        {
            var dateStr = illustration?[PensionConstants.IllustrationDate]?.GetValue<string>();

            if (!DateTime.TryParse(dateStr, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            {
                continue;
            }

            if (latestDate == null || date > latestDate)
            {
                latestDate = date;
                latestNode = illustration;
            }
        }

        return latestNode ?? fallback;
    }

    public JsonNode? SelectEarliestComponent(JsonNode? benefitIllustration, string illustrationType)
    {
        if (benefitIllustration == null)
        {
            return null;
        }

        var componentsNode = benefitIllustration[PensionConstants.IllustrationComponents];

        if (componentsNode is not JsonArray components || components.Count == 0)
        {
            return null;
        }

        // Filter by type first
        var matching = components
            .Where(c => c?[PensionConstants.IllustrationType]?.GetValue<string>() == illustrationType)
            .ToList();

        if (matching.Count == 0)
        {
            return null;
        }

        JsonNode? fallback = matching[0];
        DateTime? earliestDate = null;
        JsonNode? earliestNode = null;

        foreach (var component in matching)
        {
            var dateStr = component?[PensionConstants.PayableDetails]?[PensionConstants.PayableDate]?.GetValue<string>();

            if (!DateTime.TryParse(dateStr, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            {
                continue;
            }

            if (earliestDate == null || date < earliestDate)
            {
                earliestDate = date;
                earliestNode = component;
            }
        }

        return earliestNode ?? fallback;
    }

    public JsonNode? SelectEarliestLumpSumComponent(JsonNode retrievalResult, string illustrationType)
    {
        DateTime? earliestDate = null;
        JsonNode? earliestNode = null;

        foreach (var component in GetIllustrationComponents(retrievalResult))
        {
            if (!IsMatchingIllustrationType(component, illustrationType))
            {
                continue;
            }

            if (!HasLumpSumAmount(component))
            {
                continue;
            }

            var date = GetPayableDate(component);
            if (!date.HasValue)
            {
                continue;
            }

            if (earliestDate == null || date < earliestDate)
            {
                earliestDate = date;
                earliestNode = component;
            }
        }

        return earliestNode;
    }


    public DateTime? SelectRetirementDate(JsonNode retrievalResult, JsonNode? component)
    {
        // Use the payableDetails.payableDate if there is one
        var payableDateStr = component?[PensionConstants.PayableDetails]?[PensionConstants.PayableDate]?.GetValue<string>();

        if (DateTime.TryParse(payableDateStr, CultureInfo.InvariantCulture, DateTimeStyles.None, out var payableDate))
        {
            return payableDate;
        }

        // Fallback to top-level retirementDate
        var retirementDateStr = retrievalResult[PensionConstants.RetirementDate]?.GetValue<string>();

        if (DateTime.TryParse(retirementDateStr, CultureInfo.InvariantCulture, DateTimeStyles.None, out var retirementDate))
        {
            return retirementDate;
        }

        return null;
    }

    public string? SelectUnavailableCode(JsonNode? component)
    {
        var unavailableReason =  component?[PensionConstants.UnavailableReason]?.GetValue<string>();

        unavailableReason ??= component?[PensionConstants.PayableDetails]?[PensionConstants.AmountNotProvidedReason]?.GetValue<string>();

        return unavailableReason;
    }

    public PayableDetails GetPayableDetails(JsonNode? component)
    {
        return new PayableDetails
        {
            MonthlyAmount = SelectMonthlyAmount(component),
            AnnualAmount = SelectAnnualAmount(component),
            LumpSumAmount = SelectLumpSumAmount(component),
            PayableDate = SelectPayableDate(component),
            LastPaymentDate = SelectLastPaymentDate(component),
            AmountNotProvidedReason = SelectAmountNotProvidedReason(component)
        };
    }

    public decimal? SelectMonthlyAmount(JsonNode? component)
    {
        return SelectAmount(component, PensionConstants.MonthlyAmount);
    }

    public decimal? SelectAnnualAmount(JsonNode? component)
    {
        return SelectAmount(component, PensionConstants.AnnualAmount);
    }

    public decimal? SelectLumpSumAmount(JsonNode? component)
    {
        return SelectAmount(component, PensionConstants.Amount);
    }

    private static decimal? SelectAmount(JsonNode? component, string amountName)
    {
        var amountNode = component?[PensionConstants.PayableDetails]?[amountName];

        if (amountNode == null)
        {
            return null;
        }

        if (amountNode is JsonValue valueNode &&
            valueNode.TryGetValue<decimal>(out var amount))
        {
            return amount;
        }

        return null;
    }

    public string? SelectAmountNotProvidedReason(JsonNode? component)
    {
        return component?[PensionConstants.PayableDetails]?[PensionConstants.AmountNotProvidedReason]?.GetValue<string>();
    }

    private static DateTime? SelectPayableDate(JsonNode? component)
    {
        return SelectPayableDetailsDate(component, PensionConstants.PayableDate);
    }

    private static DateTime? SelectLastPaymentDate(JsonNode? component)
    {
        return SelectPayableDetailsDate(component, PensionConstants.LastPaymentDate);
    }

    private static DateTime? SelectPayableDetailsDate(JsonNode? component, string dateName)
    {
        var dateStr = component?[PensionConstants.PayableDetails]?[dateName]?.GetValue<string>();
        if (DateTime.TryParse(dateStr, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            return date;
        }
        return null;
    }

    private static IEnumerable<JsonNode> GetIllustrationComponents(JsonNode retrievalResult)
    {
        if (retrievalResult[PensionConstants.BenefitIllustrations] is not JsonArray illustrations)
        {
            yield break;
        }

        foreach (var illustration in illustrations)
        {
            if (illustration?[PensionConstants.IllustrationComponents] is not JsonArray components)
            {
                continue;
            }

            foreach (var component in components)
            {
                yield return component!;
            }
        }
    }

    private static bool IsMatchingIllustrationType(JsonNode component, string illustrationType)
    {
        return component?[PensionConstants.IllustrationType]?.GetValue<string>() == illustrationType;
    }

    private static bool HasLumpSumAmount(JsonNode component)
    {
        return component?[PensionConstants.PayableDetails]?[PensionConstants.Amount]?.GetValue<decimal>() != null;
    }

    private static DateTime? GetPayableDate(JsonNode component)
    {
        var dateStr = component?[PensionConstants.PayableDetails]?[PensionConstants.PayableDate]?.GetValue<string>();

        return DateTime.TryParse(dateStr, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
            ? date
            : null;
    }
}
