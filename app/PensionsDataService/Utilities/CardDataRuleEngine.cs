using MhpdCommon.ViewData;
using PensionsDataService.Models;
using System.Text.Json.Nodes;

namespace PensionsDataService.Utilities;

public sealed class CardDataRuleEngine(IPensionNavigator navigator) : ICardDataRuleEngine
{
    public CardData Evaluate(JsonNode retrievalResult, string category)
    {
        var earliestComponent = navigator.SelectIllustrationComponent(retrievalResult);

        return new CardData
        {
            RetirementDate = navigator.SelectRetirementDate(retrievalResult, earliestComponent),
            MonthlyAmount = category == EvaluationConstants.Category.Confirmed ? navigator.SelectMonthlyAmount(earliestComponent) : null,
            UnavailableCode = category == EvaluationConstants.Category.Confirmed ? navigator.SelectUnavailableCode(earliestComponent) : null
        };
    }
}
