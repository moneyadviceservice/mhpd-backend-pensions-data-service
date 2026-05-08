using MhpdCommon.Constants;
using MhpdCommon.Extensions;
using MhpdCommon.Models.MHPDModels;
using MhpdCommon.ViewData;
using PensionsDataService.Models;
using PensionsDataService.Utilities;
using System.Text.Json;
using System.Text.Json.Nodes;
using static MhpdCommon.ViewData.EvaluationConstants;

namespace PensionsDataServiceUnitTests.Utilities;

public class SummaryDataRuleEngineTests
{
    private readonly PensionNavigator _navigator;
    private readonly SummaryDataRuleEngine _engine;

    public SummaryDataRuleEngineTests()
    {
        _navigator = new PensionNavigator();
        _engine = new SummaryDataRuleEngine(_navigator);
    }

    [Fact]
    public void Evaluate_ShouldSetStatePensionDate_FromStatePension()
    {
        var statePension = CreatePension(
            pensionType: Constants.PensionTypes.SP,
            payableDate: "2035-01-01");

        var pensions = new List<RetrievedPensionRecord> { statePension };

        var result = _engine.Evaluate(statePension, pensions);

        Assert.NotNull(result.StatePensionDate);
        Assert.Equal("2035-01-01", result.StatePensionDate);
    }

    [Fact]
    public void Evaluate_ShouldSumMonthlyAndAnnualTotals_WhenPensionsFallWithinWindow()
    {
        var statePension = CreatePension(
            pensionType: Constants.PensionTypes.SP,
            payableDate: "2035-01-01");

        var pension1 = CreatePension(
            payableDate: "2034-01-01",
            lastPaymentDate: "2040-01-01",
            monthlyAmount: 1000,
            annualAmount: 12000);

        var pension2 = CreatePension(
            payableDate: "2035-01-01",
            lastPaymentDate: "2050-01-01",
            monthlyAmount: 500,
            annualAmount: 6000);

        var multiPension = CreateMultiplicityPension(
        [
            new(PayableDetailsType.LumpSum, 300, 3600, "2033-01-01"),
            new(PayableDetailsType.Recurring, 400, 3600, "2034-01-01"),
            new(PayableDetailsType.Recurring, 500, 6000, "2035-01-01"),
            new(PayableDetailsType.Recurring, 600, 7200, "2036-01-01"),
        ]);

        var pensions = new List<RetrievedPensionRecord>
        {
            statePension,
            pension1,
            pension2,
            multiPension
        };

        var result = _engine.Evaluate(statePension, pensions);

        Assert.Equal(2400, result.StandardPayment!.MonthlyAmount);
        Assert.Equal(27600, result.StandardPayment.AnnualAmount);
        Assert.Null(result.LegacyPayment);
        Assert.Null(result.AlternativePayment);
    }

    [Fact]
    public void Evaluate_ShouldCreateLegacyAndNewPayments_WhenHasMcCloudPension()
    {
        var statePension = CreatePension(
            pensionType: Constants.PensionTypes.SP,
            payableDate: "2035-01-01");

        var pension1 = CreatePension(
            payableDate: "2034-01-01",
            lastPaymentDate: "2040-01-01",
            monthlyAmount: 1000,
            annualAmount: 12000);

        var pension2 = CreatePension(
            payableDate: "2035-01-01",
            lastPaymentDate: "2050-01-01",
            monthlyAmount: 500,
            annualAmount: 6000);

        var mcCloudPension = CreateMultiplicityPension(
        [
            new(PayableDetailsType.RecurringNew, 300, 3600, "2033-01-01"),
            new(PayableDetailsType.LumpSumNew, 400, 3600, "2034-01-01"),
            new(PayableDetailsType.RecurringLegacy, 500, 6000, "2035-01-01"),
            new(PayableDetailsType.LumpSumLegacy, 600, 7200, "2036-01-01"),
        ]);

        var pensions = new List<RetrievedPensionRecord>
        {
            statePension,
            pension1,
            pension2,
            mcCloudPension
        };

        var result = _engine.Evaluate(statePension, pensions);

        Assert.Equal(2000, result.LegacyPayment!.MonthlyAmount);
        Assert.Equal(24000, result.LegacyPayment.AnnualAmount);
        Assert.Equal(1800, result.AlternativePayment!.MonthlyAmount);
        Assert.Equal(21600, result.AlternativePayment.AnnualAmount);
        Assert.Null(result.StandardPayment);
    }

    [Fact]
    public void Evaluate_ShouldExcludePensions_StartingAfterStatePensionDate()
    {
        var statePension = CreatePension(
            pensionType: Constants.PensionTypes.SP,
            payableDate: "2035-01-01");

        var pension = CreatePension(
            payableDate: "2036-01-01",
            lastPaymentDate: "2050-01-01",
            monthlyAmount: 1000,
            annualAmount: 12000);

        var pensions = new List<RetrievedPensionRecord>
        {
            statePension,
            pension
        };

        var result = _engine.Evaluate(statePension, pensions);

        Assert.Equal(0, result.StandardPayment!.MonthlyAmount);
        Assert.Equal(0, result.StandardPayment.AnnualAmount);
    }

    [Fact]
    public void Evaluate_ShouldExcludePensions_EndingBeforeStatePensionDate()
    {
        var statePension = CreatePension(
            pensionType: Constants.PensionTypes.SP,
            payableDate: "2035-01-01");

        var pension = CreatePension(
            payableDate: "2030-01-01",
            lastPaymentDate: "2034-01-01",
            monthlyAmount: 1000,
            annualAmount: 12000);

        var pensions = new List<RetrievedPensionRecord>
        {
            statePension,
            pension
        };

        var result = _engine.Evaluate(statePension, pensions);

        Assert.Equal(0, result.StandardPayment!.MonthlyAmount);
        Assert.Equal(0, result.StandardPayment.AnnualAmount);
    }

    [Fact]
    public void Evaluate_ShouldIgnorePensions_WithMissingComponents()
    {
        var statePension = CreatePension(
            pensionType: Constants.PensionTypes.SP,
            payableDate: "2035-01-01");

        var pension = new RetrievedPensionRecord
        {
            PensionType = "DC",
            RetrievalResult = JsonSerializer.SerializeToElement(new JsonObject())
        };

        var pensions = new List<RetrievedPensionRecord>
        {
            statePension,
            pension
        };

        var result = _engine.Evaluate(statePension, pensions);

        Assert.Equal(0, result.StandardPayment!.MonthlyAmount);
        Assert.Equal(0, result.StandardPayment.AnnualAmount);
    }

    [Fact]
    public void Evaluate_ShouldReturnEmptySummary_WhenStatePensionDateIsNull()
    {
        var statePension = CreatePension(
            pensionType: Constants.PensionTypes.SP,
            payableDate: null);

        var pension = CreatePension(
            payableDate: "2030-01-01",
            lastPaymentDate: "2040-01-01",
            monthlyAmount: 1000,
            annualAmount: 12000);

        var pensions = new List<RetrievedPensionRecord>
        {
            statePension,
            pension
        };

        var result = _engine.Evaluate(statePension, pensions);

        Assert.Null(result.StatePensionDate);
        Assert.Null(result.StandardPayment);
    }

    // ---------------------------
    // Helpers
    // ---------------------------

    private static RetrievedPensionRecord CreatePension(
        string pensionType = "DC",
        string? payableDate = null,
        string? lastPaymentDate = null,
        decimal? monthlyAmount = null,
        decimal? annualAmount = null)
    {
        var component = new JsonObject
        {
            [PensionConstants.IllustrationType] = IllustrationType.Estimated,
            [PensionConstants.PayableDetails] = new JsonObject()
        };

        if (payableDate != null)
        {
            component[PensionConstants.PayableDetails]![PensionConstants.PayableDate] = payableDate;
        }

        if (lastPaymentDate != null)
        {
            component[PensionConstants.PayableDetails]![PensionConstants.LastPaymentDate] = lastPaymentDate;
        }


        if (monthlyAmount.HasValue)
        {
            component[PensionConstants.PayableDetails]![PensionConstants.MonthlyAmount] = monthlyAmount.Value;
            component[PensionConstants.PayableDetails]![PensionConstants.AmountType] = AmountType.INC;
        }

        if (annualAmount.HasValue)
        {
            component[PensionConstants.PayableDetails]![PensionConstants.AnnualAmount] = annualAmount.Value;
        }

        var illustration = new JsonObject
        {
            [PensionConstants.IllustrationDate] = "2024-01-01",
            [PensionConstants.PayableDetailsType] = PayableDetailsType.Recurring,
            [PensionConstants.IllustrationComponents] = new JsonArray(component)
        };

        var root = new JsonObject
        {
            [PensionConstants.PensionType] = pensionType,
            [PensionConstants.HasIncome] = true,
            [PensionConstants.HasMultipleIncomeOptions] = false,
            [PensionConstants.BenefitIllustrations] = new JsonArray(illustration)
        };

        return new RetrievedPensionRecord
        {
            PensionType = pensionType,
            HasIncome = monthlyAmount.HasValue,
            IsMcCloudPension = false,
            RetrievalResult = JsonSerializer.SerializeToElement(root)
        };
    }

    private static RetrievedPensionRecord CreateMultiplicityPension(List<IllustrationData> illustrationData)
    {
        var illustrations = illustrationData.Select(data =>
        {
            var component = new JsonObject
            {
                [PensionConstants.IllustrationType] = IllustrationType.Estimated,
                [PensionConstants.PayableDetails] = new JsonObject()
            };

            if (data.PayableDate != null)
            {
                component[PensionConstants.PayableDetails]![PensionConstants.PayableDate] = data.PayableDate;
            }

            if (data.LastPaymentDate != null)
            {
                component[PensionConstants.PayableDetails]![PensionConstants.LastPaymentDate] = data.LastPaymentDate;
            }

            if (data.MonthlyAmount.HasValue)
            {
                component[PensionConstants.PayableDetails]![PensionConstants.MonthlyAmount] = data.MonthlyAmount.Value;
                var amountType = data.PayableDetailsType switch
                {
                    PayableDetailsType.Recurring => AmountType.INC,
                    PayableDetailsType.LumpSum => AmountType.CSH,
                    PayableDetailsType.RecurringNew => AmountType.INCN,
                    PayableDetailsType.LumpSumNew => AmountType.CSHN,
                    PayableDetailsType.RecurringLegacy => AmountType.INCL,
                    PayableDetailsType.LumpSumLegacy => AmountType.CSHL,
                    _ => AmountType.INC
                };
            component[PensionConstants.PayableDetails]![PensionConstants.AmountType] = amountType;
            }

            if (data.AnnualAmount.HasValue)
            {
                component[PensionConstants.PayableDetails]![PensionConstants.AnnualAmount] = data.AnnualAmount.Value;
            }

            return new JsonObject
            {
                [PensionConstants.IllustrationDate] = "2024-01-01",
                [PensionConstants.PayableDetailsType] = data.PayableDetailsType,
                [PensionConstants.IllustrationComponents] = new JsonArray(component)
            };
        });

        var root = new JsonObject
        {
            [PensionConstants.BenefitIllustrations] = new JsonArray(illustrations.ToArray())
        };

        return new RetrievedPensionRecord
        {
            PensionType = PensionEnums.PensionType.DB.GetDisplayValue(),
            RetrievalResult = JsonSerializer.SerializeToElement(root),
            HasIncome = true
        };
    }
}

public record IllustrationData(
    string PayableDetailsType,
    decimal? MonthlyAmount = null,
    decimal? AnnualAmount = null,
    string? PayableDate = null,
    string? LastPaymentDate = null);
