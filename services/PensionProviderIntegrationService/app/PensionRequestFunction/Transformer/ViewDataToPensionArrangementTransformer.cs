using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using PensionRequestFunction.Enums;
using PensionRequestFunction.Extensions;
using PensionRequestFunction.Constants;

namespace PensionRequestFunction.Transformer
{
    public class ViewDataToPensionArrangementTransformer : IVewDataToPensionArrangementTransformer
    {
        public string Transform(string externalAssetId, string pdpViewData, string pei, string retrievalRecordId)
        {
            if (string.IsNullOrEmpty(externalAssetId) || !Guid.TryParse(externalAssetId, out _))
            {
                throw new Exception("Invalid externalAssetId. It must be a valid GUID.");
            }
            
            if (string.IsNullOrEmpty(pdpViewData))
            {
                throw new Exception("No arrangements present");
            }

            var pdpPensionArrangementsDocument = JsonDocument.Parse(pdpViewData);
            var pdpPensionArrangementsRoot = pdpPensionArrangementsDocument.RootElement;
            var retrievedPensionDetailsPayload = new JsonObject();
            var pensionArrangements = new JsonArray();

            retrievedPensionDetailsPayload.Add(PensionConstants.PensionRetrievalRecordId, retrievalRecordId);
            retrievedPensionDetailsPayload.Add(PensionConstants.Pei, pei);
            retrievedPensionDetailsPayload.Add(PensionConstants.RetrievalResult, pensionArrangements);

            if (!pdpPensionArrangementsRoot.TryGetProperty(PensionConstants.Arrangements, out var pdpArrangementsElement) ||
                pdpArrangementsElement.ValueKind != JsonValueKind.Array ||
                pdpArrangementsElement.ValueKind == JsonValueKind.Null ||
                pdpArrangementsElement.GetArrayLength() == 0)
            {
                throw new JsonException($"The payload either lacks the '{PensionConstants.Arrangements}' property, or the property is not a valid array.");
            }
            
            var arrayEnumerator = pdpArrangementsElement.EnumerateArray();
            foreach (var currentPdpArrangement in arrayEnumerator)
            {
                var currentPdpArrangementJsonElement = currentPdpArrangement;
                var currentPensionArrangement = GetPensionArrangement(externalAssetId, ref currentPdpArrangementJsonElement);
                pensionArrangements.Add(currentPensionArrangement);

                // Alternate scheme names
                AddAlternateSchemeNames(ref currentPdpArrangementJsonElement, ref currentPensionArrangement);

                // BenefitIllustrations
                AddBenefitIllustrations(ref currentPdpArrangementJsonElement, ref currentPensionArrangement);

                // PensionAdministrator
                AddPensionAdministrator(ref currentPdpArrangementJsonElement, ref currentPensionArrangement);

                // AdditionalDataSources
                AddAdditionalDataSources(ref currentPdpArrangementJsonElement, ref currentPensionArrangement);

                // EmploymentMembershipPeriods
                AddEmploymentMembershipPeriods(ref currentPdpArrangementJsonElement, ref currentPensionArrangement);
            }

            return ConvertRetrievedPensionDetailsPayload(retrievedPensionDetailsPayload);;
        }

        private static string ConvertRetrievedPensionDetailsPayload(JsonObject retrievedPensionDetailsPayload)
        {
            var options = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            var bytes = Encoding.ASCII.GetBytes(JsonSerializer.Serialize(retrievedPensionDetailsPayload, options));
            return Encoding.UTF8.GetString(bytes);
        }

        private static string GetMatchType(JsonElement arrangement)
        {
            if (arrangement.TryGetProperty(PensionConstants.PossibleMatch, out var possibleMatch))
            {
                return possibleMatch.GetBoolean() ? PensionEnums.MatchType.POSS.ToString() : PensionEnums.MatchType.DEFN.ToString();
            }

            throw new Exception("MatchType not found");
        }
        
        private static void AddPensionAdministrator(ref JsonElement pdpArrangement, ref JsonObject pensionArrangement)
        {
            var pdpPensionAdministratorJsonNode = JsonNode.Parse(pdpArrangement.GetProperty(PensionConstants.PensionAdministrator).GetRawText())!;
            pensionArrangement.Add(PensionConstants.PensionAdministrator, pdpPensionAdministratorJsonNode);
        }
        
        private static void AddAlternateSchemeNames(ref JsonElement pdpArrangement, ref JsonObject pensionArrangement)
        {
            var alternateSchemeNames = new JsonArray();
            if (pdpArrangement.TryGetProperty(PensionConstants.AlternateSchemeName, out var pdpAlternateSchemeName))
            {
                alternateSchemeNames.Add(pdpAlternateSchemeName);
                pensionArrangement.Add(PensionConstants.AlternateSchemeName, alternateSchemeNames);
            }
        }
        
