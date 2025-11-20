using MhpdCommon.Constants;
using MhpdCommon.Models.MHPDModels;
using System.Text.Json;
using System.Text.Json.Nodes;
using static MhpdCommon.ViewData.EvaluationConstants;

namespace PensionsDataService.Extensions;

public static class RetrievedPensionRecordExtensions
{
    private static readonly IEnumerable<string> UnlinkableCategories =
    [
        Category.Contact,
        Category.Unsupported
    ];

    public static List<RetrievedPensionRecord> EnrichLinkedPensions(this List<RetrievedPensionRecord> records)
    {
        if (records == null || records.Count == 0)
        {
            return [];
        }

        // Group by linkId only for records that have one
        var groups = records
            .Where(r => !string.IsNullOrWhiteSpace(r.PensionLinkId))
            .GroupBy(r => r.PensionLinkId!)
            .ToDictionary(g => g.Key, g => g.ToList());

        // Create a cache of link groups excluding unlinkable categories
        var linkGroupsCache = groups.ToDictionary(
            g => g.Key,
            g => g.Value.Where(record => !UnlinkableCategories.Contains(record.Category)).Select(record =>
            {
                return new
                {
                    record.AssetId,
                    Summary = new JsonObject
                    {
                        [PensionConstants.ExternalAssetId] = record.AssetId,
                        [PensionConstants.SchemeName] = record.SchemeName,
                        [PensionConstants.PensionType] = record.PensionType,
                        [PensionConstants.PensionCategory] = record.Category,
                        [PensionConstants.HasIncome] = bool.TryParse(record.HasIncome, out var hasIncome) && hasIncome
                    }
                };
            }).ToList()
        );

        var enrichedList = new List<RetrievedPensionRecord>();

        foreach (var record in records)
        {
            var arrangement = JsonNode.Parse(record.RetrievalResult!.GetRawText());

            // Only add linked pensions if the category is linkable
            if (!UnlinkableCategories.Contains(record.Category)
                && !string.IsNullOrWhiteSpace(record.PensionLinkId)
                && linkGroupsCache.TryGetValue(record.PensionLinkId!, out var cachedGroup))
            {
                var linked = new JsonArray(
                    [.. cachedGroup
                        .Where(x => x.AssetId != record.AssetId)
                        .Select(x => x.Summary.DeepClone())]
                );

                arrangement[PensionConstants.LinkedPensions] = linked;
            }

            enrichedList.Add(new RetrievedPensionRecord
            {
                AssetId = record.AssetId,
                PensionLinkId = record.PensionLinkId,
                Category = record.Category,
                RetrievalResult = JsonSerializer.SerializeToElement(arrangement)
            });
        }

        return enrichedList;
    }
}
