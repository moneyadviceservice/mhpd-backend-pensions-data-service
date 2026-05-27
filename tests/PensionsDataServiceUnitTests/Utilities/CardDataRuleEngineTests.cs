using Moq;
using PensionsDataService.Models;
using PensionsDataService.Utilities;
using System.Text.Json.Nodes;
using static MhpdCommon.ViewData.EvaluationConstants;

namespace PensionsDataServiceUnitTests.Utilities;

public class CardDataRuleEngineTests
{
    private readonly Mock<IPensionNavigator> _navigator = new();
    private CardDataRuleEngine _ruleEngine;

    public CardDataRuleEngineTests()
    {
        _ruleEngine = new(_navigator.Object);
    }

    [Fact]
    public void BothSelectors_AreCalled()
    {
        var rr = JsonNode.Parse("{}")!;
        var illustration = JsonNode.Parse("{}")!;

        _navigator.Setup(x => x.SelectIllustrationComponent(rr, PayableDetailsType.Recurring))
            .Returns(illustration);

        var prop = new PensionProperties();

        _navigator.Setup(x => x.SelectPensionProperties(rr))
            .Returns(prop);

        var details = new PayableDetails
        {
            MonthlyAmount = 200,
            LumpSumAmount = 50000,
            UnavailableCode = "ERR"
        };

        _navigator.Setup(x => x.GetPayableDetails(illustration))
            .Returns(details);

        var cardData = _ruleEngine.Evaluate(rr, Category.Confirmed);

        _navigator.Verify(x => x.SelectIllustrationComponent(rr, PayableDetailsType.Recurring), Times.Once);
        _navigator.Verify(x => x.GetPayableDetails(illustration), Times.Once);
        _navigator.Verify(x => x.SelectPensionProperties(illustration), Times.Once);
        Assert.Equal(details.MonthlyAmount, cardData.MonthlyAmount);
        Assert.Equal(details.LumpSumAmount, cardData.LumpSumAmount);
        Assert.Equal(details.BenefitType, cardData.BenefitType);
    }

    [Fact]
    public void NavigatorSelectors_AreNotCalled_WhenCategory_IsNotConfirmed()
    {
        var rr = JsonNode.Parse("{}")!;
        var illustration = JsonNode.Parse("{}")!;

        _navigator.Setup(x => x.SelectIllustrationComponent(rr, PayableDetailsType.Recurring))
            .Returns(illustration);

        var prop = new PensionProperties();

        _navigator.Setup(x => x.SelectPensionProperties(rr))
            .Returns(prop);

        var details = new PayableDetails
        {
            MonthlyAmount = 200,
            LumpSumAmount = 50000,
            UnavailableCode = "ERR"
        };

        _navigator.Setup(x => x.GetPayableDetails(illustration))
            .Returns(details);

        var cardData = _ruleEngine.Evaluate(rr, Category.Pending);

        _navigator.Verify(x => x.SelectIllustrationComponent(rr, PayableDetailsType.Recurring), Times.Once);
        _navigator.Verify(x => x.SelectRetirementDate(rr, illustration), Times.Once);
        Assert.Null(cardData.MonthlyAmount);
        Assert.Null(cardData.LumpSumAmount);
        Assert.Equal(details.BenefitType, cardData.BenefitType);
    }

    [Fact]
    public void WhenEriCode_IsDB_APCodeIsReturned()
    {
        // Arrange
        var record = TestData.CreatePensionWithUnavailableCodeDB();
        JsonNode arrangement = JsonNode.Parse(record.RetrievalResult!.GetRawText())!;
        _ruleEngine = new CardDataRuleEngine(new PensionNavigator());

        // Act
        var cardData = _ruleEngine.Evaluate(arrangement, Category.Confirmed);

        // Assert
        Assert.NotNull(cardData);
        Assert.Equal("WU", cardData.UnavailableCode);
        Assert.Null(cardData.MonthlyAmount);
        Assert.Equal("2040-01-01", cardData.RetirementDate?.ToString("yyyy-MM-dd"));
    }

    [Fact]
    public void WhenPensionType_IsNotCB_RecurringDetailsIsReturned()
    {
        // Arrange
        var record = TestData.CreateDCPension();
        JsonNode arrangement = JsonNode.Parse(record.RetrievalResult!.GetRawText())!;
        _ruleEngine = new CardDataRuleEngine(new PensionNavigator());

        // Act
        var cardData = _ruleEngine.Evaluate(arrangement, Category.Confirmed);

        // Assert
        Assert.NotNull(cardData);
        Assert.Null(cardData.UnavailableCode);
        Assert.Equal(3000, cardData.MonthlyAmount);
        Assert.Null(cardData.LumpSumAmount);
        Assert.Equal("DC", cardData.BenefitType);
        Assert.Equal("2038-01-01", cardData.RetirementDate?.ToString("yyyy-MM-dd"));
    }

    [Fact]
    public void WhenPensionType_IsCBL_LumpsumDetailsIsReturned()
    {
        // Arrange
        var record = TestData.CreateCBLPension();
        JsonNode arrangement = JsonNode.Parse(record.RetrievalResult!.GetRawText())!;
        _ruleEngine = new CardDataRuleEngine(new PensionNavigator());

        // Act
        var cardData = _ruleEngine.Evaluate(arrangement, Category.Confirmed);

        // Assert
        Assert.NotNull(cardData);
        Assert.Null(cardData.UnavailableCode);
        Assert.Equal(28500, cardData.LumpSumAmount);
        Assert.Null(cardData.MonthlyAmount);
        Assert.Equal("CBL", cardData.BenefitType);
        Assert.Equal("2040-04-01", cardData.RetirementDate?.ToString("yyyy-MM-dd"));
    }

    [Fact]
    public void WhenPensionType_IsCBS_RecurringDetailsIsReturned()
    {
        // Arrange
        var record = TestData.CreateCBSPension();
        JsonNode arrangement = JsonNode.Parse(record.RetrievalResult!.GetRawText())!;
        _ruleEngine = new CardDataRuleEngine(new PensionNavigator());

        // Act
        var cardData = _ruleEngine.Evaluate(arrangement, Category.Confirmed);

        // Assert
        Assert.NotNull(cardData);
        Assert.Null(cardData.UnavailableCode);
        Assert.Null(cardData.LumpSumAmount);
        Assert.Equal(3000, cardData.MonthlyAmount);
        Assert.Equal("CBS", cardData.BenefitType);
        Assert.Equal("2040-04-01", cardData.RetirementDate?.ToString("yyyy-MM-dd"));
    }
}

