using MhpdCommon.ViewData;
using Moq;
using PensionsDataService.Utilities;
using System.Text.Json.Nodes;

namespace PensionsDataServiceUnitTests.Utilities;

public class CardDataRuleEngineTests
{
    private readonly Mock<IPensionNavigator> _navigator = new();
    private readonly CardDataRuleEngine _ruleEngine;

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
}

