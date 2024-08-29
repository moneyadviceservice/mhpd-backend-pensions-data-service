using System.Text.Json.Nodes;
using PensionRequestFunction.Extensions;

namespace PensionRequestFunctionUnitTests.Extensions;

public class JsonNodeExtensionsTests
{
    [Fact]
    public void RenameProperty_ShouldRenamePropertyInJsonObject()
    {
        // Arrange
        var json = "{\"employmentStartDate\":\"2024-01-01\"}";
        var node = JsonNode.Parse(json)!;

        // Act
        JsonNodeExtensions.RenameProperty(ref node, "employmentStartDate", "membershipStartDate");

        // Assert
        Assert.True(node is JsonObject);
        var jsonObject = node.AsObject();
        Assert.False(jsonObject.ContainsKey("employmentStartDate"));
        Assert.True(jsonObject.ContainsKey("membershipStartDate"));
        Assert.Equal("2024-01-01", jsonObject["membershipStartDate"]!.ToString());
    }

    [Fact]
    public void RenameProperty_ShouldRenamePropertyInJsonArray()
    {
        // Arrange
        var json = "[{\"employmentStartDate\":\"2024-01-01\"}, {\"employmentStartDate\":\"2024-02-01\"}]";
        var node = JsonNode.Parse(json)!;

        // Act
        JsonNodeExtensions.RenameProperty(ref node,"employmentStartDate", "membershipStartDate");

        // Assert
        Assert.True(node is JsonArray);
        var jsonArray = node.AsArray();

        foreach (var item in jsonArray)
        {
            Assert.True(item is JsonObject);
            var jsonObject = item.AsObject();
            Assert.False(jsonObject.ContainsKey("employmentStartDate"));
            Assert.True(jsonObject.ContainsKey("membershipStartDate"));
        }

        Assert.Equal("2024-01-01", jsonArray[0]!.AsObject()["membershipStartDate"]!.ToString());
        Assert.Equal("2024-02-01", jsonArray[1]!.AsObject()["membershipStartDate"]!.ToString());
    }

    [Fact]
    public void RenameProperty_ShouldDoNothingIfPropertyDoesNotExist()
    {
        // Arrange
        var json = "{\"employmentStartDate\":\"2024-01-01\"}";
        var node = JsonNode.Parse(json)!;

        // Act
        JsonNodeExtensions.RenameProperty(ref node,"nonExistentProperty", "newPropertyName");

        // Assert
        Assert.True(node is JsonObject);
        var jsonObject = node.AsObject();
        Assert.False(jsonObject.ContainsKey("newPropertyName"));
        Assert.True(jsonObject.ContainsKey("employmentStartDate"));
    }

    [Fact]
    public void RenameProperty_ShouldThrowExceptionForNonJsonObjectOrArray()
    {
        // Arrange
        var json = "\"someValue\"";
        var node = JsonNode.Parse(json)!;

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => JsonNodeExtensions.RenameProperty(ref node,"oldPropertyName", "newPropertyName"));
    }

    [Fact]
    public void RenameProperty_ShouldHandleEmptyJsonObject()
    {
        // Arrange
        var json = "{}";
        var node = JsonNode.Parse(json)!;

        // Act
        JsonNodeExtensions.RenameProperty(ref node,"employmentStartDate", "membershipStartDate");

        // Assert
        Assert.True(node is JsonObject);
        var jsonObject = node.AsObject();
        Assert.False(jsonObject.ContainsKey("employmentStartDate"));
        Assert.False(jsonObject.ContainsKey("membershipStartDate"));
    }

    [Fact]
    public void RenameProperty_ShouldHandleEmptyJsonArray()
    {
        // Arrange
        var json = "[]";
        var node = JsonNode.Parse(json)!;

        // Act
        JsonNodeExtensions.RenameProperty(ref node,"employmentStartDate", "membershipStartDate");

        // Assert
        Assert.True(node is JsonArray);
        var jsonArray = node.AsArray();
        Assert.Empty(jsonArray);
    }
}
