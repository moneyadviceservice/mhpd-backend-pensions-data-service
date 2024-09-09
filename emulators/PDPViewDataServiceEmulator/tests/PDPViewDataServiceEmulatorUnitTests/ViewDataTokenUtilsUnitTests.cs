using System.Text.Json;
using PDPViewDataServicedEmulator.Mocks;
using PDPViewDataServicedEmulator.Models;
using static PDPViewDataServicedEmulator.Utils.ViewDataTokenUtils;

namespace PDPViewDataServiceEmulatorUnitTests
{
    public class ViewDataTokenUtilsUnitTests
    {
        private const string _kid = "ec1abf89-225b-49c2-ab87-1d425ac70f8d";
        private const string _audience = "https://pdp/ig/token";       
        private const string _subject = "324bqfw348f9q4398h3";   
        private readonly ViewDataTokenManager _tokenManager;
        private readonly string _issuer = "DATA_PROVIDER_1fd1da88-9fb3-461c-a48a-3dba21bfba17";

        public ViewDataTokenUtilsUnitTests()
        {
            _tokenManager = new ViewDataTokenManager(_kid, _audience, _subject, _issuer);
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
            Assert.True(!(tokenModel == null));
            Assert.True(tokenModel.Subject ==_subject);
            Assert.True(tokenModel.Issuer ==_issuer);
            Assert.True(!string.IsNullOrEmpty(tokenModel.Audience));
            Assert.True(tokenModel.Audience == _audience);
            Assert.True(Convert.ToInt64(tokenModel.Expiry) - Convert.ToInt64(tokenModel.IssuedAt) == 60);
        }

    }
}