using PensionsDataService.Models;
using PensionsDataService.Utilities;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace PensionsDataService.Extensions;

public static partial class RetrievedPensionRecordExtensions
{ 
    public static JsonNode EnrichDetailData(this JsonNode retrievalResult, IDetailDataRuleEngine ruleEngine)
    {
        ArgumentNullException.ThrowIfNull(retrievalResult);
        ArgumentNullException.ThrowIfNull(ruleEngine);

        DetailData detailData = ruleEngine.Evaluate(retrievalResult);

        retrievalResult["detailData"] = JsonSerializer.SerializeToNode(detailData, CreateDefaultOptions());

        return retrievalResult;
    }

    private static JsonSerializerOptions CreateDefaultOptions()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        options.Converters.Add(new JsonStringEnumConverter());

        return options;
    }
}
