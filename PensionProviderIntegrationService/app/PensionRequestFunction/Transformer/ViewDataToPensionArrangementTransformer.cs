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
            JsonObject retrievedPensionDetailsPayload = new JsonObject();
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
                // benefitIllustrations
                AddBenefitIllustrations(ref currentPDPArrangementJsonElement, ref currentPensionArrangement);
                //pensionAdministrator
                AddPensionAdministrator(ref currentPDPArrangementJsonElement, ref currentPensionArrangement);
                //additionalDataSources
                AddAdditionalDataSources(ref currentPDPArrangementJsonElement, ref currentPensionArrangement);
                //employmentMembershipPeriods
                AddemploymentMembershipPeriods(ref currentPDPArrangementJsonElement, ref currentPensionArrangement);
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
        // pensionAdministrator
        private void AddPensionAdministrator(ref JsonElement pdpArrangement, ref JsonObject pensionArrangement)
        {
            JsonNode pdpPensionAdministratorJsonNode = JsonNode.Parse(pdpArrangement.GetProperty("pensionAdministrator").GetRawText())!;
            pensionArrangement.Add("pensionAdministrator", pdpPensionAdministratorJsonNode);
        }
        //alternateSchemeNames
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
        //employmentMembershipPeriods
        private void AddemploymentMembershipPeriods(ref JsonElement pdpArrangement, ref JsonObject pensionArrangement)
        {
            JsonArray employmentMembershipPeriods = new JsonArray();
            JsonElement pdpEmploymentMembershipPeriods;
            var tokenExists = pdpArrangement.TryGetProperty("employmentMembershipPeriods", out pdpEmploymentMembershipPeriods);
            if (tokenExists == true && !(pdpEmploymentMembershipPeriods.ValueKind == JsonValueKind.Undefined))
            {
                employmentMembershipPeriods.Add(pdpEmploymentMembershipPeriods);
                pensionArrangement.Add("employmentMembershipPeriods", employmentMembershipPeriods);
            }

            return;
        }
        //benefitIllustrations
        private void AddBenefitIllustrations(ref JsonElement pdpArrangement, ref JsonObject pensionArrangement)
        {
            JsonArray benefitIllustrations = new JsonArray();
            JsonElement pdpBenefitIllustrations;
            var tokenExists = pdpArrangement.TryGetProperty("benefitIllustrations", out pdpBenefitIllustrations);
            if (tokenExists == true && !(pdpBenefitIllustrations.ValueKind == JsonValueKind.Undefined))
            {
                benefitIllustrations.Add(pdpBenefitIllustrations);
                pensionArrangement.Add("benefitIllustrations", benefitIllustrations);
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
        private string GetRetirementDateFromStatePensionDate(JsonElement arrangement)
        {
            JsonElement statePensionDate, retirementDate;

            if (arrangement.TryGetProperty("statePensionDate", out statePensionDate)) 
            {
                if (!string.IsNullOrEmpty(statePensionDate.ToString()))
                {
                    return statePensionDate.ToString();
                }
            }
            if (arrangement.TryGetProperty("retirementDate", out retirementDate))
            {
                if (!string.IsNullOrEmpty(retirementDate.ToString()))
                {
                    return retirementDate.ToString();
                }
            }

            return statePensionDate.ToString();
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

        private bool GetPensionOrigin(JsonElement arrangement, out JsonElement pensionOrigin)
        {
            if (arrangement.TryGetProperty("pensionOrigin", out pensionOrigin))
            {
                if (pensionOrigin.ToString() == "A" || pensionOrigin.ToString() == "PC" || pensionOrigin.ToString() == "PM" || pensionOrigin.ToString() == "PT" || pensionOrigin.ToString() == "WC" || pensionOrigin.ToString() == "WM" || pensionOrigin.ToString() == "WT")
                {
                    return true;
                }
            }

            return false;
        }

        private bool GetPensionStatus(JsonElement arrangement, out JsonElement pensionStatus)
        {
            if (arrangement.TryGetProperty("pensionStatus", out pensionStatus))
            {
                if (pensionStatus.ToString() == "A" || pensionStatus.ToString() == "I" || pensionStatus.ToString() == "IPPF" || pensionStatus.ToString() == "IWU")
                {
                    return true;
                }
            }

            return false;
        }

        private bool GetContactReference(JsonElement arrangement, out JsonElement possibleMatchReference)
        {
            var possibleMatchReferenceExists = arrangement.TryGetProperty("possibleMatchReference", out possibleMatchReference);
            if ((possibleMatchReferenceExists == true) && !(possibleMatchReference.ValueKind == JsonValueKind.Undefined))
            {
                return true;
            }
            return false;
        }

        private bool GetStartDate(JsonElement arrangement, out JsonElement pensionStartDate)
        {
            var pensionStartDateExists = arrangement.TryGetProperty("pensionStartDate", out pensionStartDate);
            if ((pensionStartDateExists == true) && !(pensionStartDate.ValueKind == JsonValueKind.Undefined))
            {
                return true;
            }
            return false;
        }

        private bool GetMembershipStartDate(JsonElement arrangement, out JsonElement employmentStartDate)
        {
            var employmentStartDateExists = arrangement.TryGetProperty("employmentStartDate", out employmentStartDate);
            if ((employmentStartDateExists == true) && !(employmentStartDate.ValueKind == JsonValueKind.Undefined))
            {
                return true;
            }
            return false;
        }
        private bool GetEmployerStatus(JsonElement arrangement, out JsonElement employerStatus)
        {
            if (arrangement.TryGetProperty("employerStatus", out employerStatus))
            {
                if (employerStatus.ToString() == "C" || employerStatus.ToString() == "H")
                {
                    return true;
                }
            }
            return false;
        }

        private bool GetDateOfBirth(JsonElement arrangement, out JsonElement dateOfBirth)
        {
            var dateOfBirthExists = arrangement.TryGetProperty("dateOfBirth", out dateOfBirth);

            if ((dateOfBirthExists == true) && !(dateOfBirth.ValueKind == JsonValueKind.Undefined))
            {
                return true;
            }

            return false;
        }
    
        private JsonObject GetPensionArrangement(string externalAssetId, ref JsonElement pdpArrangement)
        {

            JsonObject pensionArrangement = new JsonObject();
            pensionArrangement!.Add("externalAssetId", externalAssetId);
            pensionArrangement!.Add("schemeName", pdpArrangement.GetProperty("pensionProviderSchemeName").GetString());
            pensionArrangement!.Add("matchType", GetMatchType(pdpArrangement));
            pensionArrangement!.Add("retirementDate", GetRetirementDateFromStatePensionDate(pdpArrangement));
           
            if (GetDateOfBirth(pdpArrangement, out JsonElement dateOfBirth))
            {
                pensionArrangement!.Add("dateOfBirth", dateOfBirth.ToString());
            }

            var pensionTypeExists = GetPensionType(pdpArrangement, out JsonElement pensionType);
            if (pensionTypeExists != false && pensionType.ValueKind != JsonValueKind.Undefined)
            {
                pensionArrangement!.Add("pensionType", pensionType.ToString());
            }
           
            var pensionOriginExists = GetPensionOrigin(pdpArrangement, out JsonElement pensionOrigin);
            if (pensionOriginExists != false && pensionOrigin.ValueKind != JsonValueKind.Undefined)
            {
                pensionArrangement!.Add("pensionOrigin", pensionOrigin.ToString());
            }
            
            var pensionStatusExists = GetPensionOrigin(pdpArrangement, out JsonElement pensionStatus);
            if (pensionStatusExists != false && pensionStatus.ValueKind != JsonValueKind.Undefined)
            {
                pensionArrangement!.Add("pensionStatus", pensionStatus.ToString());
            }
            var statePensionMessageEngExists = GetStatePensionMessageEng(pdpArrangement, out JsonElement statePensionMessageEng);
            if (statePensionMessageEngExists != false && statePensionMessageEng.ValueKind != JsonValueKind.Undefined)
            {
                pensionArrangement!.Add("statePensionMessageEng", statePensionMessageEng.ToString());
            }
            var StatePensionMessageWelshExists = GetStatePensionMessageWelsh(pdpArrangement, out JsonElement statePensionMessageWelsh);
            if (StatePensionMessageWelshExists != false && statePensionMessageWelsh.ValueKind != JsonValueKind.Undefined)
            {
                pensionArrangement!.Add("statePensionMessageWelsh", statePensionMessageWelsh.ToString());
            }

            if (GetContactReference(pdpArrangement, out JsonElement possibleMatchReference))
            {
                pensionArrangement!.Add("contactReference", possibleMatchReference.ToString());
            }
           
            if (GetStartDate(pdpArrangement, out JsonElement pensionStartDate))
            {
                pensionArrangement!.Add("startDate", pensionStartDate.ToString());
            }
      
            var employerStatusExists = GetPensionOrigin(pdpArrangement, out JsonElement employerStatus);
            if (employerStatusExists != false && employerStatus.ValueKind != JsonValueKind.Undefined)
            {
                pensionArrangement!.Add("employerStatus", employerStatus.ToString());
            }
           
            if (GetMembershipStartDate(pdpArrangement, out JsonElement employmentStartDate))
            {
                pensionArrangement!.Add("membershipStartDate", employmentStartDate.ToString());
            }
            return pensionArrangement;
        }
    }
}