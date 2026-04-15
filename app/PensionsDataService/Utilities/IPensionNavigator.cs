using PensionsDataService.Models;
using System.Text.Json.Nodes;

namespace PensionsDataService.Utilities;

public interface IPensionNavigator
{
    JsonNode? SelectIllustrationComponent(JsonNode retrievalResult, string payableDetailsType);

    JsonNode? SelectIllustrationByPayableType(JsonNode retrievalResult, string payableDetailsType);

    JsonNode? SelectEarliestIllustrationComponent(JsonNode? benefitIllustration);

    JsonNode? SelectComponentByType(JsonNode? benefitIllustration, string illustrationType);

    List<JsonNode> SelectAllComponentsByPayableType(JsonNode retrievalResult, string payableDetailsType);

    List<JsonNode?> GetIllustrationsByType(JsonNode? retrievalResult, IEnumerable<string?> payableDetailsTypes);

    DateTime? SelectRetirementDate(JsonNode retrievalResult, JsonNode? component);

    decimal? SelectMonthlyAmount(JsonNode? component);

    decimal? SelectAnnualAmount(JsonNode? component);

    decimal? SelectLumpSumAmount(JsonNode? component);

    string? SelectAmountNotProvidedReason(JsonNode? component);

    string? SelectUnavailableCode(JsonNode? component);

    PensionProperties SelectPensionProperties(JsonNode? retrievalResult);

    PayableDetails GetPayableDetails(JsonNode? component);
}
