using PDPViewDataServicedEmulator.Mocks;
using static PDPViewDataServicedEmulator.Utils.ViewDataTokenUtils;

namespace PDPViewDataServiceEmulatorUnitTests
{
    public class ViewDataTokenUtilsUnitTests
    {
        private const string _kid = "ec1abf89-225b-49c2-ab87-1d425ac70f8d";
        private const string _audience = "https://pdp/ig/token";       
        private const string _subject = "324bqfw348f9q4398h3";   
        private readonly ViewDataTokenManager _tokenManager;
        private readonly string _issuer = "Provider1-75b68255-444e-4d5f-bbfe-249c26d69963";

        ViewDataPayload viewDataModelObject = new ViewDataPayload
        {
            AssetGuid = "1ba03e25-659a-43b8-ae77-b956df168969",
            ViewData = "{\r\n\t\"arrangements\": [\r\n\t\t{\r\n\t\t\t\"pensionProviderSchemeName\": \"My Company Direct Contribution Scheme\",\r\n\t\t\t\"alternateSchemeName\": {\r\n\t\t\t\t\"name\": \"Converted from My Old Direct Contribution Scheme\",\r\n\t\t\t\t\"alternateNameType\": \"FOR\"\r\n\t\t\t},\r\n\t\t\t\"possibleMatch\": true,\r\n\t\t\t\"possibleMatchReference\": \"Q12345\",\r\n\t\t\t\"pensionAdministrator\": {\r\n\t\t\t\t\"name\": \"Pension Company 1\",\r\n\t\t\t\t\"contactMethods\": [\r\n\t\t\t\t\t{\r\n\t\t\t\t\t\t\"preferred\": false,\r\n\t\t\t\t\t\t\"contactMethodDetails\": {\r\n\t\t\t\t\t\t\t\"email\": \"example@examplemyline.com\"\r\n\t\t\t\t\t\t}\r\n\t\t\t\t\t},\r\n\t\t\t\t\t{\r\n\t\t\t\t\t\t\"preferred\": true,\r\n\t\t\t\t\t\t\"contactMethodDetails\": {\r\n\t\t\t\t\t\t\t\"number\": \"+123 1111111111\",\r\n\t\t\t\t\t\t\t\"usage\": [\r\n\t\t\t\t\t\t\t\t\"A\",\r\n\t\t\t\t\t\t\t\t\"M\"\r\n\t\t\t\t\t\t\t]\r\n\t\t\t\t\t\t}\r\n\t\t\t\t\t}\r\n\t\t\t\t]\r\n\t\t\t}\r\n\t\t}\r\n\t]\r\n}"
        };
        public ViewDataTokenUtilsUnitTests()
        {
            _tokenManager = new ViewDataTokenManager(_kid, _audience, _subject, viewDataModelObject!, _issuer);
        }

        [Fact]
        public void GivenATokenManager_WhenTokenIsGenerated_ThenItReturns_TokenValue()
        {
            //Act
            var token = _tokenManager.GenerateToken();

            // Assert
            Assert.True(!string.IsNullOrEmpty(token));
        }

    }
}