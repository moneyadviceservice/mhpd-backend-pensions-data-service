using MhpdCommon.Constants;
using PensionsDataService.Extensions;
using PensionsDataService.Models;
using PensionsDataService.Utilities;
using System.Text.Json.Nodes;

namespace PensionsDataServiceUnitTests.Utilities;

public class DetailDataRuleEngineTests
{
    private readonly IPensionNavigator _navigator;
    private readonly ITimelineArrangementFactory _arrangementFactory;
    private readonly ITimelineSeriesBuilder _seriesBuilder;
    private readonly DetailDataRuleEngine _engine;

    public DetailDataRuleEngineTests()
    {
        _navigator = new PensionNavigator();
        _arrangementFactory = new TimelineArrangementFactory(_navigator);
        _seriesBuilder = new TimelineSeriesBuilder(_arrangementFactory);
        _engine = new DetailDataRuleEngine(_navigator, _seriesBuilder);
    }

    [Fact]
    public void EnrichDetailData_AppendsDetailData_WhenValidIllustrationExists()
    {
        // Arrange
        var record = TestData.CreateConfirmedPensionWithDetailData();

        // Act
        JsonNode arrangement = record.EnrichDetailData(_engine);

        // Assert
        var detail = arrangement[Constants.PensionDetail.DetailData];
        Assert.NotNull(detail);

        Assert.Equal("2040-02-01", detail![PensionConstants.RetirementDate]!.GetValue<string>());
        Assert.Equal(3200m, detail[Constants.PensionDetail.StandardPayment]![PensionConstants.MonthlyAmount]!.GetValue<decimal>());
        Assert.Equal("2040-02-01", detail[Constants.PensionDetail.StandardPayment]![PensionConstants.PayableDate]!.GetValue<string>());
        Assert.Equal(125000m, detail[Constants.PensionDetail.StandardPayment]!["potValue"]!.GetValue<decimal>());
        Assert.Equal(25000m, detail[Constants.PensionDetail.StandardPayment]!["lumpSumAmount"]!.GetValue<decimal>());
        Assert.Equal("2040-04-01", detail[Constants.PensionDetail.StandardPayment]!["lumpSumPayableDate"]!.GetValue<string>());
        Assert.Equal("DB", detail[Constants.PensionDetail.StandardPayment]![PensionConstants.BenefitType]!.GetValue<string>());
    }

    [Fact]
    public void EnrichDetailData_AppendsBenefitType_WhenPensionIsCBL()
    {
        // Arrange
        var record = TestData.CreateCBLPension();

        // Act
        JsonNode arrangement = record.EnrichDetailData(_engine);

        // Assert
        var detail = arrangement[Constants.PensionDetail.DetailData];
        Assert.NotNull(detail);

        Assert.Equal("2040-04-01", detail![PensionConstants.RetirementDate]!.GetValue<string>());
        Assert.Equal(28500m, detail[Constants.PensionDetail.StandardPayment]!["lumpSumAmount"]!.GetValue<decimal>());
        Assert.Equal("2040-04-01", detail[Constants.PensionDetail.StandardPayment]!["lumpSumPayableDate"]!.GetValue<string>());
        Assert.Equal("CBL", detail[Constants.PensionDetail.StandardPayment]![PensionConstants.BenefitType]!.GetValue<string>());
    }

    [Fact]
    public void EnrichDetailData_IncludesWarnings_WhenPresent()
    {
        // Arrange
        var record = TestData.CreateConfirmedPensionWithDetailData();

        // Act
        var arrangement = record.EnrichDetailData(_engine);

        // Assert
        var warningsNode = arrangement[Constants.PensionDetail.DetailData]!["warnings"]!.AsArray();

        var warnings = ((IEnumerable<JsonNode?>)warningsNode)
            .Select(w => w!.GetValue<string>())
            .ToList();

        Assert.Equal(2, warnings.Count);
        Assert.Contains("PSO", warnings);
        Assert.Contains("PNR", warnings);
    }

    [Fact]
    public void EnrichDetailData_OmitsOptionalFields_WhenMissing()
    {
        // Arrange
        var record = TestData.CreatePensionWithoutLumpSum();

        // Act
        var arrangement = record.EnrichDetailData(_engine);

        // Assert
        var detail = arrangement[Constants.PensionDetail.DetailData];
        Assert.NotNull(detail);

        Assert.NotNull(detail[PensionConstants.RetirementDate]);
        Assert.NotNull(detail[Constants.PensionDetail.StandardPayment]![PensionConstants.MonthlyAmount]);
        Assert.NotNull(detail[Constants.PensionDetail.StandardPayment]![PensionConstants.BenefitType]);
        Assert.NotNull(detail[Constants.PensionDetail.StandardPayment]![PensionConstants.PayableDate]);
        Assert.Null(detail[Constants.PensionDetail.StandardPayment]!["potValue"]);
        Assert.Null(detail[Constants.PensionDetail.StandardPayment]!["lumpSumAmount"]);
        Assert.Equal("NEW", detail[Constants.PensionDetail.UnavailableReasonCodes]![0]!.GetValue<string>());
        Assert.Null(detail[Constants.PensionDetail.Warnings]);
    }

    [Fact]
    public void EnrichDetailData_DoesNothing_WhenNoIllustrationsExist()
    {
        // Arrange
        var record = TestData.CreatePensionWithoutIllustrations();

        // Act
        var arrangement = record.EnrichDetailData(_engine);

        // Assert
        var detail = arrangement[Constants.PensionDetail.DetailData];
        Assert.NotNull(detail);
        Assert.NotNull(detail[PensionConstants.RetirementDate]);
        Assert.Null(detail[Constants.PensionDetail.StandardPayment]);
        Assert.Null(detail[Constants.PensionDetail.Warnings]);
        Assert.Null(detail[Constants.PensionDetail.UnavailableReasonCodes]);
    }

    [Fact]
    public void EnrichDetailData_PensionIsDCType_EnrichesWithIncomeTimeline()
    {
        // Arrange
        var record = TestData.CreateDCPension();

        // Act
        var arrangement = record.EnrichDetailData(_engine);

        // Assert
        var detail = arrangement[Constants.PensionDetail.DetailData];
        Assert.NotNull(detail);
        Assert.Equal(2038, detail[Constants.PensionDetail.IncomeAndValues]?
            [Constants.PensionDetail.StandardIncome]?[0]?["year"]!.GetValue<int>());
        Assert.Equal(3000, detail[Constants.PensionDetail.IncomeAndValues]?
            [Constants.PensionDetail.StandardIncome]?[0]?["monthlyAmount"]!.GetValue<decimal>());
        Assert.Equal(36000, detail[Constants.PensionDetail.IncomeAndValues]?
            [Constants.PensionDetail.StandardIncome]?[0]?["annualAmount"]!.GetValue<decimal>());
    }

    [Fact]
    public void EnrichDetailData_PensionIsAVCType_EnrichesWithDetailAndTimeline()
    {
        // Arrange
        var record = TestData.CreateAVCPension();

        // Act
        var arrangement = record.EnrichDetailData(_engine);

        // Assert
        var detail = arrangement[Constants.PensionDetail.DetailData];
        Assert.NotNull(detail);
        Assert.Equal("SML", detail[Constants.PensionDetail.UnavailableReasonCodes]?[0]?.GetValue<string>());
    }

    [Fact]
    public void EnrichDetailData_PensionIsMcCloud_EnrichesWithDetailAndTimeline()
    {
        // Arrange
        var record = TestData.CreateMcCloudPension();

        // Act
        var arrangement = record.EnrichDetailData(_engine);

        // Assert
        var detail = arrangement[Constants.PensionDetail.DetailData];
        Assert.NotNull(detail);
        Assert.Null(detail[Constants.PensionDetail.StatePayment]);
        Assert.Null(detail[Constants.PensionDetail.StandardPayment]);
        Assert.NotNull(detail[Constants.PensionDetail.LegacyPayment]);
        Assert.NotNull(detail[Constants.PensionDetail.AlternativePayment]);

        Assert.Null(detail[Constants.PensionDetail.IncomeAndValues]?[Constants.PensionDetail.StandardIncome]);
        Assert.NotNull(detail[Constants.PensionDetail.IncomeAndValues]?[Constants.PensionDetail.LegacyIncome]);
        Assert.NotNull(detail[Constants.PensionDetail.IncomeAndValues]?[Constants.PensionDetail.AlternativeIncome]);

        Assert.Equal(2038, detail[Constants.PensionDetail.IncomeAndValues]?
            [Constants.PensionDetail.LegacyIncome]?[0]?["year"]!.GetValue<int>());
        Assert.Equal(2040, detail[Constants.PensionDetail.IncomeAndValues]?
            [Constants.PensionDetail.LegacyIncome]?[1]?["year"]!.GetValue<int>());
        Assert.Equal(3000, detail[Constants.PensionDetail.IncomeAndValues]?
            [Constants.PensionDetail.LegacyIncome]?[0]?["monthlyAmount"]!.GetValue<decimal>());
        Assert.Equal(7000, detail[Constants.PensionDetail.IncomeAndValues]?
            [Constants.PensionDetail.LegacyIncome]?[1]?["monthlyAmount"]!.GetValue<decimal>());
        Assert.Equal(6000, detail[Constants.PensionDetail.IncomeAndValues]?
            [Constants.PensionDetail.AlternativeIncome]?[1]?["monthlyAmount"]!.GetValue<decimal>());
    }
}
