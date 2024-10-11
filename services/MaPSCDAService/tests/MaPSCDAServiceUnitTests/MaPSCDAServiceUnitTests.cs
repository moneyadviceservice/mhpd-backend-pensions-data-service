using System.Net;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using MaPSCDAService;
using MaPSCDAService.Controllers;
using MaPSCDAService.Models;
using MaPSCDAService.Utils;
using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.Models.MHPDModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace MaPSCDAServiceUnitTests
{
    public class MaPSCDAServiceUnitTests
    {
        private readonly DefaultHttpContext _httpContext;
        private readonly MaPSCDAServiceController _controller;
        private readonly IConfiguration _configuration;
        private readonly Mock<ILogger<MaPSCDAServiceController>> _loggerMock;
        private readonly Mock<IPkceGenerator> _pKCEgeneratorMock;
        private readonly Mock<IRqpTokenManager> _rQPTokenManagerMock;
        private readonly string? _redirectTargetUrl = default;
        private const string CodeVerifier = "j3wKnK2Fa_mc2tgdqa6GtUfCYjdWSA5S23JKTTtPF8Y";
        private const string CodeChallenge = "7189b64cc5f65b805baf201e384dc53ae7d18305d5ebb6170ad557b6";
        public MaPSCDAServiceUnitTests()
        {
            
            var redirectTargetUrl = new UriSettings
            {
                RedirectTargetUrl = _redirectTargetUrl,
            };
           
            _configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            _configuration["Kid"] = TestConstants.Kid;
            _configuration["Audience"] = TestConstants.Audience;
            _configuration["privateKey"] = TestConstants.PrivateRsaKey;
            
            var configurations = Options.Create(redirectTargetUrl);
            _httpContext = new DefaultHttpContext();
            _loggerMock = new Mock<ILogger<MaPSCDAServiceController>>();
            _pKCEgeneratorMock = new Mock<IPkceGenerator>();
            _pKCEgeneratorMock.Setup(mock => mock.GeneratePkce()).Returns((CodeVerifier, CodeChallenge));
            _rQPTokenManagerMock = new Mock<IRqpTokenManager>();
            _rQPTokenManagerMock.Setup(mock => mock.GenerateToken(TestConstants.UserSessionId, TestConstants.Iss)).Returns(Constants.SampleRqpToken);
            _controller = new MaPSCDAServiceController(configurations, _loggerMock.Object, _pKCEgeneratorMock.Object, _configuration, _rQPTokenManagerMock.Object)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = _httpContext
                }
            };
        }

        [Fact]
        public async Task GivenValidInput_WhenPostIsCalled_ThenReturnsOk()
        {
            // Arrange
            var validRequest = new RPQRequestModel
            {
                Iss = TestConstants.Iss,
                UserSessionId = TestConstants.UserSessionId
            };

            // Act
            var result = await _controller.PostAsync(validRequest) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.NotNull(okResult);
            var response = okResult.Value;
            Assert.NotNull(response);
        }

        [Fact]
        public async Task GivenValidInput_WhenPostIsCalledAndTokenValidated_ThenTokenValidatesSuccessfully()
        {
            // Arrange
            var validRequest = new RPQRequestModel
            {
                Iss = TestConstants.Iss,
                UserSessionId = TestConstants.UserSessionId123
            };

            var secrets = new KeyVaultSecrets
            {
                Kid = _configuration["Kid"],
                Audience = _configuration["Audience"]
            };

            var rqpModel = new RQPModel 
            {
                Issuer = validRequest.Iss,
                Subject = $"{validRequest.UserSessionId}@{validRequest.Iss}",
                Audience = _configuration[TestConstants.Audience],
                Role = TestConstants.Role
            };
            
            _rQPTokenManagerMock.Setup(mock => mock.ValidateToken(It.IsAny<string>(), It.IsAny<string>(), out rqpModel))
                .Returns(true);

            // Act
            var result = await _controller.PostAsync(validRequest) as OkObjectResult;
            var generatedToken = ((RQPResponseModel)result!.Value!).Rqp;

            var valid = _rQPTokenManagerMock.Object.ValidateToken(generatedToken!, TestConstants.Iss, out RQPModel validatedRqpModel);

            // Assert
            Assert.NotNull(result);
            Assert.True(valid == true);
            Assert.NotNull(validatedRqpModel); // Ensure the model is not null
            Assert.Contains(validRequest.UserSessionId, validatedRqpModel.Subject);
        }

        [Fact]
        public async Task GivenInValidInputs_WhenPostIsCalled_ThenReturnsBadRequest()
        {
            // Arrange
            var request = new RPQRequestModel
            {
                Iss = string.Empty,
                UserSessionId = string.Empty
            };

            // Act
            var result = await _controller.PostAsync(request);
            var badResult = (BadRequestObjectResult)result;

            // Assert
            Assert.True(badResult.StatusCode == (int)HttpStatusCode.BadRequest);
            
        }

        [Fact]
        public async Task GivenInvalidIss_WhenPostIsCalled_ThenReturnsBadRequest()
        {
            // Arrange
            var request = new RPQRequestModel
            {
                Iss = string.Empty,
                UserSessionId = TestConstants.UserSessionId123
            };

            // Act
            var result = await _controller.PostAsync(request);
            var badResult = (BadRequestObjectResult)result;

            // Assert
            Assert.True(badResult.StatusCode == (int)HttpStatusCode.BadRequest);
            
        }

        [Fact]
        public async Task GivenInvalidUserSessionId_WhenPostIsCalled_ThenReturnsBadRequest()
        {
            // Arrange
            var request = new RPQRequestModel
            {
                Iss =TestConstants.Iss,
                UserSessionId = string.Empty
            };

            // Act
            var result = await _controller.PostAsync(request);
            var badResult = (BadRequestObjectResult)result;

            // Assert
            Assert.True(badResult.StatusCode == (int)HttpStatusCode.BadRequest);
            
        }

        [Fact]
        public void RedirectDetailsAsync_ValidRequest_ReturnsOkResultWithExpectedResponse()
        {
            var requestPayload = new RedirectRequestPayload
            {
                RedirectPurpose = Constants.RedirectPurpose,
                Iss = TestConstants.Iss,
                UserSessionId = TestConstants.UserSessionId                   
            };

            var result = _controller.RedirectDetails(requestPayload) as OkObjectResult;

            Assert.NotNull(result);
            Assert.IsType<RedirectResponseModel>(result.Value);

            var response = result.Value as RedirectResponseModel;
            Assert.Equal(_redirectTargetUrl, response?.RedirectTargetUrl);
            Assert.Equal(Constants.SampleRqpToken, response?.Rqp);
            Assert.Equal(CodeVerifier, response?.CodeChallenge);
            Assert.Equal(CodeChallenge, response?.CodeVerifier);
        }

        [Fact]
        public void RedirectDetailsAsync_MissingOrInvalidIss_ReturnsBadRequest()
        {
            var requestPayload = new RedirectRequestPayload
            {
                RedirectPurpose = Constants.RedirectPurpose,
                UserSessionId = TestConstants.UserSessionId
            };

            var result = _controller.RedirectDetails(requestPayload) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(Constants.MissingOrInvalidIss, result.Value);
        }

        [Fact]
        public void RedirectDetailsAsync_MissingOrInvalidUserSessionId_ReturnsBadRequest()
        {
            var requestPayload = new RedirectRequestPayload
            {
                RedirectPurpose = Constants.RedirectPurpose,
                Iss = TestConstants.Iss
            };

            var result = _controller.RedirectDetails(requestPayload) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(Constants.MissingOrInvalidUserSessionId, result.Value);
        }

        [Fact]
        public void RedirectDetailsAsync_MissingOrInvalidRedirectPurpose_ReturnsBadRequest()
        {
            var requestPayload = new RedirectRequestPayload
            {
                Iss = TestConstants.Iss,
                UserSessionId = TestConstants.UserSessionId
            };

            var result = _controller.RedirectDetails(requestPayload) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(Constants.MissingOrInvalidRedirectPurpose, result.Value);
        }
    }
}
