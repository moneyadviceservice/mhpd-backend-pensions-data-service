using System.Text.Json;
using Newtonsoft.Json.Linq;
using PDPViewDataServicedEmulator.Mocks;
using PDPViewDataServicedEmulator.Models;
using static PDPViewDataServicedEmulator.Utils.ViewDataTokenUtils;

namespace PDPViewDataServiceEmulatorUnitTests
{
    public class ViewDataTokenUtilsUnitTests
    {
        private const string Kid = "ec1abf89-225b-49c2-ab87-1d425ac70f8d";
        private const string Audience = "https://pdp/ig/token";       
        private const string Subject = "324bqfw348f9q4398h3";   
        private readonly ViewDataTokenManager _tokenManager;
        private const string Issuer = "DATA_PROVIDER_1fd1da88-9fb3-461c-a48a-3dba21bfba17";

        private readonly ViewDataPayload _viewDataModelObject = new()
        {
            AssetGuid = "1ba03e25-659a-43b8-ae77-b956df168969",
            ViewData = JObject.Parse("{\r\n\t\"arrangements\": [\r\n\t\t{\r\n\t\t\t\"pensionProviderSchemeName\": \"My Company Direct Contribution Scheme\",\r\n\t\t\t\"alternateSchemeName\": {\r\n\t\t\t\t\"name\": \"Converted from My Old Direct Contribution Scheme\",\r\n\t\t\t\t\"alternateNameType\": \"FOR\"\r\n\t\t\t},\r\n\t\t\t\"possibleMatch\": true,\r\n\t\t\t\"possibleMatchReference\": \"Q12345\",\r\n\t\t\t\"pensionAdministrator\": {\r\n\t\t\t\t\"name\": \"Pension Company 1\",\r\n\t\t\t\t\"contactMethods\": [\r\n\t\t\t\t\t{\r\n\t\t\t\t\t\t\"preferred\": false,\r\n\t\t\t\t\t\t\"contactMethodDetails\": {\r\n\t\t\t\t\t\t\t\"email\": \"example@examplemyline.com\"\r\n\t\t\t\t\t\t}\r\n\t\t\t\t\t},\r\n\t\t\t\t\t{\r\n\t\t\t\t\t\t\"preferred\": true,\r\n\t\t\t\t\t\t\"contactMethodDetails\": {\r\n\t\t\t\t\t\t\t\"number\": \"+123 1111111111\",\r\n\t\t\t\t\t\t\t\"usage\": [\r\n\t\t\t\t\t\t\t\t\"A\",\r\n\t\t\t\t\t\t\t\t\"M\"\r\n\t\t\t\t\t\t\t]\r\n\t\t\t\t\t\t}\r\n\t\t\t\t\t}\r\n\t\t\t\t]\r\n\t\t\t}\r\n\t\t}\r\n\t]\r\n}")
        };
        
        public ViewDataTokenUtilsUnitTests()
        {
            _tokenManager = new ViewDataTokenManager(Kid, Audience, Subject, _viewDataModelObject, Issuer);
        }

        [Fact]
        public void GivenATokenManager_WhenTokenIsGenerated_ThenItReturns_TokenValue()
        {
            //Act
            var token = _tokenManager.GenerateToken();

            // Assert
            Assert.True(!string.IsNullOrEmpty(token));
        }

        [Fact]
        public void GivenATokenManager_WhenTokenIsGeneratedAndValidated_ThenItValidatesTheToken()
        {
            // Act
            var token = _tokenManager.GenerateToken();
            var result = _tokenManager.ValidateToken(token, out TokenModel tokenModel);
            
            // Assert
            Assert.True(!string.IsNullOrEmpty(token));
            Assert.True(result == true);
            Assert.True(tokenModel != null);
            Assert.True(tokenModel.Subject ==Subject);
            Assert.True(tokenModel.Issuer ==Issuer);
            Assert.True(!string.IsNullOrEmpty(tokenModel.Audience));
            Assert.True(tokenModel.Audience == Audience);
            Assert.True(Convert.ToInt64(tokenModel.Expiry) - Convert.ToInt64(tokenModel.IssuedAt) == 60);
        }

    }
}