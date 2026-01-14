using PensionsDataService.Models;
using System.Text.Json.Nodes;

namespace PensionsDataService.Utilities;

public interface ICardDataRuleEngine
{
    CardData Evaluate(JsonNode retrievalResult, string category);
}
