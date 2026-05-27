using MhpdCommon.Extensions;
using MhpdCommon.ViewData;
using PensionsDataService.Models;
using System.Text.Json.Nodes;
using static MhpdCommon.ViewData.EvaluationConstants;

namespace PensionsDataService.Utilities;

public sealed class CardDataRuleEngine(IPensionNavigator navigator) : ICardDataRuleEngine
{
    public CardData Evaluate(JsonNode retrievalResult, string category)
    {
        var pensionProperties = navigator.SelectPensionProperties(retrievalResult);

        var earliestComponent = navigator.SelectIllustrationComponent(retrievalResult, PayableDetailsType.Recurring);

        if (pensionProperties.PensionType == PensionEnums.PensionType.CB.GetDisplayValue() && earliestComponent == null)
        {
            earliestComponent = navigator.SelectIllustrationComponent(retrievalResult, PayableDetailsType.LumpSum);
        }

        var payableDetails = navigator.GetPayableDetails(earliestComponent);

        return new CardData
        {
            RetirementDate = navigator.SelectRetirementDate(retrievalResult, earliestComponent),
            MonthlyAmount = category == Category.Confirmed ? payableDetails.MonthlyAmount : null,
            UnavailableCode = category == Category.Confirmed ? payableDetails.UnavailableCode : null,
            LumpSumAmount = category == Category.Confirmed ? payableDetails.LumpSumAmount : null,
            BenefitType = payableDetails.BenefitType
        };
    }
}
