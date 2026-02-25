using MhpdCommon.ViewData;
using Moq;
using PensionsDataService.Utilities;
using System.Text.Json.Nodes;

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

        _navigator.Setup(x => x.SelectIllustrationComponent(rr))
            .Returns(illustration);

        _ruleEngine.Evaluate(rr, EvaluationConstants.Category.Confirmed);

        _navigator.Verify(x => x.SelectIllustrationComponent(rr), Times.Once);
        _navigator.Verify(x => x.SelectMonthlyAmount(illustration), Times.Once);
        _navigator.Verify(x => x.SelectUnavailableCode(illustration), Times.Once);
        _navigator.Verify(x => x.SelectRetirementDate(rr, illustration), Times.Once);
    }

    [Fact]
    public void NavigatorSelectors_AreNotCalled_WhenCategory_IsNotConfirmed()
    {
        var rr = JsonNode.Parse("{}")!;
        var illustration = JsonNode.Parse("{}")!;

        _navigator.Setup(x => x.SelectIllustrationComponent(rr))
            .Returns(illustration);

        _ruleEngine.Evaluate(rr, EvaluationConstants.Category.Pending);

        _navigator.Verify(x => x.SelectIllustrationComponent(rr), Times.Once);
        _navigator.Verify(x => x.SelectMonthlyAmount(It.IsAny<JsonNode>()), Times.Never);
        _navigator.Verify(x => x.SelectUnavailableCode(It.IsAny<JsonNode>()), Times.Never);
        _navigator.Verify(x => x.SelectRetirementDate(rr, illustration), Times.Once);
    }

    [Fact]
    public void WhenEriCode_IsDB_APCodeIsReturned()
    {
        // Arrange
        var record = TestData.CreatePensionWithUnavailableCodeDB();
        JsonNode arrangement = JsonNode.Parse(record.RetrievalResult!.GetRawText())!;
        _ruleEngine = new CardDataRuleEngine(new PensionNavigator());

        // Act
        var cardData = _ruleEngine.Evaluate(arrangement, EvaluationConstants.Category.Confirmed);

        // Assert
        Assert.NotNull(cardData);
        Assert.Equal("WU", cardData.UnavailableCode);
        Assert.Null(cardData.MonthlyAmount);
        Assert.Equal("2040-01-01", cardData.RetirementDate?.ToString("yyyy-MM-dd"));
    }
}

