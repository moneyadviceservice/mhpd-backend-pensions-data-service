using CDAService.Models;
using static CDAService.Utils.RSA256TokenUtils;

namespace CDAServiceUnitTests
{
    public class RSA256TokenUtilsUnitTests
    {
        private const string _iss = "MHPD-75b68255-444e-4d5f-bbfe-249c26d69963";
        private const string _userSessionId = "6ee2ec99-70a6-4781-b2d2-da2cc75fd177";
        private const string _role = "owner";
        private readonly RQPTokenManager _tokenManager;

        public RSA256TokenUtilsUnitTests() 
        {
            _tokenManager = new RQPTokenManager(_iss, _userSessionId!);
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
            Assert.True(rqpModel.Subject == $"{_userSessionId}@{_iss}");
            Assert.True(rqpModel.Issuer == _iss);
            Assert.True(!string.IsNullOrEmpty(rqpModel.Audience));
            Assert.True(rqpModel.Role == _role);
        }
    }
}