using MhpdCommon.Constants;
using PensionsDataService.Extensions;
using PensionsDataService.Utilities;
using System.Text.Json.Nodes;

namespace PensionsDataServiceUnitTests.Utilities;

public class DetailDataRuleEngineTests
{
    private readonly IPensionNavigator _navigator;
    private readonly IDetailDataRuleEngine _engine;

    public DetailDataRuleEngineTests()
    {
        _navigator = new PensionNavigator();
        _engine = new DetailDataRuleEngine(_navigator);
    }

    [Fact]
    public void EnrichDetailData_AppendsDetailData_WhenValidIllustrationExists()
    {
        // Arrange
        var record = TestData.CreateConfirmedPensionWithDetailData();
        JsonNode arrangement = JsonNode.Parse(record.RetrievalResult!.GetRawText())!;

        // Act
        arrangement.EnrichDetailData(_engine);

        // Assert
        var detail = arrangement["detailData"];
        Assert.NotNull(detail);

        Assert.Equal("2040-02-01", detail![PensionConstants.RetirementDate]!.GetValue<string>());
        Assert.Equal("2025-01-01", detail[PensionConstants.IllustrationDate]!.GetValue<string>());
        Assert.Equal(3200m, detail[PensionConstants.MonthlyAmount]!.GetValue<decimal>());
        Assert.Equal("2040-02-01", detail[PensionConstants.PayableDate]!.GetValue<string>());
        Assert.Equal(125000m, detail["potValue"]!.GetValue<decimal>());
        Assert.Equal(25000m, detail["lumpSumAmount"]!.GetValue<decimal>());
        Assert.Equal("2040-03-01", detail["lumpSumPayableDate"]!.GetValue<string>());
        Assert.Equal("DB", detail[PensionConstants.BenefitType]!.GetValue<string>());
    }

    [Fact]
    public void EnrichDetailData_IncludesWarnings_WhenPresent()
    {
        // Arrange
        var record = TestData.CreateConfirmedPensionWithDetailData();
        JsonNode arrangement = JsonNode.Parse(record.RetrievalResult!.GetRawText())!;

        // Act
        arrangement.EnrichDetailData(_engine);

        // Assert
        var warningsNode = arrangement["detailData"]!["warnings"]!.AsArray();

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
        JsonNode arrangement = JsonNode.Parse(record.RetrievalResult!.GetRawText())!;

        // Act
        arrangement.EnrichDetailData(_engine);

        // Assert
        var detail = arrangement["detailData"];
        Assert.NotNull(detail);

        Assert.NotNull(detail[PensionConstants.RetirementDate]);
        Assert.NotNull(detail[PensionConstants.MonthlyAmount]);
        Assert.NotNull(detail[PensionConstants.BenefitType]);
        Assert.NotNull(detail[PensionConstants.IllustrationDate]);
        Assert.NotNull(detail[PensionConstants.PayableDate]);
        Assert.Null(detail["potValue"]);
        Assert.Null(detail["lumpSumAmount"]);
        Assert.Null(detail["unavailableCode"]);
        Assert.NotNull(detail["warnings"]);
    }

    [Fact]
    public void EnrichDetailData_DoesNothing_WhenNoIllustrationsExist()
    {
        // Arrange
        var record = TestData.CreatePensionWithoutIllustrations();
        JsonNode arrangement = JsonNode.Parse(record.RetrievalResult!.GetRawText())!;

        // Act
        arrangement.EnrichDetailData(_engine);

        // Assert
        var detail = arrangement["detailData"];
        Assert.NotNull(detail);
        Assert.Null(detail[PensionConstants.RetirementDate]);
        Assert.Null(detail[PensionConstants.MonthlyAmount]);
        Assert.Null(detail[PensionConstants.BenefitType]);
        Assert.Null(detail[PensionConstants.IllustrationDate]);
        Assert.Null(detail[PensionConstants.PayableDate]);
        Assert.Null(detail["potValue"]);
        Assert.Null(detail["lumpSumAmount"]);
        Assert.Null(detail["unavailableCode"]);
        Assert.NotNull(detail["warnings"]);
    }
}
