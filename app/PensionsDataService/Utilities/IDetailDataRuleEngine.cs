using PensionsDataService.Models;
using System.Text.Json.Nodes;

namespace PensionsDataService.Utilities;

public interface IDetailDataRuleEngine
{
    DetailData Evaluate(JsonNode retrievalResult);
}
