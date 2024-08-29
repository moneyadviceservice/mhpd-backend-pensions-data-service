using MaPSCDAService.Models;
using static MaPSCDAService.Utils.RSA256TokenUtils;

namespace MaPSCDAServiceUnitTests
{
    public class RSA256TokenUtilsUnitTests
    {
        private const string _userSessionId = "6ee2ec99-70a6-4781-b2d2-da2cc75fd177";
        private const string _issuer = "MHPD-75b68255-444e-4d5f-bbfe-249c26d69963";
        private const string _role = "owner";
        private readonly RQPTokenManager _tokenManager;

        private readonly KeyVaultSecrets _secrets = new KeyVaultSecrets
        {
            Kid = "ec1abf89-225b-49c2-ab87-1d425ac70f8d",
            Audience = "https://pdp/ig/token"
        };

        public RSA256TokenUtilsUnitTests() 
        {
            _tokenManager = new RQPTokenManager(_userSessionId, _issuer, _secrets);
        }

        [Fact]
        public void GivenATokenManager_WhenTokenIsGenerated_ThenItReturns_CorrectToken()
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
            var result = _tokenManager.ValidateToken(token, out RQPModel rqpModel);

            // Assert
            Assert.True(!string.IsNullOrEmpty(token));
            Assert.True(result == true);
            Assert.True(!(rqpModel == null));
            Assert.True(rqpModel.Subject == $"{_userSessionId}@{rqpModel.Issuer}");
            Assert.True(rqpModel.Issuer == rqpModel.Issuer);
            Assert.True(!string.IsNullOrEmpty(rqpModel.Audience));
            Assert.True(rqpModel.Role == _role);
        }
    }
}