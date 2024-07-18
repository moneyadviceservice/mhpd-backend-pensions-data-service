using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace PensionRequestFunction.Transformer
{
    public class ViewDataToPensionArrangementTransformer
    {
        public string Transform(string externalAssetId, string pdpPensionArrangements)
        {
            if (string.IsNullOrEmpty(pdpPensionArrangements))
            {
                throw new Exception("No arrangements present");
            }

            JsonDocument pdpPensionArrangementsDocument = JsonDocument.Parse(pdpPensionArrangements);
            JsonElement pdpPensionArrangementsRoot = pdpPensionArrangementsDocument.RootElement;
            // source root element
            JsonObject retrievedPensionDetailsPayload = new JsonObject();

            // target root element
            JsonArray pensionArrangements = new JsonArray();
            retrievedPensionDetailsPayload.Add("pensionArrangements", pensionArrangements);

            JsonElement pdpArrangementsElement;

            if (!pdpPensionArrangementsRoot.TryGetProperty("arrangements", out pdpArrangementsElement))
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

                // alternate benefits
                AddBenefitIllustrations(ref currentPDPArrangementJsonElement, ref currentPensionArrangement);
                AddPensionAdministrator(ref currentPDPArrangementJsonElement, ref currentPensionArrangement);
                AddAdditionalDataSources(ref currentPDPArrangementJsonElement, ref currentPensionArrangement);
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
                if (possibleMatch.GetBoolean() == true)
                {
                    return "POSS";
                }
                else
                {
                    return "DEFN";
                }
            }

            throw new Exception("MatchType not found");
        }

        private bool GetRetirementDate(JsonElement arrangement, out JsonElement statePensionDate)
        {
            var statePensionExists = arrangement.TryGetProperty("statePensionDate", out statePensionDate);

            if ( (statePensionExists == true) && !(statePensionDate.ValueKind == JsonValueKind.Undefined))
            {
                return true;
            }

            return false;
        }

        private bool GetPensionType(JsonElement arrangement, out JsonElement pensionType)
        {            
            var statePensionTypeExists = arrangement.TryGetProperty("pensionType", out pensionType);

            if ((statePensionTypeExists == true) && !(pensionType.ValueKind == JsonValueKind.Undefined))
            {
                return true;
            }

            return false;
        }

        private bool GetStatePensionMessageEng(JsonElement arrangement, out JsonElement statePensionMessageEng)
        {
            var statePensionMessageEngExists = arrangement.TryGetProperty("statePensionMessageEng", out statePensionMessageEng);

            if ((statePensionMessageEngExists == true) && !(statePensionMessageEng.ValueKind == JsonValueKind.Undefined))
            {
                return true;
            }

            return false;            
        }

        private bool GetStatePensionMessageWelsh(JsonElement arrangement, out JsonElement statePensionMessageWelsh)
        {
            var StatePensionMessageWelshExists = arrangement.TryGetProperty("statePensionMessageWelsh", out statePensionMessageWelsh);

            if ((StatePensionMessageWelshExists == true) && !(statePensionMessageWelsh.ValueKind == JsonValueKind.Undefined))
            {
                return true;
            }

            return false;
        }

        // pensionAdministrator
        private void AddPensionAdministrator(ref JsonElement pdpArrangement, ref JsonObject pensionArrangement)
        {
            JsonNode pdpPensionAdministratorJsonNode = JsonNode.Parse(pdpArrangement.GetProperty("pensionAdministrator").GetRawText())!;
            pensionArrangement.Add("pensionAdministrator", pdpPensionAdministratorJsonNode);
        }

        //pensionArrangement
        private JsonObject GetPensionArrangement(string externalAssetId, ref JsonElement pdpArrangement)
        {

            JsonObject pensionArrangement = new JsonObject();
            pensionArrangement!.Add("matchType", GetMatchType(pdpArrangement));
            pensionArrangement!.Add("schemeName", pdpArrangement.GetProperty("pensionProviderSchemeName").GetString());
            pensionArrangement!.Add("externalAssetId", externalAssetId);

            if (GetRetirementDate(pdpArrangement, out JsonElement statePensionDate))
            {
                pensionArrangement!.Add("retirementDate", statePensionDate.ToString());
            }
            if (GetPensionType(pdpArrangement, out JsonElement pensionType))
            {
                pensionArrangement!.Add("pensionType", pensionType.ToString());
            }
            if (GetStatePensionMessageEng(pdpArrangement, out JsonElement statePensionMessageEng))
            {
                pensionArrangement!.Add("statePensionMessageEng", statePensionMessageEng.ToString());
            }
            if (GetStatePensionMessageWelsh(pdpArrangement, out JsonElement statePensionMessageWelsh))
            {
                pensionArrangement!.Add("statePensionMessageWelsh", statePensionMessageWelsh.ToString());
            }
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

        // additionalDataSources
        private void AddAdditionalDataSources(ref JsonElement pdpArrangement, ref JsonObject pensionArrangement)
        {

            JsonArray additionalDataSources = new JsonArray();

            JsonElement pdpAdditionalDataSources;
            if (pdpArrangement.TryGetProperty("additionalDataSources", out pdpAdditionalDataSources))
            {
                additionalDataSources.Add(pdpAdditionalDataSources);
                pensionArrangement.Add("additionalDataSources", additionalDataSources);
            }

            return;
        }

        // benefitIllustrations
        private void AddBenefitIllustrations(ref JsonElement pdpArrangement, ref JsonObject pensionArrangement)
        {

            JsonArray benefitIllustrations = new JsonArray();
            JsonElement pdpBenefitIllustrations;

            var tokenExists = pdpArrangement.TryGetProperty("benefitIllustrations", out pdpBenefitIllustrations);

            if (tokenExists == true && !(pdpBenefitIllustrations.ValueKind == JsonValueKind.Undefined))
            {
                pensionArrangement.Add("benefitIllustrations", pdpBenefitIllustrations.ToString());

            }

            return;
        }

    }
}