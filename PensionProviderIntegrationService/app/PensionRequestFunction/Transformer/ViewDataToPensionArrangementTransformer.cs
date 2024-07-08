using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace PensionRequestFunction.Transformer
{
    public class ViewDataToPensionArrangementTransformer
    {
        public string Transform(string externalAssetId, string viewDataPayload)
        {

            JsonDocument viewDataPayloadDocument = JsonDocument.Parse(viewDataPayload);
            JsonElement viewDataPayloadRoot = viewDataPayloadDocument.RootElement;

            // source root element
            JsonObject retrievedPensionDetailsPayload = new JsonObject();

            // target root element
            JsonArray pensionArrangements = new JsonArray();
            retrievedPensionDetailsPayload.Add("pensionArrangements", pensionArrangements);

            JsonElement viewDataElement;

            if (!viewDataPayloadRoot.TryGetProperty("view_data", out viewDataElement))
            {
                throw new Exception("No view_data present");
            }

            JsonElement pdpArrangementsElement;

            if (!viewDataElement.TryGetProperty("arrangements", out pdpArrangementsElement))
            {
                throw new Exception("No arrangements present");
            }

            if (pdpArrangementsElement.ValueKind == JsonValueKind.Null)
            {
                throw new Exception("No arrangements present");
            }

            var arrayEnumerator = pdpArrangementsElement.EnumerateArray();

            foreach (var currentPDPArrangement in arrayEnumerator)
            {
                JsonElement currentPDPArrangementJsonElement = currentPDPArrangement;
                JsonObject currentPensionArrangement = GetPensionArrangement(externalAssetId, ref currentPDPArrangementJsonElement);

                pensionArrangements.Add(currentPensionArrangement);

                // alternate scheme names
                AddAlternateSchemeNames(ref currentPDPArrangementJsonElement, ref currentPensionArrangement);

                AddPensionAdministrator(ref currentPDPArrangementJsonElement, ref currentPensionArrangement);

            }

            var result = ConvertRetrievedPensionDetailsPayload(retrievedPensionDetailsPayload);

            return result;
        }

        private string ConvertRetrievedPensionDetailsPayload(JsonObject retrievedPensionDetailsPayload)
        {
            var options = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            byte[] bytes = Encoding.ASCII.GetBytes(JsonSerializer.Serialize(retrievedPensionDetailsPayload, options)!);

            return Encoding.UTF8.GetString(bytes);
        }

        private string GetMatchType(JsonElement arrangement)
        {
            JsonElement possibleMatch;

            if (arrangement.TryGetProperty("possibleMatch", out possibleMatch))
            {
                if (possibleMatch.GetBoolean())
                {
                    return "POSS";
                }
            }

            throw new Exception("MatchType not found");
        }

        // pensionAdministrator
        private void AddPensionAdministrator(ref JsonElement pdpArrangement, ref JsonObject pensionArrangement)
        {
            JsonNode pdpPensionAdministratorJsonNode = JsonNode.Parse(pdpArrangement.GetProperty("pensionAdministrator").GetRawText())!;
            pensionArrangement.Add("pensionAdministrator", pdpPensionAdministratorJsonNode);
        }

        //PensionArrangement
        private JsonObject GetPensionArrangement(string externalAssetId, ref JsonElement pdpArrangement)
        {

            JsonObject pensionArrangement = new JsonObject();
            pensionArrangement!.Add("externalAssetId", externalAssetId);
            pensionArrangement!.Add("schemeName", pdpArrangement.GetProperty("pensionProviderSchemeName").GetString());
            pensionArrangement!.Add("matchType", GetMatchType(pdpArrangement));

            return pensionArrangement;
        }

        // alternateSchemeNames
        private void AddAlternateSchemeNames(ref JsonElement pdpArrangement, ref JsonObject pensionArrangement)
        {

            JsonArray alternateSchemeNames = new JsonArray();
            JsonElement pdpAlternateSchemeName;
            if (pdpArrangement.TryGetProperty("alternateSchemeName", out pdpAlternateSchemeName))
            {
                alternateSchemeNames.Add(pdpAlternateSchemeName);
                pensionArrangement.Add("alternateSchemeNames", alternateSchemeNames);
            }

            return;
        }

    }
}