using PensionsDataService.Models;
using System.Text.Json.Nodes;

namespace PensionsDataService.Utilities;

public interface IPensionNavigator
{
    JsonNode? SelectIllustrationComponent(JsonNode retrievalResult);

    JsonNode? SelectLatestIllustration(JsonNode retrievalResult);

    JsonNode? SelectEarliestComponent(JsonNode benefitIllustration, string illustrationType);

    DateTime? SelectRetirementDate(JsonNode retrievalResult, JsonNode? component);

    decimal? SelectMonthlyAmount(JsonNode? component);

    decimal? SelectAnnualAmount(JsonNode? component);

    string? SelectAmountNotProvidedReason(JsonNode? component);

    string? SelectUnavailableCode(JsonNode? component);

    PayableDetails GetPayableDetails(JsonNode? component);
}
