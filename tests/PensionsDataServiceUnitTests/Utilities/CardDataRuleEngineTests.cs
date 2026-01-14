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

        _navigator.Setup(x => x.SelectLatestIllustration(rr))
            .Returns(illustration);

        _navigator.Setup(x => x.SelectEarliestComponent(illustration, It.IsAny<string>()))
            .Returns((JsonNode?)null);

        _ruleEngine.Evaluate(rr, EvaluationConstants.Category.Confirmed);

        _navigator.Verify(x => x.SelectLatestIllustration(rr), Times.Once);
        _navigator.Verify(x => x.SelectEarliestComponent(illustration, It.IsAny<string>()), Times.Once);
        _navigator.Verify(x => x.SelectMonthlyAmount(It.IsAny<JsonNode>()), Times.Once);
        _navigator.Verify(x => x.SelectUnavailableCode(It.IsAny<JsonNode>()), Times.Once);
        _navigator.Verify(x => x.SelectRetirementDate(rr, It.IsAny<JsonNode?>()), Times.Once);
    }

    [Fact]
    public void ComponentSelector_IsNotCalled()
    {
        var rr = JsonNode.Parse("{}")!;

        _navigator.Setup(x => x.SelectLatestIllustration(rr))
            .Returns((JsonNode?)null);

        _navigator.Setup(x => x.SelectEarliestComponent(rr, It.IsAny<string>()))
            .Returns((JsonNode?)null);

        _ruleEngine.Evaluate(rr, EvaluationConstants.Category.Confirmed);

        _navigator.Verify(x => x.SelectLatestIllustration(rr), Times.Once);
        _navigator.Verify(x => x.SelectEarliestComponent(It.IsAny<JsonNode>(), It.IsAny<string>()), Times.Never);
        _navigator.Verify(x => x.SelectMonthlyAmount(It.IsAny<JsonNode>()), Times.Once);
        _navigator.Verify(x => x.SelectUnavailableCode(It.IsAny<JsonNode>()), Times.Once);
        _navigator.Verify(x => x.SelectRetirementDate(rr, null), Times.Once);
    }

    [Fact]
    public void NavigatorSelectors_AreNotCalled_WhenCategory_IsNotConfirmed()
    {
        var rr = JsonNode.Parse("{}")!;
        var illustration = JsonNode.Parse("{}")!;

        _navigator.Setup(x => x.SelectLatestIllustration(rr))
            .Returns(illustration);

        _navigator.Setup(x => x.SelectEarliestComponent(illustration, It.IsAny<string>()))
            .Returns((JsonNode?)null);

        _ruleEngine.Evaluate(rr, EvaluationConstants.Category.Pending);

        _navigator.Verify(x => x.SelectLatestIllustration(rr), Times.Once);
        _navigator.Verify(x => x.SelectEarliestComponent(illustration, It.IsAny<string>()), Times.Once);
        _navigator.Verify(x => x.SelectMonthlyAmount(It.IsAny<JsonNode>()), Times.Never);
        _navigator.Verify(x => x.SelectUnavailableCode(It.IsAny<JsonNode>()), Times.Never);
        _navigator.Verify(x => x.SelectRetirementDate(rr, It.IsAny<JsonNode?>()), Times.Once);
    }
}

