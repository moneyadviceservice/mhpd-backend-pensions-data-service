using MhpdCommon.Constants;
using MhpdCommon.Extensions;
using MhpdCommon.Models.MHPDModels;
using MhpdCommon.ViewData;
using PensionsDataService.Models;
using System.Text.Json.Nodes;
using static MhpdCommon.ViewData.EvaluationConstants;

namespace PensionsDataService.Utilities;

public sealed class DetailDataRuleEngine(IPensionNavigator navigator, ITimelineSeriesBuilder builder)
    : IDetailDataRuleEngine
{
    private static readonly List<string?> AllPayableTypes = [
            PayableDetailsType.RecurringLegacy,
            PayableDetailsType.RecurringNew,
            PayableDetailsType.LumpSumLegacy,
            PayableDetailsType.LumpSumNew,
            PayableDetailsType.Recurring,
            PayableDetailsType.LumpSum,
            PayableDetailsType.None,
            PayableDetailsType.NoPayment,
            null
        ];

    private static readonly List<string?> LegacyPayableTypes = [
            PayableDetailsType.RecurringLegacy,
            PayableDetailsType.LumpSumLegacy,
            PayableDetailsType.Recurring,
            PayableDetailsType.LumpSum,
            PayableDetailsType.None,
            PayableDetailsType.NoPayment,
            null
        ];

    private static readonly List<string?> AlternativePayableTypes = [
            PayableDetailsType.RecurringNew,
            PayableDetailsType.LumpSumNew,
            PayableDetailsType.Recurring,
            PayableDetailsType.LumpSum,
            PayableDetailsType.None,
            PayableDetailsType.NoPayment,
            null
        ];

    public DetailData Evaluate(RetrievedPensionRecord pension)
    {
        JsonNode retrievalResult = JsonNode.Parse(pension.RetrievalResult!.GetRawText());
        var pensionProperties = navigator.SelectPensionProperties(retrievalResult);

        // recurringComponent = The ERI component - unless the this has an unavailable code of DB in which case it will be the AP component.
        // recurringEriComponent = Always the ERI component of the recurring illustration
        // recurringApComponent = Always the AP component of the recurring illustration
        var recurringIllustration = navigator.SelectIllustrationByPayableType(retrievalResult, PayableDetailsType.Recurring);
        var recurringComponent = navigator.SelectEarliestIllustrationComponent(recurringIllustration);
        var recurringEriComponent = navigator.SelectComponentByType(recurringIllustration, IllustrationType.Estimated);
        var recurringApComponent = navigator.SelectComponentByType(recurringIllustration, IllustrationType.Accrued);

        var detailData = new DetailData();

        var allIllustrations = navigator.GetIllustrationsByType(retrievalResult, AllPayableTypes);

        // Payable details information
        if (pensionProperties.PensionType == PensionEnums.PensionType.SP.GetDisplayValue())
        {
            var eriIllustrtationDate = FormatDate(recurringIllustration?[PensionConstants.IllustrationDate]?.GetValue<DateTime?>());
            detailData.StatePayment = ExtractStatePayment(recurringEriComponent, recurringApComponent, eriIllustrtationDate);
            detailData.RetirementDate = FormatDate(navigator.SelectRetirementDate(retrievalResult, recurringComponent));
        }
        else if (pensionProperties.HasMultipleIncomeOptions.GetValueOrDefault())
        {
            var inclIllustration = navigator.SelectIllustrationByPayableType(retrievalResult, PayableDetailsType.RecurringLegacy);
            var incnIllustration = navigator.SelectIllustrationByPayableType(retrievalResult, PayableDetailsType.RecurringNew);
            var cshlIllustration = navigator.SelectIllustrationByPayableType(retrievalResult, PayableDetailsType.LumpSumLegacy);
            var cshnIllustration = navigator.SelectIllustrationByPayableType(retrievalResult, PayableDetailsType.LumpSumNew);
            var legacyIllustrations = navigator.GetIllustrationsByType(retrievalResult, LegacyPayableTypes);
            var alternativeIllustrations = navigator.GetIllustrationsByType(retrievalResult, AlternativePayableTypes);

            var legacyRecurringEri = navigator.SelectEarliestIllustrationComponent(inclIllustration);
            var legacyRecurringAp = navigator.SelectComponentByType(inclIllustration, IllustrationType.Accrued);
            var legacyLumpSum = navigator.SelectEarliestIllustrationComponent(cshlIllustration);
            detailData.LegacyPayment = ExtractPayment(legacyRecurringEri, legacyRecurringAp, legacyLumpSum, legacyIllustrations);

            var alternativeRecurringEri = navigator.SelectEarliestIllustrationComponent(incnIllustration);
            var alternativeRecurringAp = navigator.SelectComponentByType(incnIllustration, IllustrationType.Accrued);
            var alternativeLumpSum = navigator.SelectEarliestIllustrationComponent(cshnIllustration);
            detailData.AlternativePayment = ExtractPayment(alternativeRecurringEri, alternativeRecurringAp, alternativeLumpSum, alternativeIllustrations);

            detailData.RetirementDate = FormatDate(navigator.SelectRetirementDate(retrievalResult, recurringComponent));
        }
        else
        {
            // lumpSumEriComponent = Always the ERI component of the lump sum illustration
            var lumpSumIllustration = navigator.SelectIllustrationByPayableType(retrievalResult, PayableDetailsType.LumpSum);
            var lumpSumEriComponent = navigator.SelectComponentByType(lumpSumIllustration, IllustrationType.Estimated);

            detailData.StandardPayment = ExtractPayment(recurringComponent, recurringApComponent, lumpSumEriComponent, allIllustrations);
            detailData.RetirementDate = FormatDate(navigator.SelectRetirementDate(retrievalResult, recurringComponent ?? lumpSumEriComponent));
        }

        // Income timeline information
        if (pensionProperties.PensionType != PensionEnums.PensionType.SP.GetDisplayValue())
        {
            detailData.IncomeAndValues = new DetailIncome();

            if (pensionProperties.HasMultipleIncomeOptions.GetValueOrDefault())
            {
                var legacyTimelineSeries = builder.BuildLegacy([pension], true);
                var alternativeTimelineSeries = builder.BuildAlternative([pension], true);

                detailData.IncomeAndValues.LegacyIncome = GetTimeBasedIncome(legacyTimelineSeries);
                detailData.IncomeAndValues.AlternativeIncome = GetTimeBasedIncome(alternativeTimelineSeries);
            }
            else
            {
                var timelineSeries = builder.Build([pension], true);
                detailData.IncomeAndValues.StandardIncome = GetTimeBasedIncome(timelineSeries);
            }
        }

        // Warnings and unavailable codes
        List<string>? warnings = [];
        List<string>? unavailableCodes = [];

        foreach(var illustration in allIllustrations)
        {
            var eri = navigator.SelectComponentByType(illustration, IllustrationType.Estimated);
            var ap = navigator.SelectComponentByType(illustration, IllustrationType.Accrued);

            var illustrationWarnings = ExtractWarnings(eri, ap);
            var illustrationCodes = ExtractUnavailableCodes(eri, ap);

            warnings.AddRange(illustrationWarnings);
            unavailableCodes.AddRange(illustrationCodes);
        }

        detailData.Warnings = warnings.Count > 0 ? warnings.Distinct().ToList() : null;
        detailData.UnavailableCodes = unavailableCodes.Count > 0 ? unavailableCodes.Distinct().ToList() : null;

        return detailData;
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

    private List<string> ExtractUnavailableCodes(JsonNode? eriComponent, JsonNode? apComponent)
    {
        var eriCode = navigator.SelectUnavailableCode(eriComponent);
        var apCode = navigator.SelectUnavailableCode(apComponent);

        var codes = new[] { eriCode, apCode }
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Select(c => c!)
            .Distinct()
            .ToList();

        return codes;
    }

    private PensionPayment? ExtractPayment(JsonNode? recurringEriComponent, JsonNode? recurringApComponent, 
        JsonNode? lumpSumComponent, IEnumerable<JsonNode?> illustrations)
    {
        var recurringEriDetails = navigator.GetPayableDetails(recurringEriComponent);
        var recurringApDetails = navigator.GetPayableDetails(recurringApComponent);
        var lumpSumDetails = navigator.GetPayableDetails(lumpSumComponent);

        var payment = new PensionPayment
        {
            MonthlyAmount = 0,
            PotValue = 0,
            LumpSumAmount = 0,
            PayableDate = FormatDate(recurringEriDetails.PayableDate),
            LumpSumPayableDate = FormatDate(lumpSumDetails.PayableDate),
            BenefitType = recurringEriDetails.BenefitType ?? lumpSumDetails.BenefitType,
        };

        foreach (var illustration in illustrations)
        {
            var eriComponent = navigator.SelectEarliestIllustrationComponent(illustration);
            var apComponent = navigator.SelectComponentByType(illustration, IllustrationType.Accrued);

            var eriDetails = navigator.GetPayableDetails(eriComponent);
            var apDetails = navigator.GetPayableDetails(apComponent);

            // regular ERI
            if (eriDetails.PayableDate == recurringEriDetails.PayableDate)
            {
                payment.MonthlyAmount += (eriDetails.MonthlyAmount ?? 0m);
            }

            // AP Pot
            if (apDetails.PayableDate == recurringApDetails.PayableDate)
            {
                payment.PotValue += (apDetails.PotValue ?? 0m);
            }

            // Lump sums
            if (eriDetails.PayableDate == lumpSumDetails.PayableDate)
            {
                payment.LumpSumAmount += (eriDetails.LumpSumAmount ?? 0m);
            }
        }

        payment.MonthlyAmount = payment.MonthlyAmount > 0 ? payment.MonthlyAmount : null;
        payment.PotValue = payment.PotValue > 0 ? payment.PotValue : null;
        payment.LumpSumAmount = payment.LumpSumAmount > 0 ? payment.LumpSumAmount : null;

        return payment.HasAnyValues ? payment : null;
    }

    private StatePayment? ExtractStatePayment(JsonNode? recurringEriComponent, JsonNode? recurringApComponent, string? illustrationDate)
    {
        var eriDetails = navigator.GetPayableDetails(recurringEriComponent);
        var apDetails = navigator.GetPayableDetails(recurringApComponent);

        var payment = new StatePayment
        {
            EstimatedMonthlyAmount = eriDetails.MonthlyAmount,
            EstimatedAnnualAmount = eriDetails.AnnualAmount,
            AccruedMonthlyAmount = apDetails.MonthlyAmount,
            AccruedAnnualAmount = apDetails.AnnualAmount,
            IllustrationDate = illustrationDate,
            BenefitType = eriDetails.BenefitType
        };

        return payment.HasAnyValues ? payment : null;
    }

    private static List<IllustrationIncome>? GetTimeBasedIncome(TimelineSeries series)
    {
        if(series.Years == null || series.Years.Count == 0)
        {
            return null;
        }

        var incomeList = new List<IllustrationIncome>();
        TimelineYear? previousYear = null;

        foreach(var year in series.Years)
        {
            incomeList.Add(new IllustrationIncome
            {
                Year = year.Year,
                MonthlyAmount = year.MonthlyTotal,
                AnnualAmount = year.AnnualTotal,
                LumpSumAmount = year.LumpSumTotal,
                Difference = previousYear == null ? null : year.MonthlyTotal - previousYear.MonthlyTotal
            });

            previousYear = year;
        }

        return incomeList;
    }
}

