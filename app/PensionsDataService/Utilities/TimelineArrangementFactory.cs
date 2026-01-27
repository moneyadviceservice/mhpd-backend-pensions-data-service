using MhpdCommon.Models.MHPDModels;
using PensionsDataService.Models;
using System.Text.Json.Nodes;
using static MhpdCommon.ViewData.EvaluationConstants;

namespace PensionsDataService.Utilities;

public sealed class TimelineArrangementFactory(IPensionNavigator navigator) : ITimelineArrangementFactory
{
    private readonly IPensionNavigator _navigator = navigator;

    public TimelineArrangement? Create(RetrievedPensionRecord pension)
    {
        if (pension.HasIncome == Boolean.FalseString)
        {
            return null;
        }

        var root = JsonNode.Parse(pension.RetrievalResult!.GetRawText());
        if (root == null)
        {
            return null;
        }

        // Recurring payment component
        var recurringComponent = _navigator.SelectIllustrationComponent(root);

        if (recurringComponent == null)
        {
            return null;
        }

        // Lump sum component - if one exists
        var lumpSumComponent = _navigator.SelectEarliestLumpSumComponent(root, IllustrationType.Estimated);

        PayableDetails recurringPayableDetails = _navigator.GetPayableDetails(recurringComponent);
        PayableDetails lumpSumPayableDetails = _navigator.GetPayableDetails(lumpSumComponent);

        if (!recurringPayableDetails.HasAmount)
        {
            return null;
        }

        return new TimelineArrangement
        {
            Id = pension.AssetId,
            SchemeName = pension.SchemeName,
            PensionType = pension.PensionType,
            MonthlyAmount = recurringPayableDetails.MonthlyAmount,
            AnnualAmount = recurringPayableDetails.AnnualAmount,
            LumpSumAmount = lumpSumPayableDetails.LumpSumAmount,
            LumpSumYear = lumpSumPayableDetails.PayableDate?.Year,
            PayableDate = recurringPayableDetails.PayableDate!.Value,
            EndYear = recurringPayableDetails.LastPaymentDate?.Year
        };
    }
}