        private static void AddEmploymentMembershipPeriods(ref JsonElement pdpArrangement, ref JsonObject pensionArrangement)
        {
            var tokenExists = pdpArrangement.TryGetProperty(PensionConstants.EmploymentMembershipPeriods, out var pdpEmploymentMembershipPeriods);
            if (tokenExists && pdpEmploymentMembershipPeriods.ValueKind != JsonValueKind.Undefined)
            {
                var employmentMembershipPeriods = JsonNode.Parse(pdpEmploymentMembershipPeriods.GetRawText())!;
                
                // Transform employment start date to membership start date
                JsonNodeExtensions.RenameProperty(ref employmentMembershipPeriods,
                    PensionConstants.EmploymentStartDate, PensionConstants.MembershipStartDate);
                
                // Transform employment end date to membership end date
                JsonNodeExtensions.RenameProperty(ref employmentMembershipPeriods,
                    PensionConstants.EmploymentStartDate, PensionConstants.MembershipStartDate);
                
                pensionArrangement.Add(PensionConstants.EmploymentMembershipPeriods, employmentMembershipPeriods);
            }
        }
        
        private static void AddBenefitIllustrations(ref JsonElement pdpArrangement, ref JsonObject pensionArrangement)
        {
            var tokenExists = pdpArrangement.TryGetProperty(PensionConstants.BenefitIllustrations, out var pdpBenefitIllustrations);
            if (tokenExists && pdpBenefitIllustrations.ValueKind != JsonValueKind.Undefined)
            {
                var pdpBenefitsIllustrationsJsonNode = JsonNode.Parse(pdpBenefitIllustrations.GetRawText())!;
                pensionArrangement.Add(PensionConstants.BenefitIllustrations, pdpBenefitsIllustrationsJsonNode);
            }
        }

        private static void AddAdditionalDataSources(ref JsonElement pdpArrangement, ref JsonObject pensionArrangement)
        {
            var additionalDataSources = new JsonArray();
            if (pdpArrangement.TryGetProperty(PensionConstants.AdditionalDataSources, out var pdpAdditionalDataSources))
            {
                additionalDataSources.Add(pdpAdditionalDataSources);
                pensionArrangement.Add(PensionConstants.AdditionalDataSources, additionalDataSources);
            }
        }
        
        private static bool TryGetElementValueWithValidation(JsonElement inputElement, string propertyName, 
            out JsonElement outputElement, string[]? validValues = null)
        {
            // Check if the property exists and is not undefined
            if (inputElement.TryGetProperty(propertyName, out outputElement) && outputElement.ValueKind != JsonValueKind.Undefined)
            {
                // Check if the value is in the list of valid values, if provided
                return validValues == null || validValues.Contains(outputElement.ToString());
            }

            outputElement = default; // Set to default if not found
            return false;
        }
        
        private static bool GetContactReference(JsonElement pdpArrangement, JsonObject pensionArrangement, out JsonElement contactReference)
        {
            var matchTypeElement = pensionArrangement.FirstOrDefault(x => x.Key == PensionConstants.MatchType).Value!.ToString();
            return matchTypeElement == PensionEnums.MatchType.POSS.ToString() ? 
                pdpArrangement.TryGetProperty(PensionConstants.PossibleMatchReference, out contactReference) : 
                pdpArrangement.TryGetProperty(PensionConstants.PensionReference, out contactReference);
        }

        private static JsonObject GetPensionArrangement(string externalAssetId, ref JsonElement pdpArrangement)
        {
            if (!TryGetElementValueWithValidation(pdpArrangement, PensionConstants.StatePensionDate, out var retirementDate))
                TryGetElementValueWithValidation(pdpArrangement, PensionConstants.RetirementDate, out retirementDate);
            
            var pensionArrangement = new JsonObject
            {
                { PensionConstants.ExternalAssetId, externalAssetId },
                { PensionConstants.SchemeName, pdpArrangement.GetProperty(PensionConstants.PensionProviderSchemeName).GetString() },
                { PensionConstants.MatchType, GetMatchType(pdpArrangement) },
                { PensionConstants.RetirementDate,  retirementDate.ToString()} // Should this be decided depending on type of pension i.e state pension for statePensionDate
            };

            // Helper method to add properties if they exist
            var element = pdpArrangement;

            void TryAddProperty(string propertyName, string constantName, string[]? validValues = null)
            {
                if (TryGetElementValueWithValidation(element, propertyName, out var value, validValues))
                {
                    pensionArrangement.Add(constantName, value.ToString());
                }
            }
            
            TryAddProperty(PensionConstants.DateOfBirth, PensionConstants.DateOfBirth);
            TryAddProperty(PensionConstants.PensionType, PensionConstants.PensionType);
            TryAddProperty(PensionConstants.PensionOrigin, PensionConstants.PensionOrigin, PensionConstants.ValidPensionOrigins);
            TryAddProperty(PensionConstants.PensionStatus, PensionConstants.PensionStatus, PensionConstants.ValidPensionStatuses); // Ensure this uses the correct valid statuses
            TryAddProperty(PensionConstants.StatePensionMessageEng, PensionConstants.StatePensionMessageEng);
            TryAddProperty(PensionConstants.StatePensionMessageWelsh, PensionConstants.StatePensionMessageWelsh);

            if (GetContactReference(pdpArrangement, pensionArrangement, out var contactReference))
            {
                pensionArrangement.Add(PensionConstants.ContractReference, contactReference.ToString());
            }

            TryAddProperty(PensionConstants.PensionStartDate, PensionConstants.StartDate);
            TryAddProperty(PensionConstants.MembershipStartDate, PensionConstants.MembershipStartDate);

            return pensionArrangement;
        }
    }
}