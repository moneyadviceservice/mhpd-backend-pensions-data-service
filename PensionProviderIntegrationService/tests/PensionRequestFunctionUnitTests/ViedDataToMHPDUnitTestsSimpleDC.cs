using System.Text.Json;
using System.Text.Json.Nodes;
using CommonServices.Models;

namespace PensionRequestFunctionUnitTests
{
    public  class ViedDataToMHPDUnitTestsSimpleDC
    {

        [Fact]
        public void WhenViewDataToMHPDIsCalled_ThenItShouldReturnTrue()
        {
            // Assign
            var externalAssetId = "63ab8af1-2004-4a0b-bad0-629cca220757";
            var externalAssetIdNodeName = "externalAssetId";
            var matchTypeElement = "matchType";
            var matchType = "DEFN";
            var pensionProviderSchemeName = "Your Pension DC Master Trust";
            var possibleMatchReference = "D1006548723";
            var pensionType = "DC";
            var pensionOrigin = "WM";
            var pensionStatus = "WM";
            var pensionStartDate = "1998-05-16";
            var retirementDate = "2038-09-18";
            var dateOfBirth = "1973-09-18";
            var pensionAdministratorName = "Your Pension";
            var pensionAdministratorUrl = "https://www.yourpension.co.uk";
            var pensionAdministratorEmail = "mastertrust@yourpension.com";
            var pensionAdministratorNumber = "+44 80080087355";
            var pensionAdministratorUsage = """
                [
                  "M"
                ]
                """;
            var pensionAdministratorPostalName = "Your Pension";
            var employerName = "Sweets R Us";
            var employerStatus = "C";
            var illustrationDate = "2023-05-16";
            var pensionRequestPayload = GetRequestPayload();
            var viewDataPayload = GetViewDataPayload();
            PensionRequestPayload pensionRequestPayloadDeserialized = JsonSerializer.Deserialize<PensionRequestPayload>(pensionRequestPayload)!;

            // Act
            var transformer = new PensionRequestFunction.Transformer.ViewDataToPensionArrangementTransformer();
            var result = transformer.Transform(externalAssetId, viewDataPayload);
            JsonNode pensionArrangementNode = JsonNode.Parse(result)!;
            var pensionArrangement = pensionArrangementNode["pensionArrangements"]!;
            var pensionEmploymentMembershipPeriods = pensionArrangement[0]!["employmentMembershipPeriods"]!;
            var pensionEenefitIllustrations = pensionArrangement[0]!["benefitIllustrations"]!;
            var pensionArrangementString = pensionArrangement.ToString();
            var schemeName = pensionArrangement[0]!["schemeName"]!.ToString();
            var matchTypeResult = pensionArrangement[0]!["matchType"]!.ToString();
            var PossibleMatchReference = pensionArrangement[0]!["contactReference"]!.ToString();
            var PensionType = pensionArrangement[0]!["pensionType"]!.ToString();
            var PensionOrigin = pensionArrangement[0]!["pensionOrigin"]!.ToString();
            var PensionStatus = pensionArrangement[0]!["pensionStatus"]!.ToString();
            var StartDate = pensionArrangement[0]!["startDate"]!.ToString();
            var RetirementDate = pensionArrangement[0]!["retirementDate"]!.ToString();
            var DateOfBirth = pensionArrangement[0]!["dateOfBirth"]!.ToString();
            var pensionAdministratorResultName = pensionArrangement[0]!["pensionAdministrator"]!["name"]!.ToString();
            var pensionAdministratorUrlValue = pensionArrangement[0]!["pensionAdministrator"]!["contactMethods"]![1]!["contactMethodDetails"]!["url"]!.ToString();
            var pensionAdministratorEmailValue = pensionArrangement[0]!["pensionAdministrator"]!["contactMethods"]![0]!["contactMethodDetails"]!["email"]!.ToString();
            var pensionAdministratorNumberValue = pensionArrangement[0]!["pensionAdministrator"]!["contactMethods"]![2]!["contactMethodDetails"]!["number"]!.ToString();
            var pensionAdministratorUsageValue = pensionArrangement[0]!["pensionAdministrator"]!["contactMethods"]![2]!["contactMethodDetails"]!["usage"]!.ToString();
            var pensionAdministratorPostalNameValue = pensionArrangement[0]!["pensionAdministrator"]!["contactMethods"]![3]!["contactMethodDetails"]!["postalName"]!.ToString();
            var employmentMembershipPeriodsEmployerName = pensionEmploymentMembershipPeriods[0]![0]!["employerName"]!.ToString();
            var employmentMembershipPeriodsEmployerStatus = pensionEmploymentMembershipPeriods[0]![0]!["employerStatus"]!.ToString();
            var benefitIllustrationsIllustrationDate = pensionEenefitIllustrations[0]![0]!["illustrationDate"]!.ToString();
            
            //Assert
            Assert.NotNull(result);
            Assert.Contains(externalAssetIdNodeName, pensionArrangementString);
            Assert.Contains(externalAssetId, pensionRequestPayloadDeserialized.AssetGuid);
            Assert.Contains(matchTypeElement, pensionArrangementString);
            Assert.Equal(pensionProviderSchemeName, schemeName);
            Assert.Equal(matchType, matchTypeResult);
            Assert.Equal(possibleMatchReference, PossibleMatchReference);
            Assert.Equal(pensionType, PensionType);
            Assert.Equal(pensionOrigin, PensionOrigin);
            Assert.Equal(pensionStatus, PensionStatus);
            Assert.Equal(pensionStartDate, StartDate);
            Assert.Equal(retirementDate, RetirementDate);
            Assert.Equal(dateOfBirth, DateOfBirth);
            Assert.Equal(pensionAdministratorUrl, pensionAdministratorUrlValue);
            Assert.Equal(pensionAdministratorEmail, pensionAdministratorEmailValue);
            Assert.Equal(pensionAdministratorNumber, pensionAdministratorNumberValue);
            Assert.Equal(pensionAdministratorName, pensionAdministratorResultName);
            Assert.Equal(pensionAdministratorUsage, pensionAdministratorUsageValue);
            Assert.Equal(pensionAdministratorPostalName, pensionAdministratorPostalNameValue);
            Assert.Equal(employerName, employmentMembershipPeriodsEmployerName);
            Assert.Equal(employerStatus, employmentMembershipPeriodsEmployerStatus);
            Assert.Equal(illustrationDate, benefitIllustrationsIllustrationDate);
        }
        [Fact]
        public void WhenModifiedViewDataPayloadToMHPDIsCalled_ThenItShouldReturnTrue()
        {
            // Assign
            var externalAssetId = "63ab8af1-2004-4a0b-bad0-629cca220757";
            var externalAssetIdNodeName = "externalAssetId";
            var matchTypeElement = "matchType";
            var matchType = "POSS";
            var pensionProviderSchemeName = "ABC";
            var possibleMatchReference = "D9999";
            var pensionType = "SP";
            var pensionOrigin = "PC";
            var pensionStatus = "PC";
            var pensionStartDate = "2024-05-05";
            var retirementDate = "2042-05-05";
            var dateOfBirth = "2000-05-05";
            var pensionAdministratorName = "ABC Your Pension";
            var pensionAdministratorUrl = "https://www.abcyourpension.co.uk";
            var pensionAdministratorEmail = "abcmastertrust@yourpension.com";
            var pensionAdministratorNumber = "+44 9999999999";
            var pensionAdministratorUsage = """
                [
                  "A"
                ]
                """;
            var pensionAdministratorPostalName = "ABCYour Pension";
            var employerName = "ABCSweets R Us";
            var employerStatus = "H";
            var illustrationDate = "2030-05-05";
            var pensionRequestPayload = GetRequestPayload();
            var viewDataPayload = GetModifiedViewDataPayload();
            PensionRequestPayload pensionRequestPayloadDeserialized = JsonSerializer.Deserialize<PensionRequestPayload>(pensionRequestPayload)!;

            // Act
            var transformer = new PensionRequestFunction.Transformer.ViewDataToPensionArrangementTransformer();
            var result = transformer.Transform(externalAssetId, viewDataPayload);

            JsonNode pensionArrangementNode = JsonNode.Parse(result)!;
            var pensionArrangement = pensionArrangementNode["pensionArrangements"]!;
            var pensionEmploymentMembershipPeriods = pensionArrangement[0]!["employmentMembershipPeriods"]!;
            var pensionEenefitIllustrations = pensionArrangement[0]!["benefitIllustrations"]!;
            var pensionArrangementString = pensionArrangement.ToString();
            var schemeName = pensionArrangement[0]!["schemeName"]!.ToString();
            var matchTypeResult = pensionArrangement[0]!["matchType"]!.ToString();
            var PossibleMatchReference = pensionArrangement[0]!["contactReference"]!.ToString();
            var PensionType = pensionArrangement[0]!["pensionType"]!.ToString();
            var PensionOrigin = pensionArrangement[0]!["pensionOrigin"]!.ToString();
            var PensionStatus = pensionArrangement[0]!["pensionStatus"]!.ToString();
            var StartDate = pensionArrangement[0]!["startDate"]!.ToString();
            var RetirementDate = pensionArrangement[0]!["retirementDate"]!.ToString();
            var DateOfBirth = pensionArrangement[0]!["dateOfBirth"]!.ToString();
            var pensionAdministratorResultName = pensionArrangement[0]!["pensionAdministrator"]!["name"]!.ToString();
            var pensionAdministratorUrlValue = pensionArrangement[0]!["pensionAdministrator"]!["contactMethods"]![1]!["contactMethodDetails"]!["url"]!.ToString();
            var pensionAdministratorEmailValue = pensionArrangement[0]!["pensionAdministrator"]!["contactMethods"]![0]!["contactMethodDetails"]!["email"]!.ToString();
            var pensionAdministratorNumberValue = pensionArrangement[0]!["pensionAdministrator"]!["contactMethods"]![2]!["contactMethodDetails"]!["number"]!.ToString();
            var pensionAdministratorUsageValue = pensionArrangement[0]!["pensionAdministrator"]!["contactMethods"]![2]!["contactMethodDetails"]!["usage"]!.ToString();
            var pensionAdministratorPostalNameValue = pensionArrangement[0]!["pensionAdministrator"]!["contactMethods"]![3]!["contactMethodDetails"]!["postalName"]!.ToString();
            var employmentMembershipPeriodsEmployerName = pensionEmploymentMembershipPeriods[0]![0]!["employerName"]!.ToString();
            var employmentMembershipPeriodsEmployerStatus = pensionEmploymentMembershipPeriods[0]![0]!["employerStatus"]!.ToString();
            var benefitIllustrationsIllustrationDate = pensionEenefitIllustrations[0]![0]!["illustrationDate"]!.ToString();
            
            //Assert
            Assert.NotNull(result);
            Assert.Contains(externalAssetIdNodeName, pensionArrangementString);
            Assert.Contains(externalAssetId, pensionRequestPayloadDeserialized.AssetGuid);
            Assert.Contains(matchTypeElement, pensionArrangementString);
            Assert.Equal(pensionProviderSchemeName, schemeName);
            Assert.Equal(matchType, matchTypeResult);
            Assert.Equal(possibleMatchReference, PossibleMatchReference);
            Assert.Equal(pensionType, PensionType);
            Assert.Equal(pensionOrigin, PensionOrigin);
            Assert.Equal(pensionStatus, PensionStatus);
            Assert.Equal(pensionStartDate, StartDate);
            Assert.Equal(retirementDate, RetirementDate);
            Assert.Equal(dateOfBirth, DateOfBirth);
            Assert.Equal(pensionAdministratorUrl, pensionAdministratorUrlValue);
            Assert.Equal(pensionAdministratorEmail, pensionAdministratorEmailValue);
            Assert.Equal(pensionAdministratorNumber, pensionAdministratorNumberValue);
            Assert.Equal(pensionAdministratorName, pensionAdministratorResultName);
            Assert.Equal(pensionAdministratorUsage, pensionAdministratorUsageValue);
            Assert.Equal(pensionAdministratorPostalName, pensionAdministratorPostalNameValue);
            Assert.Equal(employerName, employmentMembershipPeriodsEmployerName);
            Assert.Equal(employerStatus, employmentMembershipPeriodsEmployerStatus);
            Assert.Equal(illustrationDate, benefitIllustrationsIllustrationDate);
        }
        [Fact]
        public void WhenEmptyViewDataPayloadToMHPDIsCalled_ThenItShouldReturnTrue()
        {
            // Assign
            var externalAssetId = "63ab8af1-2004-4a0b-bad0-629cca220757";
            var externalAssetIdNodeName = "externalAssetId";
            var matchTypeElement = "matchType";
            var matchType = "DEFN";
            var pensionProviderSchemeName = string.Empty;
            var possibleMatchReference = string.Empty;
            var pensionType = "DC";
            var pensionOrigin = "WM";
            var pensionStatus = "WM";
            var pensionStartDate = string.Empty;
            var retirementDate = string.Empty;
            var dateOfBirth = string.Empty;
            var pensionAdministratorName = string.Empty;
            var pensionAdministratorUrl = string.Empty;
            var pensionAdministratorEmail = string.Empty;
            var pensionAdministratorNumber = string.Empty;
            var pensionAdministratorUsage = """
                [
                  "M"
                ]
                """;
            var pensionAdministratorPostalName = string.Empty;
            var employerName = string.Empty;
            var employerStatus = "C";
            var illustrationDate = string.Empty;
            var pensionRequestPayload = GetRequestPayload();
            var viewDataPayload = GetEmptyDataViewDataPayload();
            PensionRequestPayload pensionRequestPayloadDeserialized = JsonSerializer.Deserialize<PensionRequestPayload>(pensionRequestPayload)!;

            // Act
            var transformer = new PensionRequestFunction.Transformer.ViewDataToPensionArrangementTransformer();
            var result = transformer.Transform(externalAssetId, viewDataPayload);
            JsonNode pensionArrangementNode = JsonNode.Parse(result)!;
            var pensionArrangement = pensionArrangementNode["pensionArrangements"]!;
            var pensionEmploymentMembershipPeriods = pensionArrangement[0]!["employmentMembershipPeriods"]!;
            var pensionEenefitIllustrations = pensionArrangement[0]!["benefitIllustrations"]!;
            var pensionArrangementString = pensionArrangement.ToString();
            var schemeName = pensionArrangement[0]!["schemeName"]!.ToString();
            var matchTypeResult = pensionArrangement[0]!["matchType"]!.ToString();
            var PossibleMatchReference = pensionArrangement[0]!["contactReference"]!.ToString();
            var PensionType = pensionArrangement[0]!["pensionType"]!.ToString();
            var PensionOrigin = pensionArrangement[0]!["pensionOrigin"]!.ToString();
            var PensionStatus = pensionArrangement[0]!["pensionStatus"]!.ToString();
            var StartDate = pensionArrangement[0]!["startDate"]!.ToString();
            var RetirementDate = pensionArrangement[0]!["retirementDate"]!.ToString();
            var DateOfBirth = pensionArrangement[0]!["dateOfBirth"]!.ToString();
            var pensionAdministratorResultName = pensionArrangement[0]!["pensionAdministrator"]!["name"]!.ToString();
            var pensionAdministratorUrlValue = pensionArrangement[0]!["pensionAdministrator"]!["contactMethods"]![1]!["contactMethodDetails"]!["url"]!.ToString();
            var pensionAdministratorEmailValue = pensionArrangement[0]!["pensionAdministrator"]!["contactMethods"]![0]!["contactMethodDetails"]!["email"]!.ToString();
            var pensionAdministratorNumberValue = pensionArrangement[0]!["pensionAdministrator"]!["contactMethods"]![2]!["contactMethodDetails"]!["number"]!.ToString();
            var pensionAdministratorUsageValue = pensionArrangement[0]!["pensionAdministrator"]!["contactMethods"]![2]!["contactMethodDetails"]!["usage"]!.ToString();
            var pensionAdministratorPostalNameValue = pensionArrangement[0]!["pensionAdministrator"]!["contactMethods"]![3]!["contactMethodDetails"]!["postalName"]!.ToString();
            var employmentMembershipPeriodsEmployerName = pensionEmploymentMembershipPeriods[0]![0]!["employerName"]!.ToString();
            var employmentMembershipPeriodsEmployerStatus = pensionEmploymentMembershipPeriods[0]![0]!["employerStatus"]!.ToString();
            var benefitIllustrationsIllustrationDate = pensionEenefitIllustrations[0]![0]!["illustrationDate"]!.ToString();
            
            //Assert
            Assert.NotNull(result);
            Assert.Contains(externalAssetIdNodeName, pensionArrangementString);
            Assert.Contains(externalAssetId, pensionRequestPayloadDeserialized.AssetGuid);
            Assert.Contains(matchTypeElement, pensionArrangementString);
            Assert.Equal(pensionProviderSchemeName, schemeName);
            Assert.Equal(matchType, matchTypeResult);
            Assert.Equal(possibleMatchReference, PossibleMatchReference);
            Assert.Equal(pensionType, PensionType);
            Assert.Equal(pensionOrigin, PensionOrigin);
            Assert.Equal(pensionStatus, PensionStatus);
            Assert.Equal(pensionStartDate, StartDate);
            Assert.Equal(retirementDate, RetirementDate);
            Assert.Equal(dateOfBirth, DateOfBirth);
            Assert.Equal(pensionAdministratorUrl, pensionAdministratorUrlValue);
            Assert.Equal(pensionAdministratorEmail, pensionAdministratorEmailValue);
            Assert.Equal(pensionAdministratorNumber, pensionAdministratorNumberValue);
            Assert.Equal(pensionAdministratorName, pensionAdministratorResultName);
            Assert.Equal(pensionAdministratorUsage, pensionAdministratorUsageValue);
            Assert.Equal(pensionAdministratorPostalName, pensionAdministratorPostalNameValue);
            Assert.Equal(employerName, employmentMembershipPeriodsEmployerName);
            Assert.Equal(employerStatus, employmentMembershipPeriodsEmployerStatus);
            Assert.Equal(illustrationDate, benefitIllustrationsIllustrationDate);
        }
        [Fact]
        public void WhenEmptyViewDataEmptyRequestPayloadCalled_ThenItShouldReturnTrue()
        {
            // Assign
            var externalAssetId = string.Empty;
            var externalAssetIdNodeName = "externalAssetId";
            var matchTypeElement = "matchType";
            var matchType = "DEFN";
            var pensionProviderSchemeName = string.Empty;
            var possibleMatchReference = string.Empty;
            var pensionType = "DC";
            var pensionOrigin = "WM";
            var pensionStatus = "WM";
            var pensionStartDate = string.Empty;
            var retirementDate = string.Empty;
            var dateOfBirth = string.Empty;
            var pensionAdministratorName = string.Empty;
            var pensionAdministratorUrl = string.Empty;
            var pensionAdministratorEmail = string.Empty;
            var pensionAdministratorNumber = string.Empty;
            var pensionAdministratorUsage = """
                [
                  "M"
                ]
                """;
            var pensionAdministratorPostalName = string.Empty;
            var employerName = string.Empty;
            var employerStatus = "C";
            var illustrationDate = string.Empty;
            var pensionRequestPayload = GetEmptyRequestPayload();
            var viewDataPayload = GetEmptyDataViewDataPayload();
            PensionRequestPayload pensionRequestPayloadDeserialized = JsonSerializer.Deserialize<PensionRequestPayload>(pensionRequestPayload)!;

            // Act
            var transformer = new PensionRequestFunction.Transformer.ViewDataToPensionArrangementTransformer();
            var result = transformer.Transform(externalAssetId, viewDataPayload);
            JsonNode pensionArrangementNode = JsonNode.Parse(result)!;
            var pensionArrangement = pensionArrangementNode["pensionArrangements"]!;
            var pensionEmploymentMembershipPeriods = pensionArrangement[0]!["employmentMembershipPeriods"]!;
            var pensionEenefitIllustrations = pensionArrangement[0]!["benefitIllustrations"]!;
            var pensionArrangementString = pensionArrangement.ToString();
            var schemeName = pensionArrangement[0]!["schemeName"]!.ToString();
            var matchTypeResult = pensionArrangement[0]!["matchType"]!.ToString();
            var PossibleMatchReference = pensionArrangement[0]!["contactReference"]!.ToString();
            var PensionType = pensionArrangement[0]!["pensionType"]!.ToString();
            var PensionOrigin = pensionArrangement[0]!["pensionOrigin"]!.ToString();
            var PensionStatus = pensionArrangement[0]!["pensionStatus"]!.ToString();
            var StartDate = pensionArrangement[0]!["startDate"]!.ToString();
            var RetirementDate = pensionArrangement[0]!["retirementDate"]!.ToString();
            var DateOfBirth = pensionArrangement[0]!["dateOfBirth"]!.ToString();
            var pensionAdministratorResultName = pensionArrangement[0]!["pensionAdministrator"]!["name"]!.ToString();
            var pensionAdministratorUrlValue = pensionArrangement[0]!["pensionAdministrator"]!["contactMethods"]![1]!["contactMethodDetails"]!["url"]!.ToString();
            var pensionAdministratorEmailValue = pensionArrangement[0]!["pensionAdministrator"]!["contactMethods"]![0]!["contactMethodDetails"]!["email"]!.ToString();
            var pensionAdministratorNumberValue = pensionArrangement[0]!["pensionAdministrator"]!["contactMethods"]![2]!["contactMethodDetails"]!["number"]!.ToString();
            var pensionAdministratorUsageValue = pensionArrangement[0]!["pensionAdministrator"]!["contactMethods"]![2]!["contactMethodDetails"]!["usage"]!.ToString();
            var pensionAdministratorPostalNameValue = pensionArrangement[0]!["pensionAdministrator"]!["contactMethods"]![3]!["contactMethodDetails"]!["postalName"]!.ToString();
            var employmentMembershipPeriodsEmployerName = pensionEmploymentMembershipPeriods[0]![0]!["employerName"]!.ToString();
            var employmentMembershipPeriodsEmployerStatus = pensionEmploymentMembershipPeriods[0]![0]!["employerStatus"]!.ToString();
            var benefitIllustrationsIllustrationDate = pensionEenefitIllustrations[0]![0]!["illustrationDate"]!.ToString();
            
            //Assert
            Assert.NotNull(result);
            Assert.Contains(externalAssetIdNodeName, pensionArrangementString);
            Assert.Contains(externalAssetId, pensionRequestPayloadDeserialized.AssetGuid);
            Assert.Contains(matchTypeElement, pensionArrangementString);
            Assert.Equal(pensionProviderSchemeName, schemeName);
            Assert.Equal(matchType, matchTypeResult);
            Assert.Equal(possibleMatchReference, PossibleMatchReference);
            Assert.Equal(pensionType, PensionType);
            Assert.Equal(pensionOrigin, PensionOrigin);
            Assert.Equal(pensionStatus, PensionStatus);
            Assert.Equal(pensionStartDate, StartDate);
            Assert.Equal(retirementDate, RetirementDate);
            Assert.Equal(dateOfBirth, DateOfBirth);
            Assert.Equal(pensionAdministratorUrl, pensionAdministratorUrlValue);
            Assert.Equal(pensionAdministratorEmail, pensionAdministratorEmailValue);
            Assert.Equal(pensionAdministratorNumber, pensionAdministratorNumberValue);
            Assert.Equal(pensionAdministratorName, pensionAdministratorResultName);
            Assert.Equal(pensionAdministratorUsage, pensionAdministratorUsageValue);
            Assert.Equal(pensionAdministratorPostalName, pensionAdministratorPostalNameValue);
            Assert.Equal(employerName, employmentMembershipPeriodsEmployerName);
            Assert.Equal(employerStatus, employmentMembershipPeriodsEmployerStatus);
            Assert.Equal(illustrationDate, benefitIllustrationsIllustrationDate);
        }
        [Fact]
        public void WhenRequestPayloadWithAssetId99a9b3c9_ToMHPDIsCalled_ThenItShouldReturnTrue()
        {
            // Assign
            var matchTypeElement = "matchType";
            var externalAssetId = "99a9b3c9-ac18-43c3-b2e7-723a74eba292";
            var externalAssetIdNodeName = "externalAssetId";
            var matchType = "POSS";
            var pensionRequestPayload = GetRequestPayloadWithAssetID99a9b3c9();
            var viewDataPayload = GetViewDataPayloadPOSS();
            PensionRequestPayload pensionRequestPayloadDeserialized = JsonSerializer.Deserialize<PensionRequestPayload>(pensionRequestPayload)!;
            
            // Act
            var transformer = new PensionRequestFunction.Transformer.ViewDataToPensionArrangementTransformer();
            var result = transformer.Transform(externalAssetId, viewDataPayload);
            JsonNode pensionArrangementNode = JsonNode.Parse(result)!;
            var pensionArrangement = pensionArrangementNode["pensionArrangements"]!;
            var pensionArrangementString = pensionArrangement.ToString();
            var matchTypeResult = pensionArrangement[0]!["matchType"]!.ToString();
            
            //Assert
            Assert.NotNull(result);
            Assert.Contains(externalAssetIdNodeName, pensionArrangementString);
            Assert.Equal(externalAssetId, pensionRequestPayloadDeserialized.AssetGuid);
            Assert.Contains(matchTypeElement, pensionArrangementString);
            Assert.Equal(matchType, matchTypeResult);
        }
        private string GetViewDataPayload()
        {
            return "{\r\n\t\"arrangements\": [{\"pensionProviderSchemeName\":\"Your Pension DC Master Trust\",\"possibleMatchReference\":\"D1006548723\",\"pensionType\":\"DC\",\"pensionOrigin\":\"WM\",\"pensionStatus\":\"A\",\"pensionStartDate\":\"1998-05-16\",\"retirementDate\":\"2038-09-18\",\"dateOfBirth\":\"1973-09-18\",\"possibleMatch\":false,\"pensionAdministrator\":{\"name\":\"Your Pension\",\"contactMethods\":[{\"preferred\":false,\"contactMethodDetails\":{\"email\":\"mastertrust@yourpension.com\"}},{\"preferred\":true,\"contactMethodDetails\":{\"url\":\"https://www.yourpension.co.uk\"}},{\"preferred\":false,\"contactMethodDetails\":{\"number\":\"+44 80080087355\",\"usage\":[\"M\"]}},{\"preferred\":false,\"contactMethodDetails\":{\"postalName\":\"Your Pension\",\"line1\":\"92 Victoria Lane\",\"line2\":\"Frampton Cotterell\",\"line3\":\"Bristol\",\"line4\":\"South Glocustershire\",\"postcode\":\"BS36 9DD\",\"countryCode\":\"GB\"}}]},\"employmentMembershipPeriods\":[{\"employerName\":\"Sweets R Us\",\"employerStatus\":\"C\",\"employmentStartDate\":\"1998-05-16\"}],\"benefitIllustrations\":[{\"illustrationComponents\":[{\"illustrationType\":\"ERI\",\"benefitType\":\"DC\",\"calculationMethod\":\"SMPI\",\"payableDetails\":{\"payableDate\":\"2038-09-18\",\"annualAmount\":20700,\"amountType\":\"INC\"},\"dcPot\":300000,\"survivorBenefit\":false,\"safeguardedBenefit\":false},{\"illustrationType\":\"AP\",\"benefitType\":\"DC\",\"calculationMethod\":\"SMPI\",\"payableDetails\":{\"payableDate\":\"2038-09-18\",\"annualAmount\":16215,\"amountType\":\"INC\"},\"dcPot\":235000,\"survivorBenefit\":false,\"safeguardedBenefit\":false}],\"illustrationDate\":\"2023-05-16\"}]}]\r\n}";
        }
        private string GetRequestPayload()
        {
            return "{\r\n\t\"pensionRetrievalRecordId\": \"e01a9df7-f147-4a3a-a1dd-0507432a5b7f\",\r\n\t\"pei\": \"7075aa11-10ad-4b2f-a9f5-1068e79119bf:1ba03e25-659a-43b8-ae77-b956df168969\",\r\n\t\"iss\": \"DATA_PROVIDER_1fd1da88-9fb3-461c-a48a-3dba21bfba17\",\r\n\t\"userSessionId\": \"459566f6-5fce-479e-a098-298ca9676a85\",\r\n\t\"asset_guid\": \"63ab8af1-2004-4a0b-bad0-629cca220757\"\r\n}";
        }
        private string GetRequestPayloadWithAssetID99a9b3c9()
        {
            return "{\r\n\t\"pensionRetrievalRecordId\": \"e01a9df7-f147-4a3a-a1dd-0507432a5b7f\",\r\n\t\"pei\": \"7075aa11-10ad-4b2f-a9f5-1068e79119bf:1ba03e25-659a-43b8-ae77-b956df168969\",\r\n\t\"iss\": \"DATA_PROVIDER_1fd1da88-9fb3-461c-a48a-3dba21bfba17\",\r\n\t\"userSessionId\": \"99a9b3c9-ac18-43c3-b2e7-723a74eba292\",\r\n\t\"asset_guid\": \"99a9b3c9-ac18-43c3-b2e7-723a74eba292\"\r\n}";
        }
        private string GetViewDataPayloadPOSS()
        {
            return "{\r\n\t\"arrangements\": [{\"pensionProviderSchemeName\":\"Your Pension DC Master Trust\",\"possibleMatchReference\":\"D1006548723\",\"pensionType\":\"DC\",\"pensionOrigin\":\"WM\",\"pensionStatus\":\"A\",\"pensionStartDate\":\"1998-05-16\",\"retirementDate\":\"2038-09-18\",\"dateOfBirth\":\"1973-09-18\",\"possibleMatch\":true,\"pensionAdministrator\":{\"name\":\"Your Pension\",\"contactMethods\":[{\"preferred\":false,\"contactMethodDetails\":{\"email\":\"mastertrust@yourpension.com\"}},{\"preferred\":true,\"contactMethodDetails\":{\"url\":\"https://www.yourpension.co.uk\"}},{\"preferred\":false,\"contactMethodDetails\":{\"number\":\"+44 80080087355\",\"usage\":[\"M\"]}},{\"preferred\":false,\"contactMethodDetails\":{\"postalName\":\"Your Pension\",\"line1\":\"92 Victoria Lane\",\"line2\":\"Frampton Cotterell\",\"line3\":\"Bristol\",\"line4\":\"South Glocustershire\",\"postcode\":\"BS36 9DD\",\"countryCode\":\"GB\"}}]},\"employmentMembershipPeriods\":[{\"employerName\":\"Sweets R Us\",\"employerStatus\":\"C\",\"employmentStartDate\":\"1998-05-16\"}],\"benefitIllustrations\":[{\"illustrationComponents\":[{\"illustrationType\":\"ERI\",\"benefitType\":\"DC\",\"calculationMethod\":\"SMPI\",\"payableDetails\":{\"payableDate\":\"2038-09-18\",\"annualAmount\":20700,\"amountType\":\"INC\"},\"dcPot\":300000,\"survivorBenefit\":false,\"safeguardedBenefit\":false},{\"illustrationType\":\"AP\",\"benefitType\":\"DC\",\"calculationMethod\":\"SMPI\",\"payableDetails\":{\"payableDate\":\"2038-09-18\",\"annualAmount\":16215,\"amountType\":\"INC\"},\"dcPot\":235000,\"survivorBenefit\":false,\"safeguardedBenefit\":false}],\"illustrationDate\":\"2023-05-16\"}]}]\r\n}";
        }
        private string GetModifiedViewDataPayload()
        {
            return "{\r\n\t\"arrangements\": [{\"pensionProviderSchemeName\":\"ABC\",\"possibleMatchReference\":\"D9999\",\"pensionType\":\"SP\",\"pensionOrigin\":\"PC\",\"pensionStatus\":\"PC\",\"pensionStartDate\":\"2024-05-05\",\"retirementDate\":\"2042-05-05\",\"dateOfBirth\":\"2000-05-05\",\"possibleMatch\":true,\"pensionAdministrator\":{\"name\":\"ABC Your Pension\",\"contactMethods\":[{\"preferred\":false,\"contactMethodDetails\":{\"email\":\"abcmastertrust@yourpension.com\"}},{\"preferred\":true,\"contactMethodDetails\":{\"url\":\"https://www.abcyourpension.co.uk\"}},{\"preferred\":false,\"contactMethodDetails\":{\"number\":\"+44 9999999999\",\"usage\":[\"A\"]}},{\"preferred\":false,\"contactMethodDetails\":{\"postalName\":\"ABCYour Pension\",\"line1\":\"92 Victoria Lane\",\"line2\":\"Frampton Cotterell\",\"line3\":\"Bristol\",\"line4\":\"South Glocustershire\",\"postcode\":\"BS36 9DD\",\"countryCode\":\"GB\"}}]},\"employmentMembershipPeriods\":[{\"employerName\":\"ABCSweets R Us\",\"employerStatus\":\"H\",\"employmentStartDate\":\"1998-05-16\"}],\"benefitIllustrations\":[{\"illustrationComponents\":[{\"illustrationType\":\"ERI\",\"benefitType\":\"DC\",\"calculationMethod\":\"SMPI\",\"payableDetails\":{\"payableDate\":\"2038-09-18\",\"annualAmount\":20700,\"amountType\":\"INC\"},\"dcPot\":300000,\"survivorBenefit\":false,\"safeguardedBenefit\":false},{\"illustrationType\":\"AP\",\"benefitType\":\"DC\",\"calculationMethod\":\"SMPI\",\"payableDetails\":{\"payableDate\":\"2038-09-18\",\"annualAmount\":16215,\"amountType\":\"INC\"},\"dcPot\":235000,\"survivorBenefit\":false,\"safeguardedBenefit\":false}],\"illustrationDate\":\"2030-05-05\"}]}]\r\n}";

        }
        private string GetEmptyDataViewDataPayload()
        {
            return "{\r\n\t\"arrangements\": [{\"pensionProviderSchemeName\":\"\",\"possibleMatchReference\":\"\",\"pensionType\":\"DC\",\"pensionOrigin\":\"WM\",\"pensionStatus\":\"A\",\"pensionStartDate\":\"\",\"retirementDate\":\"\",\"dateOfBirth\":\"\",\"possibleMatch\":false,\"pensionAdministrator\":{\"name\":\"\",\"contactMethods\":[{\"preferred\":false,\"contactMethodDetails\":{\"email\":\"\"}},{\"preferred\":true,\"contactMethodDetails\":{\"url\":\"\"}},{\"preferred\":false,\"contactMethodDetails\":{\"number\":\"\",\"usage\":[\"M\"]}},{\"preferred\":false,\"contactMethodDetails\":{\"postalName\":\"\",\"line1\":\"92 Victoria Lane\",\"line2\":\"Frampton Cotterell\",\"line3\":\"Bristol\",\"line4\":\"South Glocustershire\",\"postcode\":\"BS36 9DD\",\"countryCode\":\"GB\"}}]},\"employmentMembershipPeriods\":[{\"employerName\":\"\",\"employerStatus\":\"C\",\"employmentStartDate\":\"1998-05-16\"}],\"benefitIllustrations\":[{\"illustrationComponents\":[{\"illustrationType\":\"ERI\",\"benefitType\":\"DC\",\"calculationMethod\":\"SMPI\",\"payableDetails\":{\"payableDate\":\"2038-09-18\",\"annualAmount\":20700,\"amountType\":\"INC\"},\"dcPot\":300000,\"survivorBenefit\":false,\"safeguardedBenefit\":false},{\"illustrationType\":\"AP\",\"benefitType\":\"DC\",\"calculationMethod\":\"SMPI\",\"payableDetails\":{\"payableDate\":\"2038-09-18\",\"annualAmount\":16215,\"amountType\":\"INC\"},\"dcPot\":235000,\"survivorBenefit\":false,\"safeguardedBenefit\":false}],\"illustrationDate\":\"\"}]}]\r\n}";
        }
        private string GetEmptyRequestPayload()
        {
            return "{\r\n\t\"pensionRetrievalRecordId\": \"\",\r\n\t\"pei\": \"\",\r\n\t\"iss\": \"\",\r\n\t\"userSessionId\": \"\",\r\n\t\"asset_guid\": \"\"\r\n}";
        }
    }
}
