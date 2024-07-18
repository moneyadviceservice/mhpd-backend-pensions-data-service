using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using CommonServices.Models;
using Xunit;

namespace PensionRequestFunctionUnitTests
{
    public class ViewDataToMHPDUnitTest
    {
        [Fact]
        public void WhenTransformerIsCalled_AndNoPdpArrangesmenrtsAreProvided_ThenItThrowsException()
        {
            /// Arrange
            var externalAssetId = "459566f6-5fce-479e-a098-298ca9676a85";
            var pdpPensionArrangements = GetViewDataPayload();
            JsonObject pdpPensionArrangementsJson = JsonSerializer.Deserialize<JsonObject>(pdpPensionArrangements)!;
            pdpPensionArrangementsJson.Remove("arrangements");
            var pdpPensionArrangementsString = JsonSerializer.Serialize<JsonObject>(pdpPensionArrangementsJson)!;

            // Act
            var transformer = new PensionRequestFunction.Transformer.ViewDataToPensionArrangementTransformer();
            var ex = Assert.Throws<Exception>(() => transformer.Transform(externalAssetId, string.Empty)); ;

            // Assert
            Assert.Equal("No arrangements present", ex.Message);
        }
          
        [Fact]
        public void WhenTransformerIsCalled_AndPensionProviderSchemeNameIsProvided_ThenSchemeNameIsPopulated()
        {
            // Arrange
            var pdpPensionArrangements = GetViewDataPayload();
            var externalAssetId = "459566f6-5fce-479e-a098-298ca9676a85";
            var pensionProviderSchemeName = "State Pension";
            JsonObject pdpPensionArrangementsJson = JsonSerializer.Deserialize<JsonObject>(pdpPensionArrangements)!;
            var pdpPensionArrangementsString = JsonSerializer.Serialize<JsonObject>(pdpPensionArrangementsJson)!;

            // Act
            var transformer = new PensionRequestFunction.Transformer.ViewDataToPensionArrangementTransformer();
            var actual = transformer.Transform(externalAssetId, pdpPensionArrangementsString);                     
            var doc = JsonDocument.Parse(actual);
            var root = doc.RootElement;
            var pensionArrangements = root.GetProperty("pensionArrangements");
            var schemeName = pensionArrangements[0].GetProperty("schemeName").ToString();

            // Assert
            Assert.Equal(pensionProviderSchemeName, schemeName);
        }

        [Fact]
        public void WhenViewDataNoArrangments_ThenThrowError()
        {
            // Arrange
            var externalAssetId = "459566f6-5fce-479e-a098-298ca9676a85";
            var pdpPensionArrangements = GetViewDataPayload();
            JsonObject pdpPensionArrangementsJson = JsonSerializer.Deserialize<JsonObject>(pdpPensionArrangements)!;
            pdpPensionArrangementsJson.Remove("arrangements");
            var pdpPensionArrangementsString = JsonSerializer.Serialize<JsonObject>(pdpPensionArrangementsJson)!;

            // Act
            var transformer = new PensionRequestFunction.Transformer.ViewDataToPensionArrangementTransformer();
            var ex = Assert.Throws<Exception>(() => transformer.Transform(externalAssetId, pdpPensionArrangementsString)); ;

            // Assert
            Assert.Equal("No arrangements present", ex.Message);
        }

        [Fact]
        public void WhenViewDataEmptyArrangementsPresent_ThenThrowError1()
        {
            var externalAssetId = "459566f6-5fce-479e-a098-298ca9676a85";
            var pdpPensionArrangements = "{\"view_data\":" + GetEmptyViewDataPayload() + "}";
            JsonObject pdpPensionArrangementsJson = JsonSerializer.Deserialize<JsonObject>(pdpPensionArrangements)!;
            var pdpPensionArrangementsString = JsonSerializer.Serialize<JsonObject>(pdpPensionArrangementsJson)!;
            var transformer = new PensionRequestFunction.Transformer.ViewDataToPensionArrangementTransformer();
            var ex = Assert.Throws<Exception>(() => transformer.Transform(externalAssetId, pdpPensionArrangementsString)); ;
            
            // Assert
            Assert.Equal("No arrangements present", ex.Message);
        }

        private string GetEmptyViewDataPayload()
        {
            return "{\r\n\t\"arrangements\": []\r\n}";
        }      

        [Fact]
        public void WhenViewDataEmptyArrangementsPresent_ThenThrowError()
        {
            var externalAssetId = "7f0763a9-ac18-43c3-b2e7-723a74eba292";
            var viewDataPayload = GetEmptyViewDataPayload();
            JsonObject requestJson = JsonSerializer.Deserialize<JsonObject>(viewDataPayload)!;
            requestJson.Remove("arrangements");
            var newViewDataPayload = JsonSerializer.Serialize<JsonObject>(requestJson)!;
            var transformer = new PensionRequestFunction.Transformer.ViewDataToPensionArrangementTransformer();
            var ex = Assert.Throws<Exception>(() => transformer.Transform(externalAssetId, newViewDataPayload)); ;

            // Assert
            Assert.Equal("No arrangements present", ex.Message);
        }

        [Fact]
        public void WhenViewDataWithOutPossibleMatch_ThenThrowError()
        {
            var externalAssetId = "7f0763a9-ac18-43c3-b2e7-723a74eba292";           
            var viewDataPayload = GetViewDataPayloadWithOutPossibleMatch();
            JsonObject requestJson = JsonSerializer.Deserialize<JsonObject>(viewDataPayload)!;               
            var newViewDataPayload = JsonSerializer.Serialize<JsonObject>(requestJson)!;
            var transformer = new PensionRequestFunction.Transformer.ViewDataToPensionArrangementTransformer();
            var exMatchType = Assert.Throws<Exception>(() => transformer.Transform(externalAssetId, newViewDataPayload));
            var exRetirementDate = Assert.Throws<Exception>(() => transformer.Transform(externalAssetId, newViewDataPayload));
            
            // Assert
            Assert.Equal("MatchType not found", exMatchType.Message);
            
        }

        [Fact]
        public void WhenViewDataWithEmptyStatePensionDateAndPensionType_ThenItShouldReturnEmptyRetirementDate()
        {
            // Assign            
            var externalAssetId = "7f0763a9-ac18-43c3-b2e7-723a74eba292";
            var externalAssetIdNodeName = "externalAssetId";           
            var statePensionDate = string.Empty;
            var pensionType = string.Empty;
            var pensionRequestPayload = GetRequestPayload();           
            var viewDataPayload = GetViewDataPayloadWithEmptyStatePensionDateAndPensionType();
            PensionRequestPayload pensionRequestPayloadDeserialized = JsonSerializer.Deserialize<PensionRequestPayload>(pensionRequestPayload)!;

            // Act
            var transformer = new PensionRequestFunction.Transformer.ViewDataToPensionArrangementTransformer();
            var result = transformer.Transform(externalAssetId, viewDataPayload);

            JsonNode pensionArrangementNode = JsonNode.Parse(result)!;          
            var pensionArrangement = pensionArrangementNode["pensionArrangements"]!;            
            var pensionArrangementString = pensionArrangement.ToString(); 
            var retirementDateResult = pensionArrangement[0]!["retirementDate"]!.ToString();
            var pensionTypeResult = pensionArrangement[0]!["pensionType"]!.ToString();
            
            //Assert
            Assert.NotNull(result);
            Assert.Contains(externalAssetIdNodeName, pensionArrangementString);
            Assert.Contains(externalAssetId, pensionRequestPayloadDeserialized.AssetGuid);           
            Assert.Equal(statePensionDate, retirementDateResult);
            Assert.Equal(pensionType, pensionTypeResult);
        }

        [Fact]
        public void WhenViewDataWithEmptyStatePensionMessage_ThenItShouldReturnEmptyStatePensionMessage()
        {
            // Assign            
            var externalAssetId = "7f0763a9-ac18-43c3-b2e7-723a74eba292";
            var externalAssetIdNodeName = "externalAssetId";
            var statePensionMessageEng = string.Empty;
            var statePensionMessageWelsh = string.Empty;
            var pensionRequestPayload = GetRequestPayload();
            var viewDataPayload = GetViewDataPayloadWithEmptyStatePensionMessage();
            PensionRequestPayload pensionRequestPayloadDeserialized = JsonSerializer.Deserialize<PensionRequestPayload>(pensionRequestPayload)!;

            // Act
            var transformer = new PensionRequestFunction.Transformer.ViewDataToPensionArrangementTransformer();
            var result = transformer.Transform(externalAssetId, viewDataPayload);

            JsonNode pensionArrangementNode = JsonNode.Parse(result)!;            
            var pensionArrangement = pensionArrangementNode["pensionArrangements"]!;
            var pensionArrangementString = pensionArrangement.ToString();
            var statePensionMessageEngResult = pensionArrangement[0]!["statePensionMessageEng"]!.ToString();
            var statePensionMessageWelshResult = pensionArrangement[0]!["statePensionMessageWelsh"]!.ToString();
           
            //Assert
            Assert.NotNull(result);
            Assert.Contains(externalAssetIdNodeName, pensionArrangementString);
            Assert.Contains(externalAssetId, pensionRequestPayloadDeserialized.AssetGuid);
            Assert.Equal(statePensionMessageEng, statePensionMessageEngResult);
            Assert.Equal(statePensionMessageWelsh, statePensionMessageWelshResult);
        }

        [Fact]
        public void WhenViewDataToMHPDIsCalled_ThenItShouldReturnTrue()
        {
            // Assign
            var matchTypeElement = "matchType";
            var externalAssetId = "7f0763a9-ac18-43c3-b2e7-723a74eba292";
            var externalAssetIdNodeName = "externalAssetId";
            var matchType = "DEFN";
            var statePensionDate = "2042-02-23";
            var pensionAdministratorUsage = """
                [
                  "M",
                  "W"
                ]
                """;
            var pensionProviderSchemeName = "State Pension";
            var preferredFalse = "false";
            var preferredTrue = "true";
            var pensionAdministratorUrl = "https://www.gov.uk/future-pension-centre";
            var pensionAdministratorNumber = "+44 8007310175";
            var pensionAdministratorNumber3 = "+44 8007310176";
            var pensionAdministratorName = "DWP";
            var postalName = "Freepost DWP";
            var countryCode = "GB";
            var additionalDataSourcesUrl = "https://www.gov.uk/check-state-pension";
            var informationType = "SP";
            var statePensionMessageEng = "State pension message in English.";
            var statePensionMessageWelsh = "Neges pensiwn gwladol yn Saesneg.";
            var pensionRequestPayload = GetRequestPayload();            
            var viewDataPayload = GetViewDataPayload();
            PensionRequestPayload pensionRequestPayloadDeserialized = JsonSerializer.Deserialize<PensionRequestPayload>(pensionRequestPayload)!;

            // Act
            var transformer = new PensionRequestFunction.Transformer.ViewDataToPensionArrangementTransformer();
            var result = transformer.Transform(externalAssetId, viewDataPayload);

            JsonNode pensionArrangementNode = JsonNode.Parse(result)!;           
            var pensionArrangement = pensionArrangementNode["pensionArrangements"]!;
            var pensionadditionalDataSources = pensionArrangement[0]!["additionalDataSources"]!;
            var pensionArrangementString = pensionArrangement.ToString();
            var schemeName = pensionArrangement[0]!["schemeName"]!.ToString();
            var matchTypeResult = pensionArrangement[0]!["matchType"]!.ToString();
            var retirementDate = pensionArrangement[0]!["retirementDate"]!.ToString();
            var pensionAdministratorPreferredFalse = pensionArrangement[0]!["pensionAdministrator"]!["contactMethods"]![0]!["preferred"]!.ToString();
            var pensionAdministratorPreferredTrue = pensionArrangement[0]!["pensionAdministrator"]!["contactMethods"]![1]!["preferred"]!.ToString();
            var pensionAdministratorPostalName = pensionArrangement[0]!["pensionAdministrator"]!["contactMethods"]![0]!["contactMethodDetails"]!["postalName"]!.ToString();
            var pensionAdministratorCountryCode = pensionArrangement[0]!["pensionAdministrator"]!["contactMethods"]![0]!["contactMethodDetails"]!["countryCode"]!.ToString();
            var pensionAdministratorUrlValue = pensionArrangement[0]!["pensionAdministrator"]!["contactMethods"]![1]!["contactMethodDetails"]!["url"]!.ToString();
            var pensionAdministratorNumberValue = pensionArrangement[0]!["pensionAdministrator"]!["contactMethods"]![2]!["contactMethodDetails"]!["number"]!.ToString();
            var pensionAdministratorNumber3Value = pensionArrangement[0]!["pensionAdministrator"]!["contactMethods"]![3]!["contactMethodDetails"]!["number"]!.ToString();
            var pensionAdministratorUsageValue = pensionArrangement[0]!["pensionAdministrator"]!["contactMethods"]![2]!["contactMethodDetails"]!["usage"]!.ToString();
            var pensionAdministratorResultName = pensionArrangement[0]!["pensionAdministrator"]!["name"]!.ToString();
            var additionalDataSourcesUrlValue = pensionadditionalDataSources[0]![0]!["url"]!.ToString();
            var additionalDataSourcesInformationType = pensionadditionalDataSources[0]![0]!["informationType"]!.ToString();
            var pensionStatePensionMessageEng = pensionArrangement[0]!["statePensionMessageEng"]!.ToString();
            var pensionStatePensionMessageWelsh = pensionArrangement[0]!["statePensionMessageWelsh"]!.ToString();

            //Assert
            Assert.NotNull(result);
            Assert.Contains(externalAssetIdNodeName, pensionArrangementString);
            Assert.Contains(externalAssetId, pensionRequestPayloadDeserialized.AssetGuid);
            Assert.Contains(matchTypeElement, pensionArrangementString);
            Assert.Contains(statePensionDate, retirementDate);
            Assert.Equal(preferredFalse, pensionAdministratorPreferredFalse);
            Assert.Equal(preferredTrue, pensionAdministratorPreferredTrue);
            Assert.Equal(postalName, pensionAdministratorPostalName);
            Assert.Equal(countryCode, pensionAdministratorCountryCode);
            Assert.Equal(pensionProviderSchemeName, schemeName);
            Assert.Equal(matchType, matchTypeResult);
            Assert.Equal(pensionAdministratorName, pensionAdministratorResultName);
            Assert.Equal(pensionAdministratorUrl, pensionAdministratorUrlValue);
            Assert.Equal(pensionAdministratorNumber, pensionAdministratorNumberValue);
            Assert.Equal(pensionAdministratorNumber3, pensionAdministratorNumber3Value);
            Assert.Equal(pensionAdministratorUsage, pensionAdministratorUsageValue);
            Assert.Equal(additionalDataSourcesUrl, additionalDataSourcesUrlValue);
            Assert.Equal(informationType, additionalDataSourcesInformationType);
            Assert.Equal(statePensionMessageEng, pensionStatePensionMessageEng);
            Assert.Equal(statePensionMessageWelsh, pensionStatePensionMessageWelsh);

        }

        [Fact]
        public void WhenViewDataToMHPDIsCalledWithModifiedValues_ThenItShouldReturnTrue()
        {
            // Assign
            var matchTypeElement = "matchType";
            var externalAssetId = "7f0763a9-ac18-43c3-b2e7-723a74eba292";
            var externalAssetIdNodeName = "externalAssetId";
            var matchType = "DEFN";
            var statePensionDate = "2025-05-05";
            var pensionAdministratorUsage = """
                [
                  "A",
                  "B"
                ]
                """;
            var pensionProviderSchemeName = "A1";
            var preferredFalse = "false";
            var preferredTrue = "true";
            var pensionAdministratorUrl = "https://www.gov.uk/future-pension-centre";
            var pensionAdministratorNumber = "+44 8999999999";
            var pensionAdministratorNumber3 = "+44 8999999999";
            var pensionAdministratorName = "A1DWP";
            var postalName = "A1Freepost DWP";
            var countryCode = "GB";
            var additionalDataSourcesUrl = "https://www.gov.uk/check-state-pension";
            var informationType = "SP";
            var statePensionMessageEng = "State pension.";
            var statePensionMessageWelsh = "Neges.";
            var pensionRequestPayload = GetRequestPayload();
            var viewDataPayload = GetViewDataValuePayload();
            PensionRequestPayload pensionRequestPayloadDeserialized = JsonSerializer.Deserialize<PensionRequestPayload>(pensionRequestPayload)!;

            // Act
            var transformer = new PensionRequestFunction.Transformer.ViewDataToPensionArrangementTransformer();
            var result = transformer.Transform(externalAssetId, viewDataPayload);

            JsonNode pensionArrangementNode = JsonNode.Parse(result)!;           
            var pensionArrangement = pensionArrangementNode["pensionArrangements"]!;
            var pensionadditionalDataSources = pensionArrangement[0]!["additionalDataSources"]!;
            var pensionArrangementString = pensionArrangement.ToString();
            var schemeName = pensionArrangement[0]!["schemeName"]!.ToString();
            var matchTypeResult = pensionArrangement[0]!["matchType"]!.ToString();
            var retirementDate = pensionArrangement[0]!["retirementDate"]!.ToString();
            var pensionAdministratorPreferredFalse = pensionArrangement[0]!["pensionAdministrator"]!["contactMethods"]![0]!["preferred"]!.ToString();
            var pensionAdministratorPreferredTrue = pensionArrangement[0]!["pensionAdministrator"]!["contactMethods"]![1]!["preferred"]!.ToString();
            var pensionAdministratorPostalName = pensionArrangement[0]!["pensionAdministrator"]!["contactMethods"]![0]!["contactMethodDetails"]!["postalName"]!.ToString();
            var pensionAdministratorCountryCode = pensionArrangement[0]!["pensionAdministrator"]!["contactMethods"]![0]!["contactMethodDetails"]!["countryCode"]!.ToString();
            var pensionAdministratorUrlValue = pensionArrangement[0]!["pensionAdministrator"]!["contactMethods"]![1]!["contactMethodDetails"]!["url"]!.ToString();
            var pensionAdministratorNumberValue = pensionArrangement[0]!["pensionAdministrator"]!["contactMethods"]![2]!["contactMethodDetails"]!["number"]!.ToString();
            var pensionAdministratorNumber3Value = pensionArrangement[0]!["pensionAdministrator"]!["contactMethods"]![3]!["contactMethodDetails"]!["number"]!.ToString();
            var pensionAdministratorUsageValue = pensionArrangement[0]!["pensionAdministrator"]!["contactMethods"]![2]!["contactMethodDetails"]!["usage"]!.ToString();
            var pensionAdministratorResultName = pensionArrangement[0]!["pensionAdministrator"]!["name"]!.ToString();
            var additionalDataSourcesUrlValue = pensionadditionalDataSources[0]![0]!["url"]!.ToString();
            var additionalDataSourcesInformationType = pensionadditionalDataSources[0]![0]!["informationType"]!.ToString();
            var pensionStatePensionMessageEng = pensionArrangement[0]!["statePensionMessageEng"]!.ToString();
            var pensionStatePensionMessageWelsh = pensionArrangement[0]!["statePensionMessageWelsh"]!.ToString();

            //Assert
            Assert.NotNull(result);
            Assert.Contains(externalAssetIdNodeName, pensionArrangementString);
            Assert.Contains(externalAssetId, pensionRequestPayloadDeserialized.AssetGuid);
            Assert.Contains(matchTypeElement, pensionArrangementString);
            Assert.Contains(statePensionDate, retirementDate);
            Assert.Equal(preferredFalse, pensionAdministratorPreferredFalse);
            Assert.Equal(preferredTrue, pensionAdministratorPreferredTrue);
            Assert.Equal(postalName, pensionAdministratorPostalName);
            Assert.Equal(countryCode, pensionAdministratorCountryCode);
            Assert.Equal(pensionProviderSchemeName, schemeName);
            Assert.Equal(matchType, matchTypeResult);
            Assert.Equal(pensionAdministratorName, pensionAdministratorResultName);
            Assert.Equal(pensionAdministratorUrl, pensionAdministratorUrlValue);
            Assert.Equal(pensionAdministratorNumber, pensionAdministratorNumberValue);
            Assert.Equal(pensionAdministratorNumber3, pensionAdministratorNumber3Value);
            Assert.Equal(pensionAdministratorUsage, pensionAdministratorUsageValue);
            Assert.Equal(additionalDataSourcesUrl, additionalDataSourcesUrlValue);
            Assert.Equal(informationType, additionalDataSourcesInformationType);
            Assert.Equal(statePensionMessageEng, pensionStatePensionMessageEng);
            Assert.Equal(statePensionMessageWelsh, pensionStatePensionMessageWelsh);
        }

        [Fact]
        public void WhenEmptyRequestPayload_ToMHPDIsCalled_ThenItShouldReturnTrue()
        {
            // Assign
            var matchTypeElement = "matchType";
            var externalAssetId = string.Empty;
            var externalAssetIdNodeName = "externalAssetId";
            var matchType = "DEFN";
            var pensionRequestPayload = GetEmptyRequestPayload();
            var viewDataPayload = GetViewDataPayload();
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

        [Fact]
        public void WhenEmptyArrangementsData_ToMHPDIsCalled_ThenItShouldReturnFalse()
        {
            // Assign
            var matchTypeElement = "matchType";
            var externalAssetId = "7f0763a9-ac18-43c3-b2e7-723a74eba292";
            var externalAssetIdNodeName = "externalAssetId";
            var pensionRequestPayload = GetRequestPayload();
            var viewDataPayload = GetEmptyViewDataPayload();
            PensionRequestPayload pensionRequestPayloadDeserialized = JsonSerializer.Deserialize<PensionRequestPayload>(pensionRequestPayload)!;

            // Act
            var transformer = new PensionRequestFunction.Transformer.ViewDataToPensionArrangementTransformer();
            var result = transformer.Transform(externalAssetId, viewDataPayload);
            JsonNode pensionArrangementNode = JsonNode.Parse(result)!;          
            var pensionArrangement = pensionArrangementNode["pensionArrangements"]!;
            var pensionArrangementString = pensionArrangement.ToString();

            //Assert
            Assert.NotNull(result);
            Assert.DoesNotContain(externalAssetIdNodeName, pensionArrangementString);
            Assert.Equal(externalAssetId, pensionRequestPayloadDeserialized.AssetGuid);
            Assert.DoesNotContain(matchTypeElement, pensionArrangementString);
        }

        [Fact]
        public void WhenViewDataWithEmptyValuesEmptyRequestPayload_ThenItShouldReturnTrue()
        {
            // Assign
            var matchTypeElement = "matchType";
            var externalAssetId = string.Empty;
            var externalAssetIdNodeName = "externalAssetId";
            var matchType = "DEFN";
            var statePensionDate = "2025-05-05";
            var pensionAdministratorUsage = """
                [
                  "A",
                  "Z"
                ]
                """;
            var pensionProviderSchemeName = string.Empty;
            var preferredFalse = "false";
            var preferredTrue = "true";
            var pensionAdministratorUrl = string.Empty;
            var pensionAdministratorNumber = string.Empty;         
            var pensionAdministratorName = string.Empty;
            var postalName = string.Empty;
            var informationType = "SP";
            var pensionRequestPayload = GetEmptyRequestPayload();
            var viewDataPayload = GetViewDataWithEmptyValuePayload();
            PensionRequestPayload pensionRequestPayloadDeserialized = JsonSerializer.Deserialize<PensionRequestPayload>(pensionRequestPayload)!;

            // Act
            var transformer = new PensionRequestFunction.Transformer.ViewDataToPensionArrangementTransformer();
            var result = transformer.Transform(externalAssetId, viewDataPayload);

            JsonNode pensionArrangementNode = JsonNode.Parse(result)!;           
            var pensionArrangement = pensionArrangementNode["pensionArrangements"]!;
            var pensionadditionalDataSources = pensionArrangement[0]!["additionalDataSources"]!;
            var pensionArrangementString = pensionArrangement.ToString();
            var schemeName = pensionArrangement[0]!["schemeName"]!.ToString();
            var matchTypeResult = pensionArrangement[0]!["matchType"]!.ToString();
            var retirementDate = pensionArrangement[0]!["retirementDate"]!.ToString();
            var pensionAdministratorPreferredFalse = pensionArrangement[0]!["pensionAdministrator"]!["contactMethods"]![0]!["preferred"]!.ToString();
            var pensionAdministratorPreferredTrue = pensionArrangement[0]!["pensionAdministrator"]!["contactMethods"]![1]!["preferred"]!.ToString();
            var pensionAdministratorPostalName = pensionArrangement[0]!["pensionAdministrator"]!["contactMethods"]![0]!["contactMethodDetails"]!["postalName"]!.ToString();
            var pensionAdministratorUrlValue = pensionArrangement[0]!["pensionAdministrator"]!["contactMethods"]![1]!["contactMethodDetails"]!["url"]!.ToString();
            var pensionAdministratorNumberValue = pensionArrangement[0]!["pensionAdministrator"]!["contactMethods"]![2]!["contactMethodDetails"]!["number"]!.ToString();
            var pensionAdministratorUsageValue = pensionArrangement[0]!["pensionAdministrator"]!["contactMethods"]![2]!["contactMethodDetails"]!["usage"]!.ToString();
            var pensionAdministratorResultName = pensionArrangement[0]!["pensionAdministrator"]!["name"]!.ToString();
            var additionalDataSourcesInformationType = pensionadditionalDataSources[0]![0]!["informationType"]!.ToString();
           
            //Assert
            Assert.NotNull(result);
            Assert.Contains(externalAssetIdNodeName, pensionArrangementString);
            Assert.Contains(externalAssetId, pensionRequestPayloadDeserialized.AssetGuid);            
            Assert.Contains(statePensionDate, retirementDate);
            Assert.Equal(matchType, matchTypeResult);
            Assert.Contains(matchTypeElement, pensionArrangementString);
            Assert.Equal(preferredFalse, pensionAdministratorPreferredFalse);
            Assert.Equal(preferredTrue, pensionAdministratorPreferredTrue);
            Assert.Equal(postalName, pensionAdministratorPostalName);           
            Assert.Equal(pensionProviderSchemeName, schemeName);            
            Assert.Equal(pensionAdministratorName, pensionAdministratorResultName);
            Assert.Equal(pensionAdministratorUrl, pensionAdministratorUrlValue);
            Assert.Equal(pensionAdministratorNumber, pensionAdministratorNumberValue);           
            Assert.Equal(pensionAdministratorUsage, pensionAdministratorUsageValue);           
            Assert.Equal(informationType, additionalDataSourcesInformationType);          

        }
        
        private string GetRequestPayload()
        {           
            return "{\r\n\t\"pensionRetrievalRecordId\": \"e01a9df7-f147-4a3a-a1dd-0507432a5b7f\",\r\n\t\"pei\": \"7075aa11-10ad-4b2f-a9f5-1068e79119bf:1ba03e25-659a-43b8-ae77-b956df168969\",\r\n\t\"iss\": \"DATA_PROVIDER_1fd1da88-9fb3-461c-a48a-3dba21bfba17\",\r\n\t\"userSessionId\": \"7f0763a9-ac18-43c3-b2e7-723a74eba292\",\r\n\t\"asset_guid\": \"7f0763a9-ac18-43c3-b2e7-723a74eba292\"\r\n}";
        }
        private string GetRequestPayloadWithAssetID99a9b3c9()
        {           
            return "{\r\n\t\"pensionRetrievalRecordId\": \"e01a9df7-f147-4a3a-a1dd-0507432a5b7f\",\r\n\t\"pei\": \"7075aa11-10ad-4b2f-a9f5-1068e79119bf:1ba03e25-659a-43b8-ae77-b956df168969\",\r\n\t\"iss\": \"DATA_PROVIDER_1fd1da88-9fb3-461c-a48a-3dba21bfba17\",\r\n\t\"userSessionId\": \"99a9b3c9-ac18-43c3-b2e7-723a74eba292\",\r\n\t\"asset_guid\": \"99a9b3c9-ac18-43c3-b2e7-723a74eba292\"\r\n}";
        }
        private string GetEmptyRequestPayload()
        {
            return "{\r\n\t\"pensionRetrievalRecordId\": \"\",\r\n\t\"pei\": \"\",\r\n\t\"iss\": \"\",\r\n\t\"userSessionId\": \"\",\r\n\t\"asset_guid\": \"\"\r\n}";
        }
        private string GetViewDataPayload()
        {          
            return "{\r\n\t\"arrangements\": [{\"pensionProviderSchemeName\":\"State Pension\",\"pensionType\":\"SP\",\"possibleMatch\":false,\"statePensionDate\":\"2042-02-23\",\"pensionAdministrator\":{\"name\":\"DWP\",\"contactMethods\":[{\"preferred\":false,\"contactMethodDetails\":{\"postalName\":\"Freepost DWP\",\"line1\":\"Pensions Service 3\",\"countryCode\":\"GB\"}},{\"preferred\":true,\"contactMethodDetails\":{\"url\":\"https://www.gov.uk/future-pension-centre\"}},{\"preferred\":false,\"contactMethodDetails\":{\"number\":\"+44 8007310175\",\"usage\":[\"M\",\"W\"]}},{\"preferred\":false,\"contactMethodDetails\":{\"number\":\"+44 8007310176\",\"usage\":[\"M\"]}},{\"preferred\":false,\"contactMethodDetails\":{\"number\":\"+44 8007310456\",\"usage\":[\"W\"]}},{\"preferred\":false,\"contactMethodDetails\":{\"number\":\"+44 1912182051\",\"usage\":[\"N\"]}},{\"preferred\":false,\"contactMethodDetails\":{\"number\":\"+44 1912183600\",\"usage\":[\"N\"]}}]},\"benefitIllustrations\":[{\"illustrationComponents\":[{\"benefitType\":\"SP\",\"illustrationType\":\"ERI\",\"calculationMethod\":\"BS\",\"payableDetails\":{\"payableDate\":\"2042-02-23\",\"annualAmount\":11502,\"monthlyAmount\":958.5,\"amountType\":\"INC\",\"increasing\":true}},{\"benefitType\":\"SP\",\"illustrationType\":\"AP\",\"calculationMethod\":\"BS\",\"payableDetails\":{\"payableDate\":\"2042-02-23\",\"annualAmount\":11502,\"monthlyAmount\":958.5,\"amountType\":\"INC\",\"increasing\":true}}],\"illustrationDate\":\"2024-08-24\"}],\"additionalDataSources\":[{\"url\":\"https://www.gov.uk/check-state-pension\",\"informationType\":\"SP\"}],\"statePensionMessageEng\":\"State pension message in English.\",\"statePensionMessageWelsh\":\"Neges pensiwn gwladol yn Saesneg.\"}]\r\n}";
        }
        private string GetViewDataPayloadPOSS()
        {
            return "{\r\n\t\"arrangements\": [{\"pensionProviderSchemeName\":\"State Pension\",\"pensionType\":\"SP\",\"possibleMatch\":true,\"statePensionDate\":\"2042-02-23\",\"pensionAdministrator\":{\"name\":\"DWP\",\"contactMethods\":[{\"preferred\":false,\"contactMethodDetails\":{\"postalName\":\"Freepost DWP\",\"line1\":\"Pensions Service 3\",\"countryCode\":\"GB\"}},{\"preferred\":true,\"contactMethodDetails\":{\"url\":\"https://www.gov.uk/future-pension-centre\"}},{\"preferred\":false,\"contactMethodDetails\":{\"number\":\"+44 8007310175\",\"usage\":[\"M\",\"W\"]}},{\"preferred\":false,\"contactMethodDetails\":{\"number\":\"+44 8007310176\",\"usage\":[\"M\"]}},{\"preferred\":false,\"contactMethodDetails\":{\"number\":\"+44 8007310456\",\"usage\":[\"W\"]}},{\"preferred\":false,\"contactMethodDetails\":{\"number\":\"+44 1912182051\",\"usage\":[\"N\"]}},{\"preferred\":false,\"contactMethodDetails\":{\"number\":\"+44 1912183600\",\"usage\":[\"N\"]}}]},\"benefitIllustrations\":[{\"illustrationComponents\":[{\"benefitType\":\"SP\",\"illustrationType\":\"ERI\",\"calculationMethod\":\"BS\",\"payableDetails\":{\"payableDate\":\"2042-02-23\",\"annualAmount\":11502,\"monthlyAmount\":958.5,\"amountType\":\"INC\",\"increasing\":true}},{\"benefitType\":\"SP\",\"illustrationType\":\"AP\",\"calculationMethod\":\"BS\",\"payableDetails\":{\"payableDate\":\"2042-02-23\",\"annualAmount\":11502,\"monthlyAmount\":958.5,\"amountType\":\"INC\",\"increasing\":true}}],\"illustrationDate\":\"2024-08-24\"}],\"additionalDataSources\":[{\"url\":\"https://www.gov.uk/check-state-pension\",\"informationType\":\"SP\"}],\"statePensionMessageEng\":\"State pension message in English.\",\"statePensionMessageWelsh\":\"Neges pensiwn gwladol yn Saesneg.\"}]\r\n}";
        }
        private string GetViewDataValuePayload()
        {           
            return "{\r\n\t\"arrangements\": [{\"pensionProviderSchemeName\":\"A1\",\"pensionType\":\"SP\",\"possibleMatch\":false,\"statePensionDate\":\"2025-05-05\",\"pensionAdministrator\":{\"name\":\"A1DWP\",\"contactMethods\":[{\"preferred\":false,\"contactMethodDetails\":{\"postalName\":\"A1Freepost DWP\",\"line1\":\"A1Pensions Service 3\",\"countryCode\":\"GB\"}},{\"preferred\":true,\"contactMethodDetails\":{\"url\":\"https://www.gov.uk/future-pension-centre\"}},{\"preferred\":false,\"contactMethodDetails\":{\"number\":\"+44 8999999999\",\"usage\":[\"A\",\"B\"]}},{\"preferred\":false,\"contactMethodDetails\":{\"number\":\"+44 8999999999\",\"usage\":[\"Z\"]}},{\"preferred\":false,\"contactMethodDetails\":{\"number\":\"+44 8999999999\",\"usage\":[\"V\"]}},{\"preferred\":false,\"contactMethodDetails\":{\"number\":\"+44 8999999999\",\"usage\":[\"X\"]}},{\"preferred\":false,\"contactMethodDetails\":{\"number\":\"+44 1111111111\",\"usage\":[\"N\"]}}]},\"benefitIllustrations\":[{\"illustrationComponents\":[{\"benefitType\":\"SP\",\"illustrationType\":\"ERI\",\"calculationMethod\":\"BS\",\"payableDetails\":{\"payableDate\":\"2042-02-23\",\"annualAmount\":11502,\"monthlyAmount\":958.5,\"amountType\":\"INC\",\"increasing\":true}},{\"benefitType\":\"SP\",\"illustrationType\":\"AP\",\"calculationMethod\":\"BS\",\"payableDetails\":{\"payableDate\":\"2042-02-23\",\"annualAmount\":11502,\"monthlyAmount\":958.5,\"amountType\":\"INC\",\"increasing\":true}}],\"illustrationDate\":\"2024-08-24\"}],\"additionalDataSources\":[{\"url\":\"https://www.gov.uk/check-state-pension\",\"informationType\":\"SP\"}],\"statePensionMessageEng\":\"State pension.\",\"statePensionMessageWelsh\":\"Neges.\"}]\r\n}";
        }

        private string GetViewDataWithEmptyValuePayload()
        {
            return "{\r\n\t\"arrangements\": [{\"pensionProviderSchemeName\":\"\",\"pensionType\":\"AVC\",\"possibleMatch\":false,\"statePensionDate\":\"2025-05-05\",\"pensionAdministrator\":{\"name\":\"\",\"contactMethods\":[{\"preferred\":false,\"contactMethodDetails\":{\"postalName\":\"\",\"line1\":\"A1Pensions Service 3\",\"countryCode\":\"\"}},{\"preferred\":true,\"contactMethodDetails\":{\"url\":\"\"}},{\"preferred\":false,\"contactMethodDetails\":{\"number\":\"\",\"usage\":[\"A\",\"Z\"]}},{\"preferred\":false,\"contactMethodDetails\":{\"number\":\"\",\"usage\":[\"Z\"]}},{\"preferred\":false,\"contactMethodDetails\":{\"number\":\"\",\"usage\":[\"V\"]}},{\"preferred\":false,\"contactMethodDetails\":{\"number\":\"\",\"usage\":[\"X\"]}},{\"preferred\":false,\"contactMethodDetails\":{\"number\":\"\",\"usage\":[\"N\"]}}]},\"benefitIllustrations\":[{\"illustrationComponents\":[{\"illustrationType\":\"ERI\",\"calculationMethod\":\"BS\",\"payableDetails\":{\"payableDate\":\"2042-02-23\",\"annualAmount\":11502,\"monthlyAmount\":958.5,\"amountType\":\"INC\",\"increasing\":true}},{\"illustrationType\":\"AVC\",\"calculationMethod\":\"BS\",\"payableDetails\":{\"payableDate\":\"2042-02-23\",\"annualAmount\":11502,\"monthlyAmount\":958.5,\"amountType\":\"INC\",\"increasing\":true}}],\"illustrationDate\":\"2024-08-24\"}],\"additionalDataSources\":[{\"url\":\"\",\"informationType\":\"SP\"}],\"statePensionMessageEng\":\"\",\"statePensionMessageWelsh\":\"\"}]\r\n}";
        }       
        private string GetViewDataPayloadWithEmptyStatePensionMessage()
        {
            return "{\r\n\t\"arrangements\": [{\"pensionProviderSchemeName\":\"State Pension\",\"possibleMatch\":false,\"statePensionDate\":\"2042-02-23\",\"pensionAdministrator\":{\"name\":\"SP\",\"contactMethods\":[{\"preferred\":false,\"contactMethodDetails\":{\"postalName\":\"Freepost DWP\",\"line1\":\"Pensions Service 3\",\"countryCode\":\"GB\"}},{\"preferred\":true,\"contactMethodDetails\":{\"url\":\"https://www.gov.uk/future-pension-centre\"}},{\"preferred\":false,\"contactMethodDetails\":{\"number\":\"+44 8007310175\",\"usage\":[\"M\",\"W\"]}},{\"preferred\":false,\"contactMethodDetails\":{\"number\":\"+44 8007310176\",\"usage\":[\"M\"]}},{\"preferred\":false,\"contactMethodDetails\":{\"number\":\"+44 8007310456\",\"usage\":[\"W\"]}},{\"preferred\":false,\"contactMethodDetails\":{\"number\":\"+44 1912182051\",\"usage\":[\"N\"]}},{\"preferred\":false,\"contactMethodDetails\":{\"number\":\"+44 1912183600\",\"usage\":[\"N\"]}}]},\"benefitIllustrations\":[{\"illustrationComponents\":[{\"illustrationType\":\"ERI\",\"calculationMethod\":\"BS\",\"payableDetails\":{\"payableDate\":\"2042-02-23\",\"annualAmount\":11502,\"monthlyAmount\":958.5,\"amountType\":\"INC\",\"increasing\":true}},{\"illustrationType\":\"AP\",\"calculationMethod\":\"BS\",\"payableDetails\":{\"payableDate\":\"2042-02-23\",\"annualAmount\":11502,\"monthlyAmount\":958.5,\"amountType\":\"INC\",\"increasing\":true}}],\"illustrationDate\":\"2024-08-24\"}],\"additionalDataSources\":[{\"url\":\"https://www.gov.uk/check-state-pension\",\"informationType\":\"SP\"}],\"statePensionMessageEng\":\"\",\"statePensionMessageWelsh\":\"\"}]\r\n}";
        }

        private string GetViewDataPayloadWithOutPossibleMatch()
        {            
            return "{\r\n\t\"arrangements\": [{\"pensionProviderSchemeName\":\"State Pension\",\"statePensionDate\":\"2042-02-23\",\"pensionAdministrator\":{\"name\":\"DWP\",\"contactMethods\":[{\"preferred\":false,\"contactMethodDetails\":{\"postalName\":\"Freepost DWP\",\"line1\":\"Pensions Service 3\",\"countryCode\":\"GB\"}},{\"preferred\":true,\"contactMethodDetails\":{\"url\":\"https://www.gov.uk/future-pension-centre\"}},{\"preferred\":false,\"contactMethodDetails\":{\"number\":\"+44 8007310175\",\"usage\":[\"M\",\"W\"]}},{\"preferred\":false,\"contactMethodDetails\":{\"number\":\"+44 8007310176\",\"usage\":[\"M\"]}},{\"preferred\":false,\"contactMethodDetails\":{\"number\":\"+44 8007310456\",\"usage\":[\"W\"]}},{\"preferred\":false,\"contactMethodDetails\":{\"number\":\"+44 1912182051\",\"usage\":[\"N\"]}},{\"preferred\":false,\"contactMethodDetails\":{\"number\":\"+44 1912183600\",\"usage\":[\"N\"]}}]},\"benefitIllustrations\":[{\"illustrationComponents\":[{\"illustrationType\":\"ERI\",\"calculationMethod\":\"BS\",\"payableDetails\":{\"payableDate\":\"2042-02-23\",\"annualAmount\":11502,\"monthlyAmount\":958.5,\"amountType\":\"INC\",\"increasing\":true}},{\"illustrationType\":\"AP\",\"calculationMethod\":\"BS\",\"payableDetails\":{\"payableDate\":\"2042-02-23\",\"annualAmount\":11502,\"monthlyAmount\":958.5,\"amountType\":\"INC\",\"increasing\":true}}],\"illustrationDate\":\"2024-08-24\"}],\"additionalDataSources\":[{\"url\":\"https://www.gov.uk/check-state-pension\",\"informationType\":\"SP\"}],\"statePensionMessageEng\":\"State pension message in English.\",\"statePensionMessageWelsh\":\"Neges pensiwn gwladol yn Saesneg.\"}]\r\n}";
        }       
        private string GetViewDataPayloadWithEmptyStatePensionDateAndPensionType()
        {
            return "{\r\n\t\"arrangements\": [{\"pensionProviderSchemeName\":\"State Pension\",\"pensionType\":\"\",\"possibleMatch\":false,\"statePensionDate\":\"\",\"pensionAdministrator\":{\"name\":\"DWP\",\"contactMethods\":[{\"preferred\":false,\"contactMethodDetails\":{\"postalName\":\"Freepost DWP\",\"line1\":\"Pensions Service 3\",\"countryCode\":\"GB\"}},{\"preferred\":true,\"contactMethodDetails\":{\"url\":\"https://www.gov.uk/future-pension-centre\"}},{\"preferred\":false,\"contactMethodDetails\":{\"number\":\"+44 8007310175\",\"usage\":[\"M\",\"W\"]}},{\"preferred\":false,\"contactMethodDetails\":{\"number\":\"+44 8007310176\",\"usage\":[\"M\"]}},{\"preferred\":false,\"contactMethodDetails\":{\"number\":\"+44 8007310456\",\"usage\":[\"W\"]}},{\"preferred\":false,\"contactMethodDetails\":{\"number\":\"+44 1912182051\",\"usage\":[\"N\"]}},{\"preferred\":false,\"contactMethodDetails\":{\"number\":\"+44 1912183600\",\"usage\":[\"N\"]}}]},\"benefitIllustrations\":[{\"illustrationComponents\":[{\"illustrationType\":\"ERI\",\"calculationMethod\":\"BS\",\"payableDetails\":{\"payableDate\":\"2042-02-23\",\"annualAmount\":11502,\"monthlyAmount\":958.5,\"amountType\":\"INC\",\"increasing\":true}},{\"illustrationType\":\"AP\",\"calculationMethod\":\"BS\",\"payableDetails\":{\"payableDate\":\"2042-02-23\",\"annualAmount\":11502,\"monthlyAmount\":958.5,\"amountType\":\"INC\",\"increasing\":true}}],\"illustrationDate\":\"2024-08-24\"}],\"additionalDataSources\":[{\"url\":\"https://www.gov.uk/check-state-pension\",\"informationType\":\"SP\"}],\"statePensionMessageEng\":\"State pension message in English.\",\"statePensionMessageWelsh\":\"Neges pensiwn gwladol yn Saesneg.\"}]\r\n}";
        }
    }
}