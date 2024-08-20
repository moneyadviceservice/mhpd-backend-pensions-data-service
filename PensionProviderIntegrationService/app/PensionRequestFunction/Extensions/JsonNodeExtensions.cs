using System.Text.Json.Nodes;

namespace PensionRequestFunction.Extensions;

public static class JsonNodeExtensions
{
    /// <summary>
    /// Renames a property in a JsonObject or in all objects within a JsonArray.
    /// </summary>
    /// <param name="node">The JsonNode to modify.</param>
    /// <param name="oldPropertyName">The name of the property to rename.</param>
    /// <param name="newPropertyName">The new name for the property.</param>
    public static void RenameProperty(ref JsonNode node, string oldPropertyName, string newPropertyName)
    {
        if (node is JsonObject jsonObject)
        {
            // Rename property in JsonObject
            if (jsonObject.TryGetPropertyValue(oldPropertyName, out var value))
            {
                jsonObject.Remove(oldPropertyName);
                jsonObject[newPropertyName] = value;
            }
        }
        else if (node is JsonArray jsonArray)
        {
            // Rename property in each JsonObject within the JsonArray
            foreach (var item in jsonArray)
            {
                if (item is JsonObject itemObject)
                {
                    if (itemObject.TryGetPropertyValue(oldPropertyName, out var value))
                    {
                        itemObject.Remove(oldPropertyName);
                        itemObject[newPropertyName] = value;
                    }
                }
            }
        }
        else
        {
            throw new InvalidOperationException("The node must be a JsonObject or JsonArray containing JsonObjects.");
        }
    }
}
