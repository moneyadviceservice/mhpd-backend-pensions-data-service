using MhpdCommon.Constants;
using MhpdCommon.ViewData;
using PensionsDataService.Models;
using System.Globalization;
using System.Text.Json.Nodes;
using static MhpdCommon.ViewData.PensionEnums;

namespace PensionsDataService.Utilities;

public class PensionNavigator : IPensionNavigator
{
    public JsonNode? SelectIllustrationComponent(JsonNode retrievalResult, string payableDetailsType)
    {
        var illustration = SelectIllustrationByPayableType(retrievalResult, payableDetailsType);

        if (illustration == null)
        {
            return null;
        }

        return SelectEarliestIllustrationComponent(illustration);
    }

    public JsonNode? SelectEarliestIllustrationComponent(JsonNode? benefitIllustration)
    {
        var earliestComponent = SelectComponentByType(benefitIllustration, EvaluationConstants.IllustrationType.Estimated);

        if (earliestComponent?[PensionConstants.UnavailableReason]?.GetValue<string>() == Constants.UnavailableCodes.DB)
        {
            earliestComponent = SelectComponentByType(benefitIllustration, EvaluationConstants.IllustrationType.Accrued);
        }

        return earliestComponent;
    }

    public JsonNode? SelectIllustrationByPayableType(JsonNode retrievalResult, string payableDetailsType)
    {
        List<string?> matchingPayableDetails = [payableDetailsType];

        if(payableDetailsType == EvaluationConstants.PayableDetailsType.Recurring)
        {
            matchingPayableDetails.Add(EvaluationConstants.PayableDetailsType.None);
            matchingPayableDetails.Add(EvaluationConstants.PayableDetailsType.NoPayment);
            matchingPayableDetails.Add(null);
        }

        // Filter by type first
        var illustrations = GetIllustrationsByType(retrievalResult, matchingPayableDetails);

        if (illustrations.Count == 0)
        {
            return null;
        }

        JsonNode? fallback = illustrations[0];
        DateTime? latestDate = null;
        JsonNode? latestNode = null;

        foreach (var illustration in illustrations)
        {
            var component = SelectEarliestIllustrationComponent(illustration);
            var date = SelectPayableDate(component);

            if (date == null)
            {
                continue;
            }

            if (latestDate == null || date < latestDate)
            {
                latestDate = date;
                latestNode = illustration;
            }
        }

        return latestNode ?? fallback;
    }

    public JsonNode? SelectComponentByType(JsonNode? benefitIllustration, string illustrationType)
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

    public List<JsonNode> SelectAllComponentsByPayableType(JsonNode retrievalResult, string payableDetailsType)
    {
        List<string> matchingPayableDetails = [payableDetailsType];

        // Filter by type first
        var illustrations = GetIllustrationsByType(retrievalResult, matchingPayableDetails);

        var components = illustrations.Select(illustration =>
        {
            return SelectEarliestIllustrationComponent(illustration)!;
        }).ToList();

        return components;
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
            PotValue = SelectPotValue(component),
            PayableDate = SelectPayableDate(component),
            LastPaymentDate = SelectLastPaymentDate(component),
            IsIncreasing = SelectIncreasing(component),
            BenefitType = SelectBenefitType(component),
            UnavailableCode = SelectUnavailableCode(component),
            PaymentType = SelectPaymentType(component)
        };
    }

    public decimal? SelectMonthlyAmount(JsonNode? component)
    {
        return SelectPayableAmount(component, PensionConstants.MonthlyAmount);
    }

    public decimal? SelectAnnualAmount(JsonNode? component)
    {
        return SelectPayableAmount(component, PensionConstants.AnnualAmount);
    }

    public decimal? SelectLumpSumAmount(JsonNode? component)
    {
        return SelectPayableAmount(component, PensionConstants.Amount);
    }

    public PensionProperties SelectPensionProperties(JsonNode? retrievalResult)
    {
        return new PensionProperties
        {
            PensionType = retrievalResult?[PensionConstants.PensionType]?.GetValue<string>() ?? string.Empty,
            HasMultipleTranches = SelectBooleanProperty(retrievalResult, PensionConstants.HasMultipleTranches),
            HasMultipleIncomeOptions = SelectBooleanProperty(retrievalResult, PensionConstants.HasMultipleIncomeOptions),
            HasIncome = SelectBooleanProperty(retrievalResult, PensionConstants.HasIncome)
        };
    }

    public List<JsonNode?> GetIllustrationsByType(JsonNode? retrievalResult, IEnumerable<string?> payableDetailsTypes)
    {
        var illustrationsNode = retrievalResult?[PensionConstants.BenefitIllustrations];
        if (illustrationsNode is not JsonArray illustrations || illustrations.Count == 0)
        {
            return [];
        }

        return [.. illustrations.Where(c => payableDetailsTypes.Contains(c?[PensionConstants.PayableDetailsType]?.GetValue<string>()))];
    }

    private static decimal? SelectPotValue(JsonNode? component)
    {
        return SelectPensionAmount(component, PensionConstants.RetirementPot);
    }

    private static decimal? SelectPayableAmount(JsonNode? component, string amountName)
    {
        return SelectAmount(component, c => c?[PensionConstants.PayableDetails]?[amountName]);
    }

    private static decimal? SelectPensionAmount(JsonNode? component, string amountName)
    {
        return SelectAmount(component, c => c?[amountName]);
    }

    private static decimal? SelectAmount(JsonNode? component, Func<JsonNode?, JsonNode?> selector)
    {
        var amountNode = selector(component);

        if (amountNode is JsonValue valueNode && valueNode.TryGetValue<decimal>(out var amount))
        {
            return amount;
        }

        return null;
    }

    public string? SelectAmountNotProvidedReason(JsonNode? component)
    {
        return component?[PensionConstants.PayableDetails]?[PensionConstants.AmountNotProvidedReason]?.GetValue<string>();
    }

    private static string? SelectBenefitType(JsonNode? component)
    {
        return component?[PensionConstants.BenefitType]?.GetValue<string>();
    }

    private static bool? SelectIncreasing(JsonNode? component)
    {
        return component?[PensionConstants.PayableDetails]?[PensionConstants.Increasing]?.GetValue<bool>();
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

    private static bool? SelectBooleanProperty(JsonNode? node, string propertyName)
    {
        return node?[propertyName]?.GetValue<bool>();
    }

    public static PayableDetailsType SelectPaymentType(JsonNode? component)
    {
        var amountType = component?[PensionConstants.PayableDetails]?[PensionConstants.AmountType]?.GetValue<string>();

        return amountType switch
        {
            EvaluationConstants.AmountType.INC => PayableDetailsType.Recurring,
            EvaluationConstants.AmountType.INCL => PayableDetailsType.RecurringLegacy,
            EvaluationConstants.AmountType.INCN => PayableDetailsType.RecurringNew,
            EvaluationConstants.AmountType.CSH => PayableDetailsType.LumpSum,
            EvaluationConstants.AmountType.CSHL => PayableDetailsType.LumpSumLegacy,
            EvaluationConstants.AmountType.CSHN => PayableDetailsType.LumpSumNew,
            _ => PayableDetailsType.None,
        };
    }
}
