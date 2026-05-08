using MhpdCommon.Models.MHPDModels;
using PensionsDataService.Models;
using System.Text.Json.Nodes;
using static MhpdCommon.ViewData.EvaluationConstants;

namespace PensionsDataService.Utilities;

public sealed class TimelineArrangementFactory(IPensionNavigator navigator) : ITimelineArrangementFactory
{
    private readonly IPensionNavigator _navigator = navigator;

    public IEnumerable<TimelineArrangement> CreateAll(RetrievedPensionRecord pension, bool includeWithoutIncome)
    {
        if (!includeWithoutIncome && !pension.HasIncome)
        {
            yield break;
        }

        var root = JsonNode.Parse(pension.RetrievalResult!.GetRawText());
        if (root == null)
        {
            yield break;
        }

        foreach (var component in CreateRecurringArrangements(pension, root, PayableDetailsType.Recurring))
        {
            yield return component;
        }

        foreach (var component in CreateLumpSumArrangements(pension, root, PayableDetailsType.LumpSum))
        {
            yield return component;
        }
    }

    public IEnumerable<TimelineArrangement> CreateAllLegacy(RetrievedPensionRecord pension, bool includeWithoutIncome)
    {
        if (!includeWithoutIncome && !pension.HasIncome)
        {
            yield break;
        }

        var root = JsonNode.Parse(pension.RetrievalResult!.GetRawText());
        if (root == null)
        {
            yield break;
        }

        foreach (var component in CreateRecurringArrangements(pension, root, PayableDetailsType.Recurring))
        {
            yield return component;
        }

        foreach (var component in CreateRecurringArrangements(pension, root, PayableDetailsType.RecurringLegacy))
        {
            yield return component;
        }

        foreach (var component in CreateLumpSumArrangements(pension, root, PayableDetailsType.LumpSum))
        {
            yield return component;
        }

        foreach (var component in CreateLumpSumArrangements(pension, root, PayableDetailsType.LumpSumLegacy))
        {
            yield return component;
        }
    }

    public IEnumerable<TimelineArrangement> CreateAllAlternative(RetrievedPensionRecord pension, bool includeWithoutIncome)
    {
        if (!includeWithoutIncome && !pension.HasIncome)
        {
            yield break;
        }

        var root = JsonNode.Parse(pension.RetrievalResult!.GetRawText());
        if (root == null)
        {
            yield break;
        }

        foreach (var component in CreateRecurringArrangements(pension, root, PayableDetailsType.Recurring))
        {
            yield return component;
        }

        foreach (var component in CreateRecurringArrangements(pension, root, PayableDetailsType.RecurringNew))
        {
            yield return component;
        }

        foreach (var component in CreateLumpSumArrangements(pension, root, PayableDetailsType.LumpSum))
        {
            yield return component;
        }

        foreach (var component in CreateLumpSumArrangements(pension, root, PayableDetailsType.LumpSumNew))
        {
            yield return component;
        }
    }

    private IEnumerable<TimelineArrangement> CreateRecurringArrangements(RetrievedPensionRecord pension, JsonNode root, string type)
    {
        var illustrations = _navigator.GetIllustrationsByType(root, [type]);

        foreach (var illustration in illustrations)
        {
            var component = _navigator.SelectEarliestIllustrationComponent(illustration);

            PayableDetails details = _navigator.GetPayableDetails(component);

            if (!details.HasAmount)
            {
                continue;
            }

            yield return new TimelineArrangement
            {
                Id = pension.AssetId,
                SchemeName = pension.SchemeName,
                PensionType = pension.PensionType,
                PayableDate = details.PayableDate!.Value,
                MonthlyAmount = details.MonthlyAmount,
                AnnualAmount = details.AnnualAmount,
                EndYear = details.LastPaymentDate?.Year
            };
        }
    }

    private IEnumerable<TimelineArrangement> CreateLumpSumArrangements(RetrievedPensionRecord pension, JsonNode root, string type)
    {
        var components = _navigator.SelectAllComponentsByPayableType(root, type);

        foreach (var component in components)
        {
            PayableDetails details = _navigator.GetPayableDetails(component);

            if (!details.LumpSumAmount.HasValue)
            {
                continue;
            }

            yield return new TimelineArrangement
            {
                Id = pension.AssetId,
                SchemeName = pension.SchemeName,
                PensionType = pension.PensionType,
                PayableDate = details.PayableDate!.Value,
                LumpSumYear = details.PayableDate!.Value.Year,
                LumpSumAmount = details.LumpSumAmount
            };
        }
    }
}

