using MhpdCommon.Constants;
using MhpdCommon.Models.MHPDModels;
using PensionsDataService.Extensions;
using System.Text.Json;
using System.Text.Json.Nodes;
using static MhpdCommon.ViewData.EvaluationConstants;

namespace PensionsDataServiceUnitTests.Extensions;

public class RetrievedPensionRecordExtensionTests
{
    [Fact]
    public void Enrich_NoLinkedPensions_ReturnsUnchangedRecords()
    {
        // Arrange
        var records = new List<RetrievedPensionRecord>
        {
            CreateRecord("A1", null),
            CreateRecord("A2", null)
        };

        // Act
        var enrichedRecords = records.EnrichLinkedPensions();

        // Assert
        Assert.Equal(2, enrichedRecords.Count);
        Assert.All(enrichedRecords, record =>
        {
            var result = GetRertrievalResult(record);
            Assert.False(result.AsObject().ContainsKey(PensionConstants.LinkedPensions));
        });
    }

    [Fact]
    public void Enrich_LinkedGroup_ConstructsLinkedPensions()
    {
        // Arrange
        var records = new List<RetrievedPensionRecord>
        {
            CreateRecord("A1", "L1"),
            CreateRecord("A2", "L1"),
            CreateRecord("A3", "L1")
        };

        // Act
        var enrichedRecords = records.EnrichLinkedPensions();

        // Assert
        Assert.Equal(3, enrichedRecords.Count);

        foreach (var record in enrichedRecords)
        {
            var result = GetRertrievalResult(record);

            Assert.True(result.AsObject().ContainsKey(PensionConstants.LinkedPensions));

            var linked = result[PensionConstants.LinkedPensions]!.AsArray();
            Assert.Equal(2, linked.Count);
            Assert.DoesNotContain(linked, item => item![PensionConstants.ExternalAssetId]!.GetValue<string>() == record.AssetId);
        }
    }

    [Fact]
    public void Enrich_MultipleGroups_LinkedPensionsStayWithinGroups()
    {
        // Arrange
        var records = new List<RetrievedPensionRecord>
        {
            CreateRecord("A1", "G1"),
            CreateRecord("A2", "G1"),
            CreateRecord("B1", "G2"),
            CreateRecord("B2", "G2"),
            CreateRecord("C1", null) // unrelated
        };

        // Act
        var enrichedRecords = records.EnrichLinkedPensions();

        // Assert group G1
        var group1 = GetRertrievalResult(enrichedRecords[0])[PensionConstants.LinkedPensions]!.AsArray();
        Assert.Single(group1);
        Assert.Equal("A2", group1[0]![PensionConstants.ExternalAssetId]!.GetValue<string>());

        // Assert group G2
        var group2 = GetRertrievalResult(enrichedRecords[2])[PensionConstants.LinkedPensions]!.AsArray();
        Assert.Single(group2);
        Assert.Equal("B2", group2[0]![PensionConstants.ExternalAssetId]!.GetValue<string>());

        // Unlinked record should have no linked pensions
        Assert.False(GetRertrievalResult(enrichedRecords[4]).AsObject().ContainsKey(PensionConstants.LinkedPensions));
    }

    [Fact]
    public void Enrich_IgnoresExcludedCategoriesInLinkedPensions()
    {
        // Arrange
        var records = new List<RetrievedPensionRecord>
        {
            CreateRecord("A1", "L1"),
            CreateRecord("A2", "L1", category: Category.Contact),
            CreateRecord("A3", "L1"),
            CreateRecord("A2", "L1", category: Category.Unsupported),
        };

        // Act
        var enrichedRecords = records.EnrichLinkedPensions();

        // Assert
        foreach (var record in enrichedRecords)
        {
            var rr = GetRertrievalResult(record);

            if (!rr.AsObject().ContainsKey(PensionConstants.LinkedPensions))
                continue;

            var linked = rr[PensionConstants.LinkedPensions]!.AsArray();

            // Excluded category should NOT appear
            Assert.DoesNotContain(linked,
                x => x![PensionConstants.PensionCategory]!.GetValue<string>() == Category.Contact ||
                x![PensionConstants.PensionCategory]!.GetValue<string>() == Category.Unsupported);
        }
    }

    [Fact]
    public void Enrich_ExcludedCategory_DoesNotGetLinkedPensions()
    {
        // Arrange
        var records = new List<RetrievedPensionRecord>
        {
            CreateRecord("A1", "L1", category: Category.Contact),
            CreateRecord("A2", "L1"),
            CreateRecord("A3", "L1")
        };

        // Act
        var enrichedRecords = records.EnrichLinkedPensions();

        // Assert
        var excluded = GetRertrievalResult(enrichedRecords[0]);
        Assert.False(excluded.AsObject().ContainsKey(PensionConstants.LinkedPensions));

        // Other records SHOULD have linked pensions
        var record2 = GetRertrievalResult(enrichedRecords[1]);
        var record3 = GetRertrievalResult(enrichedRecords[2]);

        Assert.True(record2.AsObject().ContainsKey(PensionConstants.LinkedPensions));
        Assert.True(record3.AsObject().ContainsKey(PensionConstants.LinkedPensions));

        // Ensure excluded one (A1) does not appear as a link
        foreach (var rr in new[] { record2, record3 })
        {
            var arr = rr[PensionConstants.LinkedPensions]!.AsArray();
            Assert.DoesNotContain(arr, item =>
                item![PensionConstants.ExternalAssetId]!.GetValue<string>() == "A1");
        }
    }

    [Fact]
    public void Enrich_EmptyList_ReturnsEmpty()
    {
        // Arrange
        var records = new List<RetrievedPensionRecord>();

        // Act
        var result = records.EnrichLinkedPensions();

        // Assert
        Assert.Empty(result);
    }

    private static RetrievedPensionRecord CreateRecord(string assetId, string? linkId,
        string category = "Main", string scheme = "Scheme", string type = "DB", bool hasIncome = false
    )
    {
        var arrangement = new JsonObject
        {
            [PensionConstants.ExternalAssetId] = assetId,
            [PensionConstants.SchemeName] = scheme,
            [PensionConstants.PensionType] = type,
            [PensionConstants.PensionCategory] = category,
            [PensionConstants.HasIncome] = hasIncome
        };

        return new RetrievedPensionRecord
        {
            AssetId = assetId,
            PensionLinkId = linkId,
            Category = category,
            SchemeName = scheme,
            PensionType = type,
            HasIncome = hasIncome,
            RetrievalResult = JsonSerializer.SerializeToElement(arrangement)
        };
    }

    private static JsonNode GetRertrievalResult (RetrievedPensionRecord record)
        => JsonNode.Parse(record.RetrievalResult!.GetRawText());
}
