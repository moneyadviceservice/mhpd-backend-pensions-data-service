using MhpdCommon.Constants;
using MhpdCommon.Models.MHPDModels;
using PensionsDataService.Extensions;
using PensionsDataService.Models;
using PensionsDataService.Utilities;
using System.Text.Json;
using System.Text.Json.Nodes;
using static MhpdCommon.ViewData.EvaluationConstants;

namespace PensionsDataServiceUnitTests.Extensions;

public class PensionDataExtensionsTests
{
    private readonly PensionNavigator _navigator;
    private readonly SummaryDataRuleEngine _engine;

    public PensionDataExtensionsTests()
    {
        _navigator = new PensionNavigator();
        _engine = new SummaryDataRuleEngine(_navigator);
    }

    [Fact]
    public void EnrichSummaryData_DoesNothing_WhenPensionsListIsEmpty()
    {
        var response = new PensionData();

        response.EnrichSummaryData([], Category.Confirmed, _engine);

        Assert.Null(response.SummaryData);
    }

    [Fact]
    public void EnrichSummaryData_DoesNothing_WhenNoStatePensionExists()
    {
        var pensions = new List<RetrievedPensionRecord>
        {
            CreatePension("DC", "2030-01-01", "2040-01-01", 1000, 12000)
        };

        var response = new PensionData();

        response.EnrichSummaryData(pensions, Category.Confirmed, _engine);

        Assert.Null(response.SummaryData?.LegacyPayment);
        Assert.Null(response.SummaryData?.AlternativePayment);
        Assert.NotNull(response.SummaryData?.StandardPayment);
    }

    [Fact]
    public void EnrichSummaryData_SetsSummaryData_WhenStatePensionExistsAndHasValues()
    {
        var statePension = CreatePension(Constants.PensionTypes.SP, "2035-01-01");
        var pension1 = CreatePension("DC", "2030-01-01", "2040-01-01", 1000, 12000);
        var pension2 = CreatePension("DC", "2034-01-01", "2050-01-01", 500, 6000);

        var pensions = new List<RetrievedPensionRecord>
        {
            statePension,
            pension1,
            pension2
        };

        var response = new PensionData();

        response.EnrichSummaryData(pensions, Category.Confirmed, _engine);

        Assert.NotNull(response.SummaryData);
        Assert.Equal(1500, response.SummaryData!.StandardPayment!.MonthlyAmount);
        Assert.Equal(18000, response.SummaryData.StandardPayment.AnnualAmount);
        Assert.Equal("2035-01-01", response.SummaryData.StatePensionDate);
    }

    [Fact]
    public void EnrichSummaryData_SetsSummaryData_ExcludesPensionsInDifferentCategory()
    {
        var statePension = CreatePension(Constants.PensionTypes.SP, "2035-01-01");
        var pension1 = CreatePension("DC", "2030-01-01", "2040-01-01", 1000, 12000, category: Category.Pending);
        var pension2 = CreatePension("DC", "2034-01-01", "2050-01-01", 500, 6000);

        var pensions = new List<RetrievedPensionRecord>
        {
            statePension,
            pension1,
            pension2
        };

        var response = new PensionData();

        response.EnrichSummaryData(pensions, Category.Confirmed, _engine);

        Assert.NotNull(response.SummaryData);
        Assert.Equal(500, response.SummaryData!.StandardPayment!.MonthlyAmount);
        Assert.Equal(6000, response.SummaryData.StandardPayment.AnnualAmount);
        Assert.Equal("2035-01-01", response.SummaryData.StatePensionDate);
    }

    [Fact]
    public void EnrichSummaryData_SetsSummaryData_ExcludesPensionsWithoutIncome()
    {
        var statePension = CreatePension(Constants.PensionTypes.SP, "2035-01-01");
        var pension1 = CreatePension("DC", "2030-01-01", "2040-01-01", 1000, 12000);
        var pension2 = CreatePension("DC", "2034-01-01", "2050-01-01", 500, 6000, false);

        var pensions = new List<RetrievedPensionRecord>
        {
            statePension,
            pension1,
            pension2
        };

        var response = new PensionData();

        response.EnrichSummaryData(pensions, Category.Confirmed, _engine);

        Assert.NotNull(response.SummaryData);
        Assert.Equal(1000, response.SummaryData!.StandardPayment!.MonthlyAmount);
        Assert.Equal(12000, response.SummaryData.StandardPayment.AnnualAmount);
        Assert.Equal("2035-01-01", response.SummaryData.StatePensionDate);
    }

    [Fact]
    public void EnrichSummaryData_DoesNotSetSummaryData_WhenRuleEngineReturnsEmpty()
    {
        var statePension = CreatePension(Constants.PensionTypes.SP, null);

        var pensions = new List<RetrievedPensionRecord>
        {
            statePension
        };

        var response = new PensionData();

        response.EnrichSummaryData(pensions, Category.Confirmed, _engine);

        Assert.Null(response.SummaryData?.LegacyPayment);
        Assert.Null(response.SummaryData?.AlternativePayment);
        Assert.NotNull(response.SummaryData?.StandardPayment);
    }

    [Fact]
    public void EnrichSummaryData_DoesNotThrow_OnMalformedJson()
    {
        var malformed = new RetrievedPensionRecord
        {
            PensionType = Constants.PensionTypes.SP,
            RetrievalResult = JsonSerializer.SerializeToElement(new JsonObject())
        };

        var pensions = new List<RetrievedPensionRecord>
        {
            malformed
        };

        var response = new PensionData();

        var exception = Record.Exception(() =>
        {
            response.EnrichSummaryData(pensions, Category.Confirmed, _engine);
        });

        Assert.Null(exception);
    }

    [Fact]
    public void EnrichSummaryData_IgnoresPensionsOutsideWindow()
    {
        var statePension = CreatePension(Constants.PensionTypes.SP, "2035-01-01");

        var pension = CreatePension("DC", "2036-01-01", "2040-01-01", 1000, 12000);

        var pensions = new List<RetrievedPensionRecord>
        {
            statePension,
            pension
        };

        var response = new PensionData();

        response.EnrichSummaryData(pensions, Category.Confirmed, _engine);

        Assert.NotNull(response.SummaryData);
        Assert.Equal(0, response.SummaryData!.StandardPayment!.MonthlyAmount);
        Assert.Equal(0, response.SummaryData.StandardPayment.AnnualAmount);
    }

    private static RetrievedPensionRecord CreatePension(
        string pensionType,
        string? payableDate,
        string? lastPaymentDate = null,
        decimal? monthlyAmount = null,
        decimal? annualAmount = null,
        bool hasIncome = true,
        string category = Category.Confirmed)
    {
        var component = new JsonObject
        {
            [PensionConstants.IllustrationType] = IllustrationType.Estimated,
            [PensionConstants.PayableDetails] = new JsonObject()
        };

        if (payableDate != null)
            component[PensionConstants.PayableDetails]![PensionConstants.PayableDate] = payableDate;

        if (lastPaymentDate != null)
            component[PensionConstants.PayableDetails]![PensionConstants.LastPaymentDate] = lastPaymentDate;

        if (monthlyAmount.HasValue)
        {
            component[PensionConstants.PayableDetails]![PensionConstants.MonthlyAmount] = monthlyAmount.Value;
            component[PensionConstants.PayableDetails]![PensionConstants.AmountType] = AmountType.INC;
        }

        if (annualAmount.HasValue)
            component[PensionConstants.PayableDetails]![PensionConstants.AnnualAmount] = annualAmount.Value;

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
            Category = category,
            HasIncome = hasIncome,
            RetrievalResult = JsonSerializer.SerializeToElement(root)
        };
    }
}
