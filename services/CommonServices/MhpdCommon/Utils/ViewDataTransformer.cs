using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using MhpdCommon.Constants;
using MhpdCommon.Models.MHPDModels;
using MhpdCommon.Extensions;

namespace MhpdCommon.Utils;

public class ViewDataTransformer(IIdValidator idValidator) : IViewDataTransformer
{
    private static readonly JsonSerializerOptions _serializerOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public string Transform(string errorCode, string pei, string retrievalRecordId)
    {
        var errorNode = new JsonObject
        {
            { PensionConstants.ErrorCode, errorCode }
        };
        var retrievedPensionDetailsPayload = new JsonObject
        {
            { PensionConstants.PensionRetrievalRecordId, retrievalRecordId },
            { PensionConstants.Pei, pei },
            { PensionConstants.RetrievalResult, errorNode }
        };

        return ConvertRetrievedPensionDetailsPayload(retrievedPensionDetailsPayload);
    }

    public string Transform(string externalAssetId, string pdpViewData, string pei, string retrievalRecordId)
    {
        if (!idValidator.IsValidGuid(externalAssetId))
        {
            throw new InvalidDataException(PensionConstants.InvalidExternalAssetId);
        }
        
        if (string.IsNullOrEmpty(pdpViewData))
        {
            throw new InvalidDataException("No arrangements present");
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

        return ConvertRetrievedPensionDetailsPayload(retrievedPensionDetailsPayload);
    }

    private static string ConvertRetrievedPensionDetailsPayload(JsonObject retrievedPensionDetailsPayload)
    {
        var bytes = Encoding.ASCII.GetBytes(JsonSerializer.Serialize(retrievedPensionDetailsPayload, _serializerOptions));
        return Encoding.UTF8.GetString(bytes);
    }

    private static string GetMatchType(JsonElement arrangement)
    {
        // Check for temporary system error
        if (arrangement.TryGetProperty(PensionConstants.TemporarySystemError, out var temporarySystemError) &&
            temporarySystemError.GetBoolean())
        {
            return PensionEnums.MatchType.SYS.ToString();
        }

        // Check for possible match
        if (arrangement.TryGetProperty(PensionConstants.PossibleMatch, out var possibleMatch) &&
            possibleMatch.GetBoolean())
        {
            return PensionEnums.MatchType.POSS.ToString();
        }

        // Definite match checks
        if (arrangement.TryGetProperty(PensionConstants.ContactScheme, out var contactScheme) &&
            contactScheme.GetBoolean())
        {
            return PensionEnums.MatchType.CONT.ToString();
        }

        if (arrangement.TryGetProperty(PensionConstants.AdministrativeDetailsNotAvailable, out var adminDetailsNotAvailable) &&
            adminDetailsNotAvailable.GetBoolean())
        {
            return PensionEnums.MatchType.NEW.ToString();
        }

        // Default to definite match
        return PensionEnums.MatchType.DEFN.ToString();
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
            pensionArrangement.Add(PensionConstants.AlternateSchemeNames, alternateSchemeNames);
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
                PensionConstants.EmploymentEndDate, PensionConstants.MembershipEndDate);
            
            pensionArrangement.Add(PensionConstants.EmploymentMembershipPeriods, employmentMembershipPeriods);
        }
    }

    private static void AddBenefitIllustrations(ref JsonElement pdpArrangement, ref JsonObject pensionArrangement)
    {
        if (pdpArrangement.TryGetProperty(PensionConstants.BenefitIllustrations, out var pdpBenefitIllustrations) &&
            pdpBenefitIllustrations.ValueKind != JsonValueKind.Undefined &&
            JsonNode.Parse(pdpBenefitIllustrations.GetRawText()) is JsonArray benefitIllustrationsArray)
        {
            var benefitIllustrations = new JsonArray();

            foreach (var benefit in benefitIllustrationsArray)
            {
                var benefitClone = benefit?.DeepClone();

                var illustrationComponents = new JsonArray();

                foreach (var component in benefit![PensionConstants.IllustrationComponents]?.AsArray()!)
                {
                    var illustration = component?.DeepClone();
                    if(illustration == null) continue;

                    JsonNodeExtensions.RenameProperty(ref illustration, PensionConstants.AccruedPot, PensionConstants.RetirementPot);
                    JsonNodeExtensions.RenameProperty(ref illustration, PensionConstants.EstimatedPot, PensionConstants.RetirementPot);

                    illustrationComponents.Add(illustration);

                    if (illustration?[PensionConstants.PayableDetails] is not JsonObject payableDetails)
                    {
                        continue; // Skip if PayableDetails is not a JsonObject
                    }

                    ProcessPayableDetails(payableDetails);
                }

                benefitClone![PensionConstants.IllustrationComponents] = illustrationComponents;
                benefitIllustrations.Add(benefitClone);
            }

            pensionArrangement.Add(PensionConstants.BenefitIllustrations, benefitIllustrations);
        }
    }

    private static void ProcessPayableDetails(JsonObject payableDetails)
    {
        // Extract and parse the amount type
        var amountTypeString = payableDetails[PensionConstants.AmountType]?.ToString();
        var isValidAmountType = Enum.TryParse(amountTypeString, out PensionEnums.AmountTypes amountType) &&
                                new[] { PensionEnums.AmountTypes.INC, PensionEnums.AmountTypes.INCL, PensionEnums.AmountTypes.INCN }
                                    .Contains(amountType);
        if (!isValidAmountType)
        {
            return; // Skip if amountType is not one of INC, INCL, or INCN
        }

        // Check for the presence of monthlyAmount and the validity of annualAmount
        var hasMonthlyAmount = payableDetails.ContainsKey(PensionConstants.MonthlyAmount);
        var hasValidAnnualAmount = payableDetails.TryGetPropertyValue(PensionConstants.AnnualAmount, out var annualAmountNode);

        if (!hasMonthlyAmount && hasValidAnnualAmount &&
            annualAmountNode?.GetValue<decimal>() is { } annualAmount)
        {
            payableDetails[PensionConstants.MonthlyAmount] = JsonValue.Create(Math.Round(annualAmount / 12));
        }
    }

    private static void AddAdditionalDataSources(ref JsonElement pdpArrangement, ref JsonObject pensionArrangement)
    {
        var additionalDataSources = new JsonArray();
        if (pdpArrangement.TryGetProperty(PensionConstants.AdditionalDataSources, out var pdpAdditionalDataSources) &&
            pdpAdditionalDataSources.ValueKind != JsonValueKind.Undefined &&
            JsonNode.Parse(pdpAdditionalDataSources.GetRawText()) is JsonArray pdpAdditionalDataSourcesArray)
        {
            foreach (var dataSource in pdpAdditionalDataSourcesArray)
            {
                if (dataSource != null)
                {
                    additionalDataSources.Add(dataSource.DeepClone());
                }
            }

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
        var pensionArrangement = new JsonObject();

        // Check if the pension link exists in pdpArrangement
        if (pdpArrangement.TryGetProperty(PensionConstants.PensionLink, out var pensionLink) && pensionLink.ValueKind != JsonValueKind.Undefined)
        {
            // Add the external pension policy ID at the top
            pensionArrangement.Add(PensionConstants.ExternalPensionPolicyId, pensionLink.GetString());
        }

        // Add other properties regardless of the pension link's existence
        pensionArrangement.Add(PensionConstants.ExternalAssetId, externalAssetId);
        pensionArrangement.Add(PensionConstants.SchemeName, pdpArrangement.GetProperty(PensionConstants.PensionProviderSchemeName).GetString());
        pensionArrangement.Add(PensionConstants.MatchType, GetMatchType(pdpArrangement));

        if (TryGetElementValueWithValidation(pdpArrangement, PensionConstants.StatePensionDate, out var retirementDate) ||
            TryGetElementValueWithValidation(pdpArrangement, PensionConstants.RetirementDate, out retirementDate))
        {
            pensionArrangement.Add(PensionConstants.RetirementDate, retirementDate.ToString());
        }

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