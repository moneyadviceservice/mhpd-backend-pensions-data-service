using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using CommonServices.Models;
using Xunit;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PensionRequestFunctionUnitTests
{
    public class ViewDataToMHPDUnitTest
    {
       
        [Fact]
        public void WhenViewDataNoArrangments_ThenThrowError()
        {
            var externalAssetId = "459566f6-5fce-479e-a098-298ca9676a85";           
            var viewDataPayload = "{\"view_data\":" + GetViewDataPayload() + "}";
            JsonObject requestJson = JsonSerializer.Deserialize<JsonObject>(viewDataPayload)!;
            var viewData = requestJson["view_data"]!.AsObject();
            viewData.Remove("arrangements");
            var newViewDataPayload = JsonSerializer.Serialize<JsonObject>(requestJson)!;
            var transformer = new PensionRequestFunction.Transformer.ViewDataToPensionArrangementTransformer();
            var ex = Assert.Throws<Exception>(() => transformer.Transform(externalAssetId, newViewDataPayload)); ;
                         
            Assert.Equal("No arrangements present", ex.Message);
        }

        [Fact]
        public void WhenViewDataNoViewDataPresent_ThenThrowError()
        {
            var externalAssetId = "459566f6-5fce-479e-a098-298ca9676a85";
            var viewDataPayload = "{}";
            JsonObject requestJson = JsonSerializer.Deserialize<JsonObject>(viewDataPayload)!;            
            var newViewDataPayload = JsonSerializer.Serialize<JsonObject>(requestJson)!;
            var transformer = new PensionRequestFunction.Transformer.ViewDataToPensionArrangementTransformer();
            var ex = Assert.Throws<Exception>(() => transformer.Transform(externalAssetId, newViewDataPayload)); ;

            Assert.Equal("No view_data present", ex.Message);
        }
        [Fact]
        public void WhenViewDataEmptyArrangementsPresent_ThenThrowError()
        {
            var externalAssetId = "459566f6-5fce-479e-a098-298ca9676a85";
            var viewDataPayload = "{\"view_data\":" + GetEmptyViewDataPayload() + "}";
            JsonObject requestJson = JsonSerializer.Deserialize<JsonObject>(viewDataPayload)!;
            var viewData = requestJson["view_data"]!.AsObject();
            viewData.Remove("arrangements");
            var newViewDataPayload = JsonSerializer.Serialize<JsonObject>(requestJson)!;
            var transformer = new PensionRequestFunction.Transformer.ViewDataToPensionArrangementTransformer();
            var ex = Assert.Throws<Exception>(() => transformer.Transform(externalAssetId, newViewDataPayload)); ;

            Assert.Equal("No arrangements present", ex.Message);
        }
        [Fact]
        public void WhenViewDataToMHPDIsCalled_ThenItShouldReturnTrue()
        {
            // Assign
            var matchTypeElement = "matchType";
            var externalAssetId = "459566f6-5fce-479e-a098-298ca9676a85";
            var externalAssetIdNodeName = "externalAssetId";
            var matchType = "POSS";
            var usage = """
                [
                  "A",
                  "M"
                ]
                """;
            var pensionProviderSchemeName = "My Company Direct Contribution Scheme";
            var alternateNameType = "FOR";
            var name = "Converted from My Old Direct Contribution Scheme";
            var preferredFalse = "false";
            var email = "example@examplemyline.com";
            var number = "+123 1111111111";
            var preferredTrue = "true";
            var pensionAdministratorName = "Pension Company 1";
            var pensionRequestPayload = GetRequestPayload();
            var viewDataPayload = "{\"view_data\":" + GetViewDataPayload() + "}";
            PensionRequestPayload pensionRequestPayloadDeserialized = JsonSerializer.Deserialize<PensionRequestPayload>(pensionRequestPayload)!;

            // Act
            var transformer = new PensionRequestFunction.Transformer.ViewDataToPensionArrangementTransformer ();
            var result = transformer.Transform(externalAssetId, viewDataPayload);

            JsonNode pensionArrangementNode = JsonNode.Parse(result)!;

            //extract root of document
            var doc = JsonDocument.Parse(result);
            var root = doc.RootElement;            
            var pensionArrangements = root.GetProperty("pensionArrangements");
            var arrangements = pensionArrangements.EnumerateArray().ToList();

            //extract pensionAdministrator
            var pensionArrangement = pensionArrangementNode["pensionArrangements"]!;
            var pensionArrangementString = pensionArrangement.ToString();           
            var pensionAdministratorEmail = pensionArrangement[0]!["pensionAdministrator"]!["contactMethods"]![0]!["contactMethodDetails"]!["email"]!.ToString();
            var pensionAdministratorPreferredFalse = pensionArrangement[0]!["pensionAdministrator"]!["contactMethods"]![0]!["preferred"]!.ToString();
            var pensionAdministratorPreferredTrue = pensionArrangement[0]!["pensionAdministrator"]!["contactMethods"]![1]!["preferred"]!.ToString();
            var pensionAdministratorNumber = pensionArrangement[0]!["pensionAdministrator"]!["contactMethods"]![1]!["contactMethodDetails"]!["number"]!.ToString();
            var alternateSchemeNamesNameType = pensionArrangement[0]!["alternateSchemeNames"]![0]!["alternateNameType"]!.ToString();
            var schemeName = pensionArrangement[0]!["schemeName"]!.ToString();
            var matchTypeResult = pensionArrangement[0]!["matchType"]!.ToString();
            var alternateSchemeNamesName= pensionArrangement[0]!["alternateSchemeNames"]![0]!["name"]!.ToString();
            var pensionAdministratorResultName = pensionArrangement[0]!["pensionAdministrator"]!["name"]!.ToString();
            var pensionAdministratorUsage = pensionArrangement[0]!["pensionAdministrator"]!["contactMethods"]![1]!["contactMethodDetails"]!["usage"]!.ToString();
            //Assert
            Assert.NotNull(result);           
            Assert.Contains(externalAssetIdNodeName, pensionArrangementString);
            Assert.Contains(externalAssetId, pensionRequestPayloadDeserialized.AssetGuid);
            Assert.Contains(matchTypeElement, pensionArrangementString);
            Assert.Equal(email, pensionAdministratorEmail);
            Assert.Equal(preferredFalse, pensionAdministratorPreferredFalse);
            Assert.Equal(preferredTrue, pensionAdministratorPreferredTrue);
            Assert.Equal(number, pensionAdministratorNumber);
            Assert.Equal(pensionProviderSchemeName, schemeName);
            Assert.Equal(name, alternateSchemeNamesName);
            Assert.Equal(matchType, matchTypeResult);
            Assert.Equal(alternateNameType, alternateSchemeNamesNameType);
            Assert.Equal(pensionAdministratorName, pensionAdministratorResultName);
            Assert.Equal(usage, pensionAdministratorUsage);
        }

        [Fact]
        public void WhenViewDataToMHPDIsCalled_WithEmptyEmailUsageViewDataPayload_ThenItShouldReturnTrue()
        {            

            // Assign
            var matchTypeElement = "matchType";
            var externalAssetId = "459566f6-5fce-479e-a098-298ca9676a85";            
            var externalAssetIdNodeName = "externalAssetId";
            var email = string.Empty;
            var pensionProviderSchemeName = "Organization PDPViewDataPayload";
            var alternateNameType = "FOR";
            var name = "Diverted from PDPViewDataPayload";
            var number = "+44 88888888";
            var usage = """[]""";
            var matchType = "POSS";
            var preferredFalse = "false";
            var preferredTrue = "true";
            var pensionAdministratorName = "MHPension Company9";
            var pensionRequestPayload = GetRequestPayload();            
            var viewDataPayload = "{\"view_data\":" + GetEmptyEmailUasgeViewDataPayload() + "}";
            PensionRequestPayload pensionRequestPayloadDeserialized = JsonSerializer.Deserialize<PensionRequestPayload>(pensionRequestPayload)!;

            // Act
            var transformer = new PensionRequestFunction.Transformer.ViewDataToPensionArrangementTransformer();
            var result = transformer.Transform(externalAssetId, viewDataPayload);

            JsonNode pensionArrangementNode = JsonNode.Parse(result)!;

            //extract root of document
            var doc = JsonDocument.Parse(result);
            var root = doc.RootElement;
            var pensionArrangements = root.GetProperty("pensionArrangements");
            var arrangements = pensionArrangements.EnumerateArray().ToList();

            //extract pensionAdministrator
            var pensionArrangement = pensionArrangementNode["pensionArrangements"]!;
            var pensionArrangementString = pensionArrangement.ToString();           
            var pensionAdministratorEmail = pensionArrangement[0]!["pensionAdministrator"]!["contactMethods"]![0]!["contactMethodDetails"]!["email"]!.ToString();
            var pensionAdministratorNumber = pensionArrangement[0]!["pensionAdministrator"]!["contactMethods"]![1]!["contactMethodDetails"]!["number"]!.ToString();
            var pensionAdministratorUsage = pensionArrangement[0]!["pensionAdministrator"]!["contactMethods"]![1]!["contactMethodDetails"]!["usage"]!.ToString();
            var alternateSchemeNamesNameType = pensionArrangement[0]!["alternateSchemeNames"]![0]!["alternateNameType"]!.ToString();
            var pensionAdministratorPreferredFalse = pensionArrangement[0]!["pensionAdministrator"]!["contactMethods"]![0]!["preferred"]!.ToString();
            var pensionAdministratorPreferredTrue = pensionArrangement[0]!["pensionAdministrator"]!["contactMethods"]![1]!["preferred"]!.ToString();
            var schemeName = pensionArrangement[0]!["schemeName"]!.ToString();
            var matchTypeResult = pensionArrangement[0]!["matchType"]!.ToString();
            var alternateSchemeNamesName = pensionArrangement[0]!["alternateSchemeNames"]![0]!["name"]!.ToString();
            var pensionAdministratorResultName = pensionArrangement[0]!["pensionAdministrator"]!["name"]!.ToString();

            //Assert
            Assert.NotNull(result);
            Assert.Contains(externalAssetIdNodeName, pensionArrangementString);
            Assert.Contains(externalAssetId, pensionRequestPayloadDeserialized.AssetGuid);
            Assert.Contains(matchTypeElement, pensionArrangementString);
            Assert.Equal(email, pensionAdministratorEmail);
            Assert.Equal(number, pensionAdministratorNumber);
            Assert.Equal(usage, pensionAdministratorUsage);
            Assert.Equal(alternateNameType, alternateSchemeNamesNameType);
            Assert.Equal(pensionProviderSchemeName, schemeName);
            Assert.Equal(name, alternateSchemeNamesName);
            Assert.Equal(matchType, matchTypeResult);
            Assert.Equal(preferredFalse, pensionAdministratorPreferredFalse);
            Assert.Equal(preferredTrue, pensionAdministratorPreferredTrue);
            Assert.Equal(pensionAdministratorName, pensionAdministratorResultName);
        }

        [Fact]
        public void WhenViewDataToMHPDIsCalled_WitEmptyNumberViewDataPayload_ThenItShouldReturnTrue()
        {          

            // Assign
            var matchTypeElement = "matchType";
            var externalAssetId = "88888f8-5ffg-479f-a098-298ca9676a88";
            var externalAssetIdNodeName = "externalAssetId";
            var email = "evergreen@examplemyline.com";
            var number = string.Empty;
            var usage = """
                [
                  "S",
                  "W"
                ]
                """;
            var pensionProviderSchemeName = "CompanyViewDataPayload";
            var name = "Excel Diverted from ViewDataPayload";            
            var alternateNameType = "OTH";
            var matchType = "POSS";
            var preferredFalse = "false";
            var preferredTrue = "true";
            var pensionAdministratorName = "World Pension Company 9";
            var pensionRequestPayload = GetNewAssetRequestPayload();
            var viewDataPayload = "{\"view_data\":" + GetEmptyNumberViewDataPayload() + "}";
            PensionRequestPayload pensionRequestPayloadDeserialized = JsonSerializer.Deserialize<PensionRequestPayload>(pensionRequestPayload)!;

            // Act
            var transformer = new PensionRequestFunction.Transformer.ViewDataToPensionArrangementTransformer();
            var result = transformer.Transform(externalAssetId, viewDataPayload);

            JsonNode pensionArrangementNode = JsonNode.Parse(result)!;

            //extract root of document
            var doc = JsonDocument.Parse(result);
            var root = doc.RootElement;
            var pensionArrangements = root.GetProperty("pensionArrangements");
            var arrangements = pensionArrangements.EnumerateArray().ToList();

            //extract pensionAdministrator
            var pensionArrangement = pensionArrangementNode["pensionArrangements"]!;
            var pensionArrangementString = pensionArrangement.ToString();            
            var pensionAdministratorEmail = pensionArrangement[0]!["pensionAdministrator"]!["contactMethods"]![0]!["contactMethodDetails"]!["email"]!.ToString();
            var pensionAdministratorNumber = pensionArrangement[0]!["pensionAdministrator"]!["contactMethods"]![2]!["contactMethodDetails"]!["number"]!.ToString();
            var pensionAdministratorUsage = pensionArrangement[0]!["pensionAdministrator"]!["contactMethods"]![2]!["contactMethodDetails"]!["usage"]!.ToString();
            var alternateSchemeNamesNameType = pensionArrangement[0]!["alternateSchemeNames"]![0]!["alternateNameType"]!.ToString();
            var pensionAdministratorPreferredFalse = pensionArrangement[0]!["pensionAdministrator"]!["contactMethods"]![0]!["preferred"]!.ToString();
            var pensionAdministratorPreferredTrue = pensionArrangement[0]!["pensionAdministrator"]!["contactMethods"]![1]!["preferred"]!.ToString();
            var schemeName = pensionArrangement[0]!["schemeName"]!.ToString();
            var matchTypeResult = pensionArrangement[0]!["matchType"]!.ToString();
            var alternateSchemeNamesName = pensionArrangement[0]!["alternateSchemeNames"]![0]!["name"]!.ToString();
            var pensionAdministratorResultName = pensionArrangement[0]!["pensionAdministrator"]!["name"]!.ToString();

            //Assert
            Assert.NotNull(result);
            Assert.Contains(externalAssetIdNodeName, pensionArrangementString);
            Assert.Contains(externalAssetId, pensionRequestPayloadDeserialized.AssetGuid);
            Assert.Contains(matchTypeElement, pensionArrangementString);
            Assert.Equal(email, pensionAdministratorEmail);
            Assert.Equal(number, pensionAdministratorNumber);
            Assert.Equal(alternateNameType, alternateSchemeNamesNameType);
            Assert.Equal(pensionProviderSchemeName, schemeName);
            Assert.Equal(name, alternateSchemeNamesName);
            Assert.Equal(matchType, matchTypeResult);
            Assert.Equal(preferredFalse, pensionAdministratorPreferredFalse);
            Assert.Equal(preferredTrue, pensionAdministratorPreferredTrue);
            Assert.Equal(pensionAdministratorName, pensionAdministratorResultName);
            Assert.Equal(usage, pensionAdministratorUsage);
        }

        [Fact]
        public void WhenViewDataToMHPDIsCalled_WithEmptyAssetId_Email_Number_UsageViewDataPayload_ThenItShouldReturnTrue()
        {

            // Assign
            var matchTypeElement = "matchType";            
            var externalAssetId = string.Empty;
            var externalAssetIdNodeName = "externalAssetId";
            var email = string.Empty;
            var number = string.Empty;
            var usage = """[]""";
            var pensionProviderSchemeName = "Organization PDPViewDataPayload";
            var name = "Diverted from PDPViewDataPayload";
            var alternateNameType = "FOR";
            var matchType = "POSS";
            var preferredFalse = "false";
            var preferredTrue = "true";
            var pensionAdministratorName = "Pension Company 1";
            var pensionRequestPayload = GetEmptyAssetRequestPayload();
            var viewDataPayload = "{\"view_data\":" + GetEmptyEmailyNumberUsageViewDataPayload() + "}";
            PensionRequestPayload pensionRequestPayloadDeserialized = JsonSerializer.Deserialize<PensionRequestPayload>(pensionRequestPayload)!;

            // Act
            var transformer = new PensionRequestFunction.Transformer.ViewDataToPensionArrangementTransformer();
            var result = transformer.Transform(externalAssetId, viewDataPayload);

            JsonNode pensionArrangementNode = JsonNode.Parse(result)!;

            //extract root of document
            var doc = JsonDocument.Parse(result);
            var root = doc.RootElement;
            var pensionArrangements = root.GetProperty("pensionArrangements");
            var arrangements = pensionArrangements.EnumerateArray().ToList();            
            //extract pensionAdministrator
            var pensionArrangement = pensionArrangementNode["pensionArrangements"]!;
            var pensionArrangementString = pensionArrangement.ToString();
            var pensionAdministratorEmail = pensionArrangement[0]!["pensionAdministrator"]!["contactMethods"]![0]!["contactMethodDetails"]!["email"]!.ToString();
            var pensionAdministratorNumber = pensionArrangement[0]!["pensionAdministrator"]!["contactMethods"]![1]!["contactMethodDetails"]!["number"]!.ToString();
            var pensionAdministratorUsage = pensionArrangement[0]!["pensionAdministrator"]!["contactMethods"]![1]!["contactMethodDetails"]!["usage"]!.ToString();
            var alternateSchemeNamesNameType = pensionArrangement[0]!["alternateSchemeNames"]![0]!["alternateNameType"]!.ToString();
            var pensionAdministratorPreferredFalse = pensionArrangement[0]!["pensionAdministrator"]!["contactMethods"]![0]!["preferred"]!.ToString();
            var pensionAdministratorPreferredTrue = pensionArrangement[0]!["pensionAdministrator"]!["contactMethods"]![1]!["preferred"]!.ToString();
            var schemeName = pensionArrangement[0]!["schemeName"]!.ToString();
            var matchTypeResult = pensionArrangement[0]!["matchType"]!.ToString();
            var alternateSchemeNamesName = pensionArrangement[0]!["alternateSchemeNames"]![0]!["name"]!.ToString();
            var pensionAdministratorResultName = pensionArrangement[0]!["pensionAdministrator"]!["name"]!.ToString();

            //Assert
            Assert.NotNull(result);
            Assert.Contains(externalAssetIdNodeName, pensionArrangementString);
            Assert.Contains(externalAssetId, pensionRequestPayloadDeserialized.AssetGuid);
            Assert.Contains(matchTypeElement, pensionArrangementString);
            Assert.Equal(email, pensionAdministratorEmail);
            Assert.Equal(number, pensionAdministratorNumber);
            Assert.Equal(usage, pensionAdministratorUsage);
            Assert.Equal(alternateNameType, alternateSchemeNamesNameType);
            Assert.Equal(pensionProviderSchemeName, schemeName);
            Assert.Equal(name, alternateSchemeNamesName);
            Assert.Equal(matchType, matchTypeResult);
            Assert.Equal(preferredFalse, pensionAdministratorPreferredFalse);
            Assert.Equal(preferredTrue, pensionAdministratorPreferredTrue);
            Assert.Equal(pensionAdministratorName, pensionAdministratorResultName);
        }

        [Fact]
        public void WhenViewDataToMHPDIsCalled_WithEmptyAlternateNameTypeDataPayload_ThenItShouldReturnTrue()
        {

            // Assign
            var matchTypeElement = "matchType";
            var externalAssetId = "459566f6-5fce-479e-a098-298ca9676a85";
            var externalAssetIdNodeName = "externalAssetId";
            var email = "mhpdexample@mhpdline.com";            
            var number = "+44 88888888";
            var usage = """
                [
                  "A",
                  "M"
                ]
                """;
            var pensionProviderSchemeName = "Organization PDPViewDataPayload";
            var name = "Diverted from PDPViewDataPayload";
            var matchType = "POSS";
            var preferredFalse = "false";
            var preferredTrue = "true";
            var pensionAdministratorName = "Pension Company 1";
            var alternateNameType = string.Empty;
            var pensionRequestPayload = GetRequestPayload();
            var viewDataPayload = "{\"view_data\":" + GetEmptyPossibleMatchReferenceViewDataPayload() + "}";
            PensionRequestPayload pensionRequestPayloadDeserialized = JsonSerializer.Deserialize<PensionRequestPayload>(pensionRequestPayload)!;

            // Act
            var transformer = new PensionRequestFunction.Transformer.ViewDataToPensionArrangementTransformer();
            var result = transformer.Transform(externalAssetId, viewDataPayload);

            JsonNode pensionArrangementNode = JsonNode.Parse(result)!;

            //extract root of document
            var doc = JsonDocument.Parse(result);
            var root = doc.RootElement;
            var pensionArrangements = root.GetProperty("pensionArrangements");
            var arrangements = pensionArrangements.EnumerateArray().ToList();

            //extract pensionAdministrator           
            var pensionArrangement = pensionArrangementNode["pensionArrangements"]!;
            var pensionArrangementString = pensionArrangement.ToString();
            var pensionAdministratorEmail = pensionArrangement[0]!["pensionAdministrator"]!["contactMethods"]![0]!["contactMethodDetails"]!["email"]!.ToString();
            var pensionAdministratorNumber = pensionArrangement[0]!["pensionAdministrator"]!["contactMethods"]![1]!["contactMethodDetails"]!["number"]!.ToString();
            var alternateSchemeNamesNameType = pensionArrangement[0]!["alternateSchemeNames"]![0]!["alternateNameType"]!.ToString();
            var pensionAdministratorPreferredFalse = pensionArrangement[0]!["pensionAdministrator"]!["contactMethods"]![0]!["preferred"]!.ToString();
            var pensionAdministratorPreferredTrue = pensionArrangement[0]!["pensionAdministrator"]!["contactMethods"]![1]!["preferred"]!.ToString();
            var pensionAdministratorUsage = pensionArrangement[0]!["pensionAdministrator"]!["contactMethods"]![1]!["contactMethodDetails"]!["usage"]!.ToString();
            var schemeName = pensionArrangement[0]!["schemeName"]!.ToString();            
            var matchTypeResult = pensionArrangement[0]!["matchType"]!.ToString();
            var alternateSchemeNamesName = pensionArrangement[0]!["alternateSchemeNames"]![0]!["name"]!.ToString();
            var pensionAdministratorResultName = pensionArrangement[0]!["pensionAdministrator"]!["name"]!.ToString();

            //Assert
            Assert.NotNull(result);
            Assert.Contains(externalAssetIdNodeName, pensionArrangementString);
            Assert.Contains(externalAssetId, pensionRequestPayloadDeserialized.AssetGuid);
            Assert.Contains(matchTypeElement, pensionArrangementString);
            Assert.Equal(email, pensionAdministratorEmail);
            Assert.Equal(usage, pensionAdministratorUsage);
            Assert.Equal(number, pensionAdministratorNumber);           
            Assert.Equal(alternateNameType, alternateSchemeNamesNameType);            
            Assert.Equal(pensionProviderSchemeName, schemeName);
            Assert.Equal(name, alternateSchemeNamesName);
            Assert.Equal(matchType, matchTypeResult);
            Assert.Equal(preferredFalse, pensionAdministratorPreferredFalse);
            Assert.Equal(preferredTrue, pensionAdministratorPreferredTrue);
            Assert.Equal(pensionAdministratorName, pensionAdministratorResultName);
        }

        [Fact]
        public void WhenEmptyViewData_ToMHPDIsCalled_ThenItShouldReturnFalse()
        {
            // Assign
            var matchTypeElement = "matchType";
            var externalAssetId = "459566f6-5fce-479e-a098-298ca9676a85";
            var externalAssetIdNodeName = "externalAssetId"; 
            var pensionRequestPayload = GetRequestPayload();           
            var viewDataPayload = "{\"view_data\":" + GetEmptyViewDataPayload() + "}";
            PensionRequestPayload pensionRequestPayloadDeserialized = JsonSerializer.Deserialize<PensionRequestPayload>(pensionRequestPayload)!;

            // Act
            var transformer = new PensionRequestFunction.Transformer.ViewDataToPensionArrangementTransformer();
            var result = transformer.Transform(externalAssetId, viewDataPayload);
            JsonNode pensionArrangementNode = JsonNode.Parse(result)!;                  

            //extract pensionAdministrator
            var pensionArrangement = pensionArrangementNode["pensionArrangements"]!;
            var pensionArrangementString = pensionArrangement.ToString(); 
            
            //Assert
            Assert.NotNull(result);
            Assert.DoesNotContain(externalAssetIdNodeName, pensionArrangementString);
            Assert.Equal(externalAssetId, pensionRequestPayloadDeserialized.AssetGuid);
            Assert.DoesNotContain(matchTypeElement, pensionArrangementString);
        }

        [Fact]
        public void WhenEmptyRequestPayload_ToMHPDIsCalled_ThenItShouldReturnTrue()
        {
            // Assign
            var matchTypeElement = "matchType";
            var externalAssetId = string.Empty;
            var externalAssetIdNodeName = "externalAssetId";
            var pensionProviderSchemeName = "My Company Direct Contribution Scheme";
            var name = "Converted from My Old Direct Contribution Scheme";
            var alternateNameType = "FOR";
            var matchType = "POSS";
            var usage = """
                [
                  "A",
                  "M"
                ]
                """;
            var email = "example@examplemyline.com";
            var number = "+123 1111111111";
            var preferredFalse = "false";
            var preferredTrue = "true";
            var pensionAdministratorName = "Pension Company 1";
            var pensionRequestPayload = GetEmptyRequestPayload();
            var viewDataPayload = "{\"view_data\":" + GetViewDataPayload() + "}";
            PensionRequestPayload pensionRequestPayloadDeserialized = JsonSerializer.Deserialize<PensionRequestPayload>(pensionRequestPayload)!;

            // Act
            var transformer = new PensionRequestFunction.Transformer.ViewDataToPensionArrangementTransformer();
            var result = transformer.Transform(externalAssetId, viewDataPayload);
            JsonNode pensionArrangementNode = JsonNode.Parse(result)!;

            //extract pensionAdministrator
            var pensionArrangement = pensionArrangementNode["pensionArrangements"]!;
            var pensionArrangementString = pensionArrangement.ToString();           
            var alternateSchemeNamesNameType = pensionArrangement[0]!["alternateSchemeNames"]![0]!["alternateNameType"]!.ToString();
            var pensionAdministratorPreferredFalse = pensionArrangement[0]!["pensionAdministrator"]!["contactMethods"]![0]!["preferred"]!.ToString();
            var pensionAdministratorPreferredTrue = pensionArrangement[0]!["pensionAdministrator"]!["contactMethods"]![1]!["preferred"]!.ToString();
            var pensionAdministratorUsage = pensionArrangement[0]!["pensionAdministrator"]!["contactMethods"]![1]!["contactMethodDetails"]!["usage"]!.ToString();
            var schemeName = pensionArrangement[0]!["schemeName"]!.ToString();
            var matchTypeResult = pensionArrangement[0]!["matchType"]!.ToString();
            var alternateSchemeNamesName = pensionArrangement[0]!["alternateSchemeNames"]![0]!["name"]!.ToString();
            var pensionAdministratorResultName = pensionArrangement[0]!["pensionAdministrator"]!["name"]!.ToString();
            var pensionAdministratorEmail = pensionArrangement[0]!["pensionAdministrator"]!["contactMethods"]![0]!["contactMethodDetails"]!["email"]!.ToString();
            var pensionAdministratorNumber = pensionArrangement[0]!["pensionAdministrator"]!["contactMethods"]![1]!["contactMethodDetails"]!["number"]!.ToString();
            //Assert
            Assert.NotNull(result);
            Assert.Contains(externalAssetIdNodeName, pensionArrangementString);
            Assert.Equal(externalAssetId, pensionRequestPayloadDeserialized.AssetGuid);
            Assert.Contains(matchTypeElement, pensionArrangementString);
            Assert.Equal(alternateNameType, alternateSchemeNamesNameType);
            Assert.Equal(pensionProviderSchemeName, schemeName);
            Assert.Equal(name, alternateSchemeNamesName);
            Assert.Equal(usage, pensionAdministratorUsage);
            Assert.Equal(matchType, matchTypeResult);
            Assert.Equal(preferredFalse, pensionAdministratorPreferredFalse);
            Assert.Equal(preferredTrue, pensionAdministratorPreferredTrue);
            Assert.Equal(pensionAdministratorName, pensionAdministratorResultName);
            Assert.Equal(email, pensionAdministratorEmail);
            Assert.Equal(number, pensionAdministratorNumber);
       
        }
        private string GetRequestPayload()
        {
            return "{\r\n\t\"pensionRetrievalRecordId\": \"e01a9df7-f147-4a3a-a1dd-0507432a5b7f\",\r\n\t\"pei\": \"7075aa11-10ad-4b2f-a9f5-1068e79119bf:1ba03e25-659a-43b8-ae77-b956df168969\",\r\n\t\"iss\": \"DATA_PROVIDER_1fd1da88-9fb3-461c-a48a-3dba21bfba17\",\r\n\t\"userSessionId\": \"459566f6-5fce-479e-a098-298ca9676a85\",\r\n\t\"asset_guid\": \"459566f6-5fce-479e-a098-298ca9676a85\"\r\n}";
        }
        private string GetEmptyRequestPayload()
        {
            return "{\r\n\t\"pensionRetrievalRecordId\": \"\",\r\n\t\"pei\": \"\",\r\n\t\"iss\": \"\",\r\n\t\"userSessionId\": \"\",\r\n\t\"asset_guid\": \"\"\r\n}";
        }
        private string GetEmptyAssetRequestPayload()
        {
            return "{\r\n\t\"pensionRetrievalRecordId\": \"e01a9df7-f147-4a3a-a1dd-0507432a5b7f\",\r\n\t\"pei\": \"7075aa11-10ad-4b2f-a9f5-1068e79119bf:1ba03e25-659a-43b8-ae77-b956df168969\",\r\n\t\"iss\": \"DATA_PROVIDER_1fd1da88-9fb3-461c-a48a-3dba21bfba17\",\r\n\t\"userSessionId\": \"\",\r\n\t\"asset_guid\": \"\"\r\n}";
        }
        private string GetViewDataPayload()
        {
            return "{\r\n\t\"arrangements\": [\r\n\t\t{\r\n\t\t\t\"pensionProviderSchemeName\": \"My Company Direct Contribution Scheme\",\r\n\t\t\t\"alternateSchemeName\": {\r\n\t\t\t\t\"name\": \"Converted from My Old Direct Contribution Scheme\",\r\n\t\t\t\t\"alternateNameType\": \"FOR\"\r\n\t\t\t},\r\n\t\t\t\"possibleMatch\": true,\r\n\t\t\t\"possibleMatchReference\": \"Q12345\",\r\n\t\t\t\"pensionAdministrator\": {\r\n\t\t\t\t\"name\": \"Pension Company 1\",\r\n\t\t\t\t\"contactMethods\": [\r\n\t\t\t\t\t{\r\n\t\t\t\t\t\t\"preferred\": false,\r\n\t\t\t\t\t\t\"contactMethodDetails\": {\r\n\t\t\t\t\t\t\t\"email\": \"example@examplemyline.com\"\r\n\t\t\t\t\t\t}\r\n\t\t\t\t\t},\r\n\t\t\t\t\t{\r\n\t\t\t\t\t\t\"preferred\": true,\r\n\t\t\t\t\t\t\"contactMethodDetails\": {\r\n\t\t\t\t\t\t\t\"number\": \"+123 1111111111\",\r\n\t\t\t\t\t\t\t\"usage\": [\r\n\t\t\t\t\t\t\t\t\"A\",\r\n\t\t\t\t\t\t\t\t\"M\"\r\n\t\t\t\t\t\t\t]\r\n\t\t\t\t\t\t}\r\n\t\t\t\t\t}\r\n\t\t\t\t]\r\n\t\t\t}\r\n\t\t}\r\n\t]\r\n}";
        }

        private string GetEmptyViewDataPayload()
        {
            return "{\r\n\t\"arrangements\": []\r\n}";           
        }

        private string GetNewAssetRequestPayload()
        {
            return "{\r\n\t\"pensionRetrievalRecordId\": \"e99a9ds9-f147-4a3a-a1ss-0507432a5b7f\",\r\n\t\"pei\": \"9995aa11-10ad-4b2f-a9f5-1068e79119bf:1ba03e25-659a-43b8-ae77-b956df9\",\r\n\t\"iss\": \"DATA_PROVIDER_x123z-9fb3-461c-a48a-3dba21bfba17\",\r\n\t\"userSessionId\": \"459566f6-5fce-479e-a098-298ca9676a85\",\r\n\t\"asset_guid\": \"88888f8-5ffg-479f-a098-298ca9676a88\"\r\n}";
        }

        private string GetEmptyEmailUasgeViewDataPayload()
        {            
            return "{\r\n\t\"arrangements\": [\r\n\t\t{\r\n\t\t\t\"pensionProviderSchemeName\": \"Organization PDPViewDataPayload\",\r\n\t\t\t\"alternateSchemeName\": {\r\n\t\t\t\t\"name\": \"Diverted from PDPViewDataPayload\",\r\n\t\t\t\t\"alternateNameType\": \"FOR\"\r\n\t\t\t},\r\n\t\t\t\"possibleMatch\": true,\r\n\t\t\t\"possibleMatchReference\": \"Q98989\",\r\n\t\t\t\"pensionAdministrator\": {\r\n\t\t\t\t\"name\": \"MHPension Company9\",\r\n\t\t\t\t\"contactMethods\": [\r\n\t\t\t\t\t{\r\n\t\t\t\t\t\t\"preferred\": false,\r\n\t\t\t\t\t\t\"contactMethodDetails\": {\r\n\t\t\t\t\t\t\t\"email\": \"\"\r\n\t\t\t\t\t\t}\r\n\t\t\t\t\t},\r\n\t\t\t\t\t{\r\n\t\t\t\t\t\t\"preferred\": true,\r\n\t\t\t\t\t\t\"contactMethodDetails\": {\r\n\t\t\t\t\t\t\t\"number\": \"+44 88888888\",\r\n\t\t\t\t\t\t\t\"usage\": []\r\n\t\t\t\t\t\t}\r\n\t\t\t\t\t}\r\n\t\t\t\t]\r\n\t\t\t}\r\n\t\t}\r\n\t]\r\n}";
        }
        private string GetEmptyPossibleMatchReferenceViewDataPayload()
        {            
            return "{\r\n\t\"arrangements\": [\r\n\t\t{\r\n\t\t\t\"pensionProviderSchemeName\": \"Organization PDPViewDataPayload\",\r\n\t\t\t\"alternateSchemeName\": {\r\n\t\t\t\t\"name\": \"Diverted from PDPViewDataPayload\",\r\n\t\t\t\t\"alternateNameType\": \"\"\r\n\t\t\t},\r\n\t\t\t\"possibleMatch\": true,\r\n\t\t\t\"possibleMatchReference\": \"\",\r\n\t\t\t\"pensionAdministrator\": {\r\n\t\t\t\t\"name\": \"Pension Company 1\",\r\n\t\t\t\t\"contactMethods\": [\r\n\t\t\t\t\t{\r\n\t\t\t\t\t\t\"preferred\": false,\r\n\t\t\t\t\t\t\"contactMethodDetails\": {\r\n\t\t\t\t\t\t\t\"email\": \"mhpdexample@mhpdline.com\"\r\n\t\t\t\t\t\t}\r\n\t\t\t\t\t},\r\n\t\t\t\t\t{\r\n\t\t\t\t\t\t\"preferred\": true,\r\n\t\t\t\t\t\t\"contactMethodDetails\": {\r\n\t\t\t\t\t\t\t\"number\": \"+44 88888888\",\r\n\t\t\t\t\t\t\t\"usage\": [\r\n\t\t\t\t\t\t\t\t\"A\",\r\n\t\t\t\t\t\t\t\t\"M\"\r\n\t\t\t\t\t\t\t]\r\n\t\t\t\t\t\t}\r\n\t\t\t\t\t}\r\n\t\t\t\t]\r\n\t\t\t}\r\n\t\t}\r\n\t]\r\n}";
        }
        private string GetEmptyEmailyNumberUsageViewDataPayload()
        {            
            return "{\r\n\t\"arrangements\": [\r\n\t\t{\r\n\t\t\t\"pensionProviderSchemeName\": \"Organization PDPViewDataPayload\",\r\n\t\t\t\"alternateSchemeName\": {\r\n\t\t\t\t\"name\": \"Diverted from PDPViewDataPayload\",\r\n\t\t\t\t\"alternateNameType\": \"FOR\"\r\n\t\t\t},\r\n\t\t\t\"possibleMatch\": true,\r\n\t\t\t\"possibleMatchReference\": \"Q98989\",\r\n\t\t\t\"pensionAdministrator\": {\r\n\t\t\t\t\"name\": \"Pension Company 1\",\r\n\t\t\t\t\"contactMethods\": [\r\n\t\t\t\t\t{\r\n\t\t\t\t\t\t\"preferred\": false,\r\n\t\t\t\t\t\t\"contactMethodDetails\": {\r\n\t\t\t\t\t\t\t\"email\": \"\"\r\n\t\t\t\t\t\t}\r\n\t\t\t\t\t},\r\n\t\t\t\t\t{\r\n\t\t\t\t\t\t\"preferred\": true,\r\n\t\t\t\t\t\t\"contactMethodDetails\": {\r\n\t\t\t\t\t\t\t\"number\": \"\",\r\n\t\t\t\t\t\t\t\"usage\": []\r\n\t\t\t\t\t\t}\r\n\t\t\t\t\t}\r\n\t\t\t\t]\r\n\t\t\t}\r\n\t\t}\r\n\t]\r\n}";
        }
        private string GetEmptyNumberViewDataPayload()
        {            
            return "{\r\n\t\"arrangements\": [\r\n\t\t{\r\n\t\t\t\"pensionProviderSchemeName\": \"CompanyViewDataPayload\",\r\n\t\t\t\"alternateSchemeName\": {\r\n\t\t\t\t\"name\": \"Excel Diverted from ViewDataPayload\",\r\n\t\t\t\t\"alternateNameType\": \"OTH\",\r\n\t\t\t\t\"altNameGroup\": \"ADM\"\r\n\t\t\t},\r\n\t\t\t\"possibleMatch\": true,\r\n\t\t\t\"possibleMatchReference\": \"A90000\",\r\n\t\t\t\"pensionAdministrator\": {\r\n\t\t\t\t\"name\": \"World Pension Company 9\",\r\n\t\t\t\t\"contactMethods\": [\r\n\t\t\t\t\t{\r\n\t\t\t\t\t\t\"preferred\": false,\r\n\t\t\t\t\t\t\"contactMethodDetails\": {\r\n\t\t\t\t\t\t\t\"email\": \"evergreen@examplemyline.com\"\r\n\t\t\t\t\t\t}\r\n\t\t\t\t\t},\r\n\t\t\t\t\t{\r\n\t\t\t\t\t\t\"preferred\": true,\r\n\t\t\t\t\t\t\"contactMethodDetails\": {\r\n\t\t\t\t\t\t\t\"email\": \"evergreen@examplemyline.com\"\r\n\t\t\t\t\t\t}\r\n\t\t\t\t\t},\r\n\t\t\t\t\t{\r\n\t\t\t\t\t\t\"preferred\": true,\r\n\t\t\t\t\t\t\"contactMethodDetails\": {\r\n\t\t\t\t\t\t\t\"number\": \"\",\r\n\t\t\t\t\t\t\t\"usage\": [\r\n\t\t\t\t\t\t\t\t\"S\",\r\n\t\t\t\t\t\t\t\t\"W\"\r\n\t\t\t\t\t\t\t]\r\n\t\t\t\t\t\t}\r\n\t\t\t\t\t}\r\n\t\t\t\t]\r\n\t\t\t}\r\n\t\t},{\r\n\t\t\t\"pensionProviderSchemeName\": \"MNC Organization PDPViewDataPayload\",\r\n\t\t\t\"alternateSchemeName\": {\r\n\t\t\t\t\"name\": \"Moved from PDPViewDataPayload\",\r\n\t\t\t\t\"alternateNameType\": \"FOR\"\r\n\t\t\t},\r\n\t\t\t\"possibleMatch\": true,\r\n\t\t\t\"possibleMatchReference\": \"W2222222\",\r\n\t\t\t\"pensionAdministrator\": {\r\n\t\t\t\t\"name\": \"Pension Company 3\",\r\n\t\t\t\t\"contactMethods\": [\r\n\t\t\t\t\t{\r\n\t\t\t\t\t\t\"preferred\": false,\r\n\t\t\t\t\t\t\"contactMethodDetails\": {\r\n\t\t\t\t\t\t\t\"email\": \"XYZPDPMainexample123@myneworg.com\"\r\n\t\t\t\t\t\t}\r\n\t\t\t\t\t},\r\n\t\t\t\t\t{\r\n\t\t\t\t\t\t\"preferred\": true,\r\n\t\t\t\t\t\t\"contactMethodDetails\": {\r\n\t\t\t\t\t\t\t\"email\": \"PDPViewDataPayload9999@myline.com\"\r\n\t\t\t\t\t\t}\r\n\t\t\t\t\t},\r\n\t\t\t\t\t{\r\n\t\t\t\t\t\t\"preferred\": true,\r\n\t\t\t\t\t\t\"contactMethodDetails\": {\r\n\t\t\t\t\t\t\t\"number\": \"+123 9999999\",\r\n\t\t\t\t\t\t\t\"usage\": [\r\n\t\t\t\t\t\t\t\t\"S\",\r\n\t\t\t\t\t\t\t\t\"W\"\r\n\t\t\t\t\t\t\t]\r\n\t\t\t\t\t\t}\r\n\t\t\t\t\t},{\r\n\t\t\t\t\t\t\"preferred\": true,\r\n\t\t\t\t\t\t\"contactMethodDetails\": {\r\n\t\t\t\t\t\t\t\"number\": \"\",\r\n\t\t\t\t\t\t\t\"usage\": [\r\n\t\t\t\t\t\t\t\t\"M\",\r\n\t\t\t\t\t\t\t\t\"W\"\r\n\t\t\t\t\t\t\t]\r\n\t\t\t\t\t\t}\r\n\t\t\t\t\t}\r\n\t\t\t\t]\r\n\t\t\t}\r\n\t\t},{\r\n\t\t\t\"pensionProviderSchemeName\": \"My Oorganization MultiValueViewDataPayload\",\r\n\t\t\t\"alternateSchemeName\": {\r\n\t\t\t\t\"name\": \"Diverted from MultiValueViewDataPayload\",\r\n\t\t\t\t\"alternateNameType\": \"OTH\",\r\n\t\t\t\t\"altNameGroup\": \"OTH\"\r\n\t\t\t},\r\n\t\t\t\"possibleMatch\": true,\r\n\t\t\t\"possibleMatchReference\": \"A90000\",\r\n\t\t\t\"pensionAdministrator\": {\r\n\t\t\t\t\"name\": \"Pension Company 3\",\r\n\t\t\t\t\"contactMethods\": [\r\n\t\t\t\t\t{\r\n\t\t\t\t\t\t\"preferred\": false,\r\n\t\t\t\t\t\t\"contactMethodDetails\": {\r\n\t\t\t\t\t\t\t\"email\": \"MultiValueViewDataPayload@examplemyline.com\"\r\n\t\t\t\t\t\t}\r\n\t\t\t\t\t},\r\n\t\t\t\t\t{\r\n\t\t\t\t\t\t\"preferred\": true,\r\n\t\t\t\t\t\t\"contactMethodDetails\": {\r\n\t\t\t\t\t\t\t\"email\": \"WorldPDPViewDataPayload@worldmyline.com\"\r\n\t\t\t\t\t\t}\r\n\t\t\t\t\t},\r\n\t\t\t\t\t{\r\n\t\t\t\t\t\t\"preferred\": true,\r\n\t\t\t\t\t\t\"contactMethodDetails\": {\r\n\t\t\t\t\t\t\t\"number\": \"+123 669999\",\r\n\t\t\t\t\t\t\t\"usage\": [\r\n\t\t\t\t\t\t\t\t\"S\",\r\n\t\t\t\t\t\t\t\t\"A\"\r\n\t\t\t\t\t\t\t]\r\n\t\t\t\t\t\t}\r\n\t\t\t\t\t}\r\n\t\t\t\t]\r\n\t\t\t}\r\n\t\t},{\r\n\t\t\t\"pensionProviderSchemeName\": \"Multination Organization PDPViewDataPayload\",\r\n\t\t\t\"alternateSchemeName\": {\r\n\t\t\t\t\"name\": \"New Diverted from PDPViewDataPayload\",\r\n\t\t\t\t\"alternateNameType\": \"FOR\"\r\n\t\t\t},\r\n\t\t\t\"possibleMatch\": true,\r\n\t\t\t\"possibleMatchReference\": \"Z9123349\",\r\n\t\t\t\"pensionAdministrator\": {\r\n\t\t\t\t\"name\": \"TOP Pension Company 5\",\r\n\t\t\t\t\"contactMethods\": [\r\n\t\t\t\t\t{\r\n\t\t\t\t\t\t\"preferred\": false,\r\n\t\t\t\t\t\t\"contactMethodDetails\": {\r\n\t\t\t\t\t\t\t\"email\": \"ACCPDPMainexample56789@examplemyline.com\"\r\n\t\t\t\t\t\t}\r\n\t\t\t\t\t},\r\n\t\t\t\t\t{\r\n\t\t\t\t\t\t\"preferred\": true,\r\n\t\t\t\t\t\t\"contactMethodDetails\": {\r\n\t\t\t\t\t\t\t\"email\": \"ABCPDPViewDataPayload88888@myline.com\"\r\n\t\t\t\t\t\t}\r\n\t\t\t\t\t},\r\n\t\t\t\t\t{\r\n\t\t\t\t\t\t\"preferred\": true,\r\n\t\t\t\t\t\t\"contactMethodDetails\": {\r\n\t\t\t\t\t\t\t\"number\": \"+123 8888888\",\r\n\t\t\t\t\t\t\t\"usage\": [\r\n\t\t\t\t\t\t\t\t\"S\",\r\n\t\t\t\t\t\t\t\t\"W\"\r\n\t\t\t\t\t\t\t]\r\n\t\t\t\t\t\t}\r\n\t\t\t\t\t},{\r\n\t\t\t\t\t\t\"preferred\": true,\r\n\t\t\t\t\t\t\"contactMethodDetails\": {\r\n\t\t\t\t\t\t\t\"number\": \"+123 3333333\",\r\n\t\t\t\t\t\t\t\"usage\": [\r\n\t\t\t\t\t\t\t\t\"S\",\r\n\t\t\t\t\t\t\t\t\"A\"\r\n\t\t\t\t\t\t\t]\r\n\t\t\t\t\t\t}\r\n\t\t\t\t\t}\r\n\t\t\t\t]\r\n\t\t\t}\r\n\t\t}\r\n\t]\r\n}";
        }

    }
}