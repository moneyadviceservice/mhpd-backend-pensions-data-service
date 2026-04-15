using PensionsDataService.Models;
using System.Text.Json.Nodes;
using static MhpdCommon.ViewData.EvaluationConstants;

namespace PensionsDataService.Utilities;

public sealed class CardDataRuleEngine(IPensionNavigator navigator) : ICardDataRuleEngine
{
    public CardData Evaluate(JsonNode retrievalResult, string category)
    {
        var earliestComponent = navigator.SelectIllustrationComponent(retrievalResult, PayableDetailsType.Recurring);

        return new CardData
        {
            RetirementDate = navigator.SelectRetirementDate(retrievalResult, earliestComponent),
            MonthlyAmount = category == Category.Confirmed ? navigator.SelectMonthlyAmount(earliestComponent) : null,
            UnavailableCode = category == Category.Confirmed ? navigator.SelectUnavailableCode(earliestComponent) : null
        };
    }
}
