using MhpdCommon.Constants;
using MhpdCommon.Models.MHPDModels;
using MhpdCommon.ViewData;
using PensionsDataService.Models;
using PensionsDataService.Utilities;
using System.Text.Json;
using System.Text.Json.Nodes;

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

        var pensions = new List<RetrievedPensionRecord>
        {
            statePension,
            pension1,
            pension2
        };

        var result = _engine.Evaluate(statePension, pensions);

        Assert.Equal(1500, result.MonthlyTotal);
        Assert.Equal(18000, result.AnnualTotal);
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
            [PensionConstants.IllustrationType] = EvaluationConstants.IllustrationType.Estimated,
            [PensionConstants.PayableDetails] = new JsonObject()
        };

        if (payableDate != null)
            component[PensionConstants.PayableDetails]![PensionConstants.PayableDate] = payableDate;

        if (lastPaymentDate != null)
            component[PensionConstants.PayableDetails]![PensionConstants.LastPaymentDate] = lastPaymentDate;

        if (monthlyAmount.HasValue)
            component[PensionConstants.PayableDetails]![PensionConstants.MonthlyAmount] = monthlyAmount.Value;

        if (annualAmount.HasValue)
            component[PensionConstants.PayableDetails]![PensionConstants.AnnualAmount] = annualAmount.Value;

        var illustration = new JsonObject
        {
            [PensionConstants.IllustrationDate] = "2024-01-01",
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
}
