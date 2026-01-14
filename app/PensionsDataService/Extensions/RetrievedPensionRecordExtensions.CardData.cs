using MhpdCommon.Constants;
using MhpdCommon.Models.MHPDModels;
using PensionsDataService.Models;
using PensionsDataService.Utilities;
using System.Text.Json;
using System.Text.Json.Nodes;
using static MhpdCommon.ViewData.EvaluationConstants;

namespace PensionsDataService.Extensions;

public static partial class RetrievedPensionRecordExtensions
{
    private static readonly HashSet<string> CarDataCategories =
    [
        Category.Confirmed,
        Category.Pending
    ];

    public static List<RetrievedPensionRecord> EnrichCardData(this List<RetrievedPensionRecord> records, ICardDataRuleEngine ruleEngine)
    {
        if (records == null || records.Count == 0)
            return [];

        var enrichedList = new List<RetrievedPensionRecord>(records.Count);

        foreach (var record in records)
        {
            // Only apply to eligible categories
            if (!CarDataCategories.Contains(record.Category))
            {
                enrichedList.Add(record);
                continue;
            }

            var arrangement = JsonNode.Parse(record.RetrievalResult!.GetRawText())!;

            var cardData = ruleEngine.Evaluate(arrangement, record.Category);

            if (cardData.HasAnyValue)
            {
                arrangement["cardData"] = BuildCardDataNode(cardData);
            }

            enrichedList.Add(new RetrievedPensionRecord
            {
                AssetId = record.AssetId,
                PensionLinkId = record.PensionLinkId,
                Category = record.Category,
                PensionType = record.PensionType,
                RetrievalResult = JsonSerializer.SerializeToElement(arrangement)
            });
        }

        return enrichedList;
    }

    private static JsonObject BuildCardDataNode(CardData data)
    {
        var node = new JsonObject();

        if (data.MonthlyAmount.HasValue)
            node[PensionConstants.MonthlyAmount] = data.MonthlyAmount.Value;

        if (!string.IsNullOrWhiteSpace(data.UnavailableCode))
            node[PensionConstants.UnavailableReason] = data.UnavailableCode;

        if (data.RetirementDate.HasValue)
            node[PensionConstants.RetirementDate] = data.RetirementDate.Value.ToString("yyyy-MM-dd");

        return node;
    }
}
