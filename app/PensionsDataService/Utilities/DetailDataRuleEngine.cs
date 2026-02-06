using MhpdCommon.Constants;
using MhpdCommon.Extensions;
using MhpdCommon.ViewData;
using PensionsDataService.Models;
using System.Text.Json.Nodes;
using static MhpdCommon.ViewData.PensionEnums;

namespace PensionsDataService.Utilities;

public sealed class DetailDataRuleEngine(IPensionNavigator navigator)
    : IDetailDataRuleEngine
{
    public DetailData Evaluate(JsonNode retrievalResult)
    {
        ArgumentNullException.ThrowIfNull(retrievalResult);

        var eriIllustration = navigator.SelectLatestIllustration(retrievalResult, EvaluationConstants.PayableDetailsType.Recurring);
        var recurringComponent = navigator.SelectEarliestIllustrationComponent(eriIllustration);
        var recurringEriComponent = navigator.SelectEarliestComponent(eriIllustration, EvaluationConstants.IllustrationType.Estimated);
        var recurringApComponent = navigator.SelectEarliestComponent(eriIllustration, EvaluationConstants.IllustrationType.Accrued);

        var lumpSumIllustration = navigator.SelectLatestIllustration(retrievalResult, EvaluationConstants.PayableDetailsType.LumpSum);
        var lumpSumEriComponent = navigator.SelectEarliestIllustrationComponent(lumpSumIllustration);
        var lumpSumApComponent = navigator.SelectEarliestComponent(lumpSumIllustration, EvaluationConstants.IllustrationType.Accrued);

        PayableDetails recurringPayableDetails = navigator.GetPayableDetails(recurringComponent);
        PayableDetails lumpSumPayableDetails = navigator.GetPayableDetails(lumpSumEriComponent);
        var eriIllustrtationDate = FormatDate(eriIllustration?[PensionConstants.IllustrationDate]?.GetValue<DateTime?>());
        var lumpSumIllustrationDate = FormatDate(lumpSumIllustration?[PensionConstants.IllustrationDate]?.GetValue<DateTime?>());

        var detailData = new DetailData
        {
            RetirementDate = FormatDate(navigator.SelectRetirementDate(retrievalResult, recurringComponent)),
            IllustrationDate = eriIllustrtationDate,
            MonthlyAmount = navigator.SelectMonthlyAmount(recurringComponent),
            PayableDate = FormatDate(recurringPayableDetails.PayableDate),
            PotValue = recurringApComponent?[PensionConstants.RetirementPot]?.GetValue<decimal?>(),
            LumpSumAmount = navigator.SelectLumpSumAmount(lumpSumEriComponent),
            LumpSumPayableDate = FormatDate((lumpSumPayableDetails.PayableDate)),
            BenefitType = recurringComponent?[PensionConstants.BenefitType]?.GetValue<string>(),
            UnavailableCode = navigator.SelectUnavailableCode(recurringComponent),
            Warnings = ExtractWarnings(recurringComponent, recurringApComponent)
        };

        // For DC and AVC pension types, there will be no lump sum component,
        // instead the retirement pot value from the recurring component will be used in the donut chart
        var pensionType = recurringEriComponent?[PensionConstants.BenefitType]?.GetValue<string?>();
        pensionType ??= retrievalResult[PensionConstants.PensionType]?.GetValue<string?>();
        bool fabricateLumpSum = pensionType == PensionType.DC.GetDisplayValue() || pensionType == PensionType.AVC.GetDisplayValue();

        detailData.IncomeAndValues.Add(ExtractIncomeAndValues(
            eriIllustrtationDate,
            recurringEriComponent,
            recurringApComponent,
            lumpSumIllustrationDate,
            lumpSumEriComponent,
            lumpSumApComponent,
            fabricateLumpSum));

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

    private IllustrationIncome ExtractIncomeAndValues(string? recurringDate, JsonNode? eriComponent, JsonNode? apComponent, 
        string? lumpSumDate, JsonNode? lumpSumEriComponent, JsonNode? lumpSumApComponent, bool fabricateLumpSum)
    { 
        JsonNode? eriPotComponent = null;
        JsonNode? apPotComponent = null;

        if (fabricateLumpSum)
        {
            // For DC and AVC pension types, there will be no lump sum component,
            // instead the retirement pot value from the recurring component will be used in the donut chart
            eriPotComponent = eriComponent;
            apPotComponent = apComponent;
            lumpSumDate = recurringDate;
        }

        IllustrationBarChart? barChart = null;
        IllustrationDonutChart? donutChart = null;

        if (eriComponent != null || apComponent!= null) 
        {
            barChart = new IllustrationBarChart
            {
                Eri = BuildRecurringChartData(eriComponent),
                Ap = BuildRecurringChartData(apComponent),
                IllustrationDate = recurringDate
            };
        }

        if (lumpSumEriComponent != null || lumpSumApComponent != null || eriPotComponent != null || apPotComponent != null)
        {
            donutChart = new IllustrationDonutChart
            {
                Eri = BuildLumpSumChartData(lumpSumEriComponent, eriPotComponent),
                Ap = BuildLumpSumChartData(lumpSumApComponent, apPotComponent),
                IllustrationDate = lumpSumDate
            };
        }

        var income = new IllustrationIncome
        {
            Bar = barChart,
            Donut = donutChart
        };

        return income;
    }

    private RecurringChartData BuildRecurringChartData(JsonNode? component)
    {
        var payableDetails = navigator.GetPayableDetails(component);

        return new RecurringChartData
        {
            MonthlyAmount = payableDetails.MonthlyAmount,
            AnnualAmount = payableDetails.AnnualAmount,
            PayableDate = FormatDate(payableDetails.PayableDate),
            BenefitType = component?[PensionConstants.BenefitType]?.GetValue<string?>(),
            CalculationMethod = component?[PensionConstants.CalculationMethod]?.GetValue<string?>(),
            Increasing = payableDetails.IsIncreasing,
            SafeguardedBenefit = component?[PensionConstants.SafeguardedBenefit]?.GetValue<bool>(),
            SurvivorBenefit = component?[PensionConstants.SurvivorBenefit]?.GetValue<bool>(),
            Warnings = ExtractWarnings(component)
        };
    }

    private LumpSumChartData BuildLumpSumChartData(JsonNode? lumpSumcomponent, JsonNode? potComponent)
    {
        var component = potComponent ?? lumpSumcomponent;
        var payableDetails = navigator.GetPayableDetails(component);

        var amount = payableDetails.LumpSumAmount;

        if(potComponent != null)
        {
            amount = potComponent[PensionConstants.RetirementPot]?.GetValue<decimal?>();
        }

        return new LumpSumChartData
        {
            Amount = amount,
            PayableDate = FormatDate(payableDetails.PayableDate),
            BenefitType = component?[PensionConstants.BenefitType]?.GetValue<string?>(),
            CalculationMethod = component?[PensionConstants.CalculationMethod]?.GetValue<string?>(),
            SafeguardedBenefit = component?[PensionConstants.SafeguardedBenefit]?.GetValue<bool>(),
            SurvivorBenefit = component?[PensionConstants.SurvivorBenefit]?.GetValue<bool>(),
            Warnings = ExtractWarnings(component)
        };
    }
}

