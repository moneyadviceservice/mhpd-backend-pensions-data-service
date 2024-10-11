using MaPSCDAService.Models;
using MaPSCDAService.Utils;
using Microsoft.Extensions.Configuration;

namespace MaPSCDAServiceUnitTests
{
    public class RSA256TokenUtilsUnitTests
    {
        private const string UserSessionId = TestConstants.UserSessionId;
        private const string Issuer = TestConstants.Iss;
        private readonly IRqpTokenManager _tokenManager;
        private readonly IConfiguration _configuration;
        
        public RSA256TokenUtilsUnitTests()
        {

            _configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();
            _configuration["Kid"] = TestConstants.Kid;
            _configuration["Audience"] = TestConstants.Audience; 
            _configuration["privateKey"] = TestConstants.PrivateRsaKey;
            
            _tokenManager = new RqpTokenManager(_configuration);
        }       

        [Fact]
        public void GivenATokenManager_WhenTokenIsGenerated_ThenItReturns_CorrectToken()
        {
            //Act
            var token = _tokenManager.GenerateToken(UserSessionId, Issuer);

            // Assert
            Assert.True(!string.IsNullOrEmpty(token));
        }

        [Fact]
        public void GivenATokenManager_WhenTokenIsGeneratedAndValidated_ThenItValidatesTheToken()
        {
            // Arrange
            var token = _tokenManager.GenerateToken(UserSessionId, Issuer);
    
            // Act
            RQPModel rqpModel;  
            var result = _tokenManager.ValidateToken(token, Issuer, out rqpModel);  

            // Assert
            Assert.True(!string.IsNullOrEmpty(token));  
            
            Assert.NotNull(rqpModel);  
            Assert.Equal($"{UserSessionId}@{Issuer}", rqpModel.Subject);
            Assert.Equal(TestConstants.Audience, rqpModel.Audience);
            Assert.Equal(TestConstants.Role, rqpModel.Role);  
            Assert.True(result);  
        }
    }
}