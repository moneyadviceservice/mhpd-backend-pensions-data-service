using MaPSCDAService;
using MaPSCDAService.Controllers;
using MaPSCDAService.Models;
using MaPSCDAService.Utils;
using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.Models.MHPDModels;
using MhpdCommon.Models.RequestHeaderModel;
using MhpdCommon.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Net;

namespace MaPSCDAServiceUnitTests;

public class MaPSCDAServiceUnitTests
{
    private readonly DefaultHttpContext _httpContext;
    private readonly MapsCdaServiceController _controller;
    private readonly IConfiguration _configuration;
    private readonly Mock<ILogger<MapsCdaServiceController>> _loggerMock;
    private readonly Mock<IPkceGenerator> _pKCEgeneratorMock;
    private readonly Mock<IRqpTokenManager> _rQPTokenManagerMock;
    private readonly Mock<IIdValidator> _idValidator;
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
        _loggerMock = new Mock<ILogger<MapsCdaServiceController>>();
        _pKCEgeneratorMock = new Mock<IPkceGenerator>();
        _pKCEgeneratorMock.Setup(mock => mock.GeneratePkce()).Returns((CodeVerifier, CodeChallenge));
        _rQPTokenManagerMock = new Mock<IRqpTokenManager>();
        _rQPTokenManagerMock.Setup(mock => mock.GenerateToken(TestConstants.UserSessionId, TestConstants.Iss)).Returns(Constants.SampleRqpToken);
        _idValidator = new Mock<IIdValidator>();
        _idValidator.Setup(mock => mock.IsValidGuid(It.IsAny<string>())).Returns(true);
        _controller = new MapsCdaServiceController(configurations, _loggerMock.Object, _pKCEgeneratorMock.Object, _configuration, _rQPTokenManagerMock.Object, _idValidator.Object)
        {
            ControllerContext = new ControllerContext()
            {
                HttpContext = _httpContext
            }
        };
    }

    [Fact]
    public void GivenValidInput_WhenPostIsCalled_ThenReturnsOk()
    {
        // Arrange
        var validRequest = new RPQRequestModel
        {
            Iss = TestConstants.Iss,
            UserSessionId = TestConstants.UserSessionId
        };

        // Act
        var result = _controller.PostRqp(validRequest, GetRequestHeader()) as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.IsType<OkObjectResult>(result);
        var okResult = result as OkObjectResult;
        Assert.NotNull(okResult);
        var response = okResult.Value;
        Assert.NotNull(response);
    }

    [Fact]
    public void GivenValidInput_WhenPostIsCalledAndTokenValidated_ThenTokenValidatesSuccessfully()
    {
        // Arrange
        var validRequest = new RPQRequestModel
        {
            Iss = TestConstants.Iss,
            UserSessionId = TestConstants.UserSessionId123
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
        var result = _controller.PostRqp(validRequest, GetRequestHeader()) as OkObjectResult;
        var generatedToken = ((RQPResponseModel)result!.Value!).Rqp;

        var valid = _rQPTokenManagerMock.Object.ValidateToken(generatedToken!, TestConstants.Iss, out RQPModel validatedRqpModel);

        // Assert
        Assert.NotNull(result);
        Assert.True(valid == true);
        Assert.NotNull(validatedRqpModel); // Ensure the model is not null
        Assert.Contains(validRequest.UserSessionId, validatedRqpModel.Subject);
    }

    [Fact]
    public void GivenInvalidCorrelationId_WhenPostIsCalled_ThenReturnsBadRequest()
    {
        // Arrange
        var validRequest = new RPQRequestModel
        {
            Iss = "Test Iss",
            UserSessionId = Guid.NewGuid().ToString(),
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

        var correlationId = Guid.NewGuid().ToString();
        _idValidator.Setup(mock => mock.IsValidGuid(correlationId)).Returns(false);

        // Act
        var result = _controller.PostRqp(validRequest, GetRequestHeader(correlationId)) as BadRequestObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(Constants.InvalidCorrelationId, result.Value);
    }

    [Fact]
    public void GivenInValidInputs_WhenPostIsCalled_ThenReturnsBadRequest()
    {
        // Arrange
        var request = new RPQRequestModel
        {
            Iss = string.Empty,
            UserSessionId = string.Empty
        };

        // Act
        var result = _controller.PostRqp(request, GetRequestHeader());
        var badResult = (BadRequestObjectResult)result;

        // Assert
        Assert.True(badResult.StatusCode == (int)HttpStatusCode.BadRequest);
        
    }

    [Fact]
    public void GivenInvalidIss_WhenPostIsCalled_ThenReturnsBadRequest()
    {
        // Arrange
        var request = new RPQRequestModel
        {
            Iss = string.Empty,
            UserSessionId = TestConstants.UserSessionId123
        };

        // Act
        var result = _controller.PostRqp(request, GetRequestHeader());
        var badResult = (BadRequestObjectResult)result;

        // Assert
        Assert.True(badResult.StatusCode == (int)HttpStatusCode.BadRequest);
        
    }

    [Fact]
    public void GivenInvalidUserSessionId_WhenPostIsCalled_ThenReturnsBadRequest()
    {
        // Arrange
        var request = new RPQRequestModel
        {
            Iss =TestConstants.Iss,
            UserSessionId = string.Empty
        };

        _idValidator.Setup(mock => mock.IsValidGuid(request.UserSessionId)).Returns(false);

        // Act
        var result = _controller.PostRqp(request, GetRequestHeader());
        var badResult = (BadRequestObjectResult)result;

        // Assert
        Assert.True(badResult.StatusCode == (int)HttpStatusCode.BadRequest);
        
    }

    [Fact]
    public void RedirectDetailsAsync_ValidRequest_ReturnsOkResultWithExpectedResponse()
    {
        // Arrange
        var requestPayload = new RedirectRequestPayload
        {
            RedirectPurpose = Constants.RedirectPurpose,
            Iss = TestConstants.Iss,
            UserSessionId = TestConstants.UserSessionId                   
        };

        // Act
        var result = _controller.RedirectDetails(requestPayload, GetRequestHeader()) as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.IsType<RedirectResponseModel>(result.Value);

        var response = result.Value as RedirectResponseModel;
        Assert.Equal(_redirectTargetUrl, response?.RedirectTargetUrl);
        Assert.Equal(Constants.SampleRqpToken, response?.Rqp);
        Assert.Equal(CodeVerifier, response?.CodeChallenge);
        Assert.Equal(CodeChallenge, response?.CodeVerifier);
    }

    [Fact]
    public void RedirectDetailsAsync_InvalidCorrelationId_ReturnsBadRequest()
    {
        // Arrange
        var requestPayload = new RedirectRequestPayload
        {
            RedirectPurpose = Constants.RedirectPurpose,
            Iss = "sample iss",
            UserSessionId = Guid.NewGuid().ToString()
        };

        var correlationId = Guid.NewGuid().ToString();
        _idValidator.Setup(mock => mock.IsValidGuid(correlationId)).Returns(false);

        // Act
        var result = _controller.RedirectDetails(requestPayload, GetRequestHeader(correlationId)) as BadRequestObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(Constants.InvalidCorrelationId, result.Value);
    }

    [Fact]
    public void RedirectDetailsAsync_MissingOrInvalidIss_ReturnsBadRequest()
    {
        // Arrange
        var requestPayload = new RedirectRequestPayload
        {
            RedirectPurpose = Constants.RedirectPurpose,
            UserSessionId = TestConstants.UserSessionId
        };

        // Act
        var result = _controller.RedirectDetails(requestPayload, GetRequestHeader()) as BadRequestObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(Constants.MissingOrInvalidIss, result.Value);
    }

    [Fact]
    public void RedirectDetailsAsync_MissingOrInvalidUserSessionId_ReturnsBadRequest()
    {
        // Arrange
        var requestPayload = new RedirectRequestPayload
        {
            RedirectPurpose = Constants.RedirectPurpose,
            Iss = TestConstants.Iss
        };

        _idValidator.Setup(mock => mock.IsValidGuid(It.IsAny<string>())).Returns(false);
        _idValidator.Setup(mock => mock.IsValidGuid(It.Is<string>(text => !string.IsNullOrEmpty(text)))).Returns(true);

        // Act
        var result = _controller.RedirectDetails(requestPayload, GetRequestHeader()) as BadRequestObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(Constants.MissingOrInvalidUserSessionId, result.Value);
    }

    [Fact]
    public void RedirectDetailsAsync_MissingOrInvalidRedirectPurpose_ReturnsBadRequest()
    {
        // Arrange
        var requestPayload = new RedirectRequestPayload
        {
            Iss = TestConstants.Iss,
            UserSessionId = TestConstants.UserSessionId
        };

        // Act
        var result = _controller.RedirectDetails(requestPayload, GetRequestHeader()) as BadRequestObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(Constants.MissingOrInvalidRedirectPurpose, result.Value);
    }

    private static RequestHeaderModel GetRequestHeader(string? correlationId = null)
    {
        return new RequestHeaderModel
        {
            CorrelationId = correlationId ?? Guid.NewGuid().ToString()
        };
    }
}
