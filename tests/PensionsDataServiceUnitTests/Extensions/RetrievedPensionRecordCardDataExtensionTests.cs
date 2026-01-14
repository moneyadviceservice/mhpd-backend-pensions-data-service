using MhpdCommon.Models.MHPDModels;
using Moq;
using PensionsDataService.Extensions;
using PensionsDataService.Models;
using PensionsDataService.Utilities;
using System.Text.Json;
using System.Text.Json.Nodes;
using static MhpdCommon.ViewData.EvaluationConstants;

namespace PensionsDataServiceUnitTests.Extensions;

public class RetrievedPensionRecordCardDataExtensionTests
{
    [Fact]
    public void EnrichCardData_ReturnsEmptyList_WhenInputIsEmpty()
    {
        // Arrange
        var records = new List<RetrievedPensionRecord>();
        var engine = new Mock<ICardDataRuleEngine>();

        // Act
        var result = records.EnrichCardData(engine.Object);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        engine.Verify(x => x.Evaluate(It.IsAny<JsonNode>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void EnrichCardData_DoesNotEnrich_IneligibleCategory()
    {
        // Arrange
        var record = new RetrievedPensionRecord
        {
            Category = Category.Contact,
            RetrievalResult = JsonSerializer.SerializeToElement(new { foo = "bar" })
        };

        var records = new List<RetrievedPensionRecord> { record };

        var engine = new Mock<ICardDataRuleEngine>();

        // Act
        var result = records.EnrichCardData(engine.Object);

        // Assert
        var node = JsonNode.Parse(result[0]?.RetrievalResult?.GetRawText())!;
        Assert.Null(node["cardData"]);
        engine.Verify(x => x.Evaluate(It.IsAny<JsonNode>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void EnrichCardData_Enriches_ConfirmedCategory()
    {
        // Arrange
        var record = new RetrievedPensionRecord
        {
            Category = Category.Confirmed,
            RetrievalResult = JsonSerializer.SerializeToElement(new { foo = "bar" })
        };

        var records = new List<RetrievedPensionRecord> { record };

        var engine = new Mock<ICardDataRuleEngine>();
        engine.Setup(x => x.Evaluate(It.IsAny<JsonNode>(), Category.Confirmed))
            .Returns(new CardData
            {
                MonthlyAmount = 2000,
                RetirementDate = new DateTime(2040, 1, 1, 0, 0, 0, DateTimeKind.Unspecified)
            });

        // Act
        var result = records.EnrichCardData(engine.Object);

        // Assert
        var node = JsonNode.Parse(result[0].RetrievalResult?.GetRawText())!;
        Assert.NotNull(node["cardData"]);
        Assert.Equal(2000, node["cardData"]!["monthlyAmount"]!.GetValue<int>());
        Assert.Equal("2040-01-01", node["cardData"]!["retirementDate"]!.GetValue<string>());
    }

    [Fact]
    public void EnrichCardData_Enriches_PendingCategory()
    {
        // Arrange
        var record = new RetrievedPensionRecord
        {
            Category = Category.Pending,
            RetrievalResult = JsonSerializer.SerializeToElement(new { foo = "bar" })
        };

        var records = new List<RetrievedPensionRecord> { record };

        var engine = new Mock<ICardDataRuleEngine>();
        engine.Setup(x => x.Evaluate(It.IsAny<JsonNode>(), Category.Pending))
            .Returns(new CardData
            {
                RetirementDate = new DateTime(2035, 10, 27, 0, 0, 0, DateTimeKind.Unspecified)
            });

        // Act
        var result = records.EnrichCardData(engine.Object);

        // Assert
        var node = JsonNode.Parse(result[0].RetrievalResult?.GetRawText())!;
        Assert.NotNull(node["cardData"]);
        Assert.Equal("2035-10-27", node["cardData"]!["retirementDate"]!.GetValue<string>());
        Assert.Null(node["cardData"]!["monthlyAmount"]);
    }

    [Fact]
    public void EnrichCardData_DoesNotAddCardData_WhenRuleReturnsEmpty()
    {
        // Arrange
        var record = new RetrievedPensionRecord
        {
            Category = Category.Confirmed,
            RetrievalResult = JsonSerializer.SerializeToElement(new { foo = "bar" })
        };

        var records = new List<RetrievedPensionRecord> { record };

        var engine = new Mock<ICardDataRuleEngine>();
        engine.Setup(x => x.Evaluate(It.IsAny<JsonNode>(), Category.Confirmed))
            .Returns(new CardData()); // HasAnyValue = false

        // Act
        var result = records.EnrichCardData(engine.Object);

        // Assert
        var node = JsonNode.Parse(result[0].RetrievalResult?.GetRawText())!;
        Assert.Null(node["cardData"]);
    }

    [Fact]
    public void EnrichCardData_DoesNotMutateOriginalRecord()
    {
        // Arrange
        var original = new RetrievedPensionRecord
        {
            Category = Category.Confirmed,
            RetrievalResult = JsonSerializer.SerializeToElement(new { foo = "bar" })
        };

        var records = new List<RetrievedPensionRecord> { original };

        var engine = new Mock<ICardDataRuleEngine>();
        engine.Setup(x => x.Evaluate(It.IsAny<JsonNode>(), Category.Confirmed))
            .Returns(new CardData
            {
                MonthlyAmount = 123
            });

        // Act
        var result = records.EnrichCardData(engine.Object);

        // Assert
        var originalNode = JsonNode.Parse(original.RetrievalResult.GetRawText())!;
        var enrichedNode = JsonNode.Parse(result[0].RetrievalResult?.GetRawText())!;

        Assert.Null(originalNode["cardData"]);
        Assert.NotNull(enrichedNode["cardData"]);
        Assert.Equal(123, enrichedNode["cardData"]!["monthlyAmount"]!.GetValue<int>());
    }

    [Fact]
    public void EnrichCardData_CallsRuleEngine_OncePerEligibleRecord()
    {
        // Arrange
        var records = new List<RetrievedPensionRecord>
        {
            new() { Category = Category.Confirmed, RetrievalResult = JsonSerializer.SerializeToElement(new {}) },
            new() { Category = Category.Pending, RetrievalResult = JsonSerializer.SerializeToElement(new {}) },
            new() { Category = Category.Contact, RetrievalResult = JsonSerializer.SerializeToElement(new {}) }
        };

        var engine = new Mock<ICardDataRuleEngine>();
        engine.Setup(x => x.Evaluate(It.IsAny<JsonNode>(), It.IsAny<string>()))
            .Returns(new CardData());

        // Act
        records.EnrichCardData(engine.Object);

        // Assert
        engine.Verify(x => x.Evaluate(It.IsAny<JsonNode>(), It.IsAny<string>()), Times.Exactly(2));
    }

    [Fact]
    public void EnrichCardData_PreservesExistingFields()
    {
        // Arrange
        var record = new RetrievedPensionRecord
        {
            AssetId = "asset-1",
            PensionLinkId = "link-1",
            Category = Category.Confirmed,
            RetrievalResult = JsonSerializer.SerializeToElement(new { foo = "bar" })
        };

        var records = new List<RetrievedPensionRecord> { record };

        var engine = new Mock<ICardDataRuleEngine>();
        engine.Setup(x => x.Evaluate(It.IsAny<JsonNode>(), Category.Confirmed))
            .Returns(new CardData
            {
                MonthlyAmount = 999
            });

        // Act
        var result = records.EnrichCardData(engine.Object);

        // Assert
        var enriched = result[0];
        Assert.Equal("asset-1", enriched.AssetId);
        Assert.Equal("link-1", enriched.PensionLinkId);
        Assert.Equal(Category.Confirmed, enriched.Category);
    }
}
