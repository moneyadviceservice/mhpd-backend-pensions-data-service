using System.Text.Json;
using System.Text.Json.Nodes;

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
            var pensionProviderSchemeName = "My Company Direct Contribution Scheme";
            JsonObject pdpPensionArrangementsJson = JsonSerializer.Deserialize<JsonObject>(pdpPensionArrangements)!;
            var pdpPensionArrangementsString = JsonSerializer.Serialize<JsonObject>(pdpPensionArrangementsJson)!;

            // Act
            var transformer = new PensionRequestFunction.Transformer.ViewDataToPensionArrangementTransformer();
            var actual = transformer.Transform(externalAssetId, pdpPensionArrangementsString);

            //extract root of document
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
        public void WhenViewDataEmptyArrangementsPresent_ThenThrowError()
        {
            var externalAssetId = "459566f6-5fce-479e-a098-298ca9676a85";
            var pdpPensionArrangements = "{\"view_data\":" + GetEmptyViewDataPayload() + "}";
            JsonObject pdpPensionArrangementsJson = JsonSerializer.Deserialize<JsonObject>(pdpPensionArrangements)!;
            var pdpPensionArrangementsString = JsonSerializer.Serialize<JsonObject>(pdpPensionArrangementsJson)!;
            var transformer = new PensionRequestFunction.Transformer.ViewDataToPensionArrangementTransformer();
            var ex = Assert.Throws<Exception>(() => transformer.Transform(externalAssetId, pdpPensionArrangementsString)); ;

            Assert.Equal("No arrangements present", ex.Message);
        }
        
        private string GetViewDataPayload()
        {
            return "{\r\n\t\"arrangements\": [\r\n\t\t{\r\n\t\t\t\"pensionProviderSchemeName\": \"My Company Direct Contribution Scheme\",\r\n\t\t\t\"alternateSchemeName\": {\r\n\t\t\t\t\"name\": \"Converted from My Old Direct Contribution Scheme\",\r\n\t\t\t\t\"alternateNameType\": \"FOR\"\r\n\t\t\t},\r\n\t\t\t\"possibleMatch\": true,\r\n\t\t\t\"possibleMatchReference\": \"Q12345\",\r\n\t\t\t\"pensionAdministrator\": {\r\n\t\t\t\t\"name\": \"Pension Company 1\",\r\n\t\t\t\t\"contactMethods\": [\r\n\t\t\t\t\t{\r\n\t\t\t\t\t\t\"preferred\": false,\r\n\t\t\t\t\t\t\"contactMethodDetails\": {\r\n\t\t\t\t\t\t\t\"email\": \"example@examplemyline.com\"\r\n\t\t\t\t\t\t}\r\n\t\t\t\t\t},\r\n\t\t\t\t\t{\r\n\t\t\t\t\t\t\"preferred\": true,\r\n\t\t\t\t\t\t\"contactMethodDetails\": {\r\n\t\t\t\t\t\t\t\"number\": \"+123 1111111111\",\r\n\t\t\t\t\t\t\t\"usage\": [\r\n\t\t\t\t\t\t\t\t\"A\",\r\n\t\t\t\t\t\t\t\t\"M\"\r\n\t\t\t\t\t\t\t]\r\n\t\t\t\t\t\t}\r\n\t\t\t\t\t}\r\n\t\t\t\t]\r\n\t\t\t}\r\n\t\t}\r\n\t]\r\n}";
        }

        private string GetEmptyViewDataPayload()
        {
            return "{\r\n\t\"arrangements\": []\r\n}";
        }
    }
}