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

        Assert.Equal(2400, result.MonthlyTotal);
        Assert.Equal(27600, result.AnnualTotal);
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

        Assert.Equal(0, result.MonthlyTotal);
        Assert.Equal(0, result.AnnualTotal);
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

        Assert.Equal(0, result.MonthlyTotal);
        Assert.Equal(0, result.AnnualTotal);
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

        Assert.Equal(0, result.MonthlyTotal);
        Assert.Equal(0, result.AnnualTotal);
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
        Assert.Equal(0, result.MonthlyTotal);
        Assert.Equal(0, result.AnnualTotal);
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
            [PensionConstants.BenefitIllustrations] = new JsonArray(illustration)
        };

        return new RetrievedPensionRecord
        {
            PensionType = pensionType,
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
            RetrievalResult = JsonSerializer.SerializeToElement(root)
        };
    }
}

public record IllustrationData(
    string PayableDetailsType,
    decimal? MonthlyAmount = null,
    decimal? AnnualAmount = null,
    string? PayableDate = null,
    string? LastPaymentDate = null);
