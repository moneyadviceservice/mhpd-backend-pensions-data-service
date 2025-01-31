using MaPSCDAService.Configuration;
using MaPSCDAService.Controllers;
using MaPSCDAService.Models;
using MaPSCDAService.Utils;
using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.Models.MHPDModels;
using MhpdCommon.Models.RequestHeaderModel;
using MhpdCommon.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace MaPSCDAServiceUnitTests;

public class MapsCdaServiceControllerTests
{
    private readonly Mock<IPkceGenerator> _mockPkceGenerator;
    private readonly Mock<ITokenUtility> _mockTokenUtility;
    private readonly Mock<IIdValidator> _mockIdValidator;
    private readonly MapsCdaServiceController _controller;

    public MapsCdaServiceControllerTests()
    {
        Mock<ILogger<MapsCdaServiceController>> mockLogger = new();
        _mockPkceGenerator = new Mock<IPkceGenerator>();
        _mockTokenUtility = new Mock<ITokenUtility>();
        _mockIdValidator = new Mock<IIdValidator>();
        
        var mockOptions = Options.Create(new UriSettings
        {
            RedirectTargetUrl = "https://mocked-redirect.com"
        });


        _controller = new MapsCdaServiceController(
            mockOptions,
            mockLogger.Object,
            _mockPkceGenerator.Object,
            _mockTokenUtility.Object,
            _mockIdValidator.Object
        );
    }

    [Fact]
    public void PostRqp_ValidRequest_ReturnsOkResponse()
    {
        // Arrange
        var request = new RedirectRequestPayload { Iss = "issuer", UserSessionId = Guid.NewGuid().ToString() };
        var header = new RequestHeaderModel { CorrelationId = Guid.NewGuid().ToString() };

        _mockIdValidator.Setup(v => v.IsValidGuid(request.UserSessionId)).Returns(true);
        _mockIdValidator.Setup(v => v.IsValidGuid(header.CorrelationId)).Returns(true);
        _mockTokenUtility.Setup(t => t.GenerateJwt(It.IsAny<CustomClaimDataModel>())).Returns("mocked_token");

        // Act
        var result = _controller.PostRqp(request, header);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<RqpResponseModel>(okResult.Value);
        Assert.Equal("mocked_token", response.Rqp);
    }

    [Fact]
    public void PostRqp_InvalidRequest_MissingIss_ReturnsBadRequest()
    {
        // Arrange
        var request = new RedirectRequestPayload { Iss = "", UserSessionId = Guid.NewGuid().ToString() };
        var header = new RequestHeaderModel { CorrelationId = Guid.NewGuid().ToString() };

        // Act
        var result = _controller.PostRqp(request, header);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(Constants.MissingOrInvalidIss, badRequestResult.Value);
    }
    
    [Fact]
    public void PostRedirectDetails_InvalidRequest_MissingRedirectPurpose_ReturnsBadRequest()
    {
        // Arrange
        var request = new RedirectRequestPayload { Iss = "", UserSessionId = Guid.NewGuid().ToString() };
        var header = new RequestHeaderModel { CorrelationId = Guid.NewGuid().ToString() };
        
        _mockIdValidator.Setup(v => v.IsValidGuid(request.RedirectPurpose)).Returns(true);

        // Act
        var result = _controller.RedirectDetails(request, header);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(Constants.MissingOrInvalidRedirectPurpose, badRequestResult.Value);
    }

    [Fact]
    public void PostRqp_InvalidRequest_InvalidUserSessionId_ReturnsBadRequest()
    {
        // Arrange
        var request = new RedirectRequestPayload { Iss = "issuer", UserSessionId = "invalid_guid", RedirectPurpose = "FIND"};
        var header = new RequestHeaderModel { CorrelationId = Guid.NewGuid().ToString() };

        _mockIdValidator.Setup(v => v.IsValidGuid(request.RedirectPurpose)).Returns(true);
        _mockIdValidator.Setup(v => v.IsValidGuid(request.UserSessionId)).Returns(false);

        // Act
        var result = _controller.PostRqp(request, header);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(Constants.MissingOrInvalidUserSessionId, badRequestResult.Value);
    }
    
    [Fact]
    public void PostRqp_InvalidRequest_InvalidCorrelationId_ReturnsBadRequest()
    {
        // Arrange
        var request = new RedirectRequestPayload { Iss = "issuer", UserSessionId = Guid.NewGuid().ToString(), RedirectPurpose = "FIND"};
        var header = new RequestHeaderModel { CorrelationId = "invalid_guid" };

        _mockIdValidator.Setup(v => v.IsValidGuid(request.RedirectPurpose)).Returns(true);
        _mockIdValidator.Setup(v => v.IsValidGuid(request.UserSessionId)).Returns(true);

        // Act
        var result = _controller.PostRqp(request, header);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(Constants.InvalidCorrelationId, badRequestResult.Value);
    }

    [Fact]
    public void RedirectDetails_ValidRequest_ReturnsOkResponse()
    {
        // Arrange
        var request = new RedirectRequestPayload { Iss = "issuer", UserSessionId = Guid.NewGuid().ToString(), RedirectPurpose = "FIND"};
        var header = new RequestHeaderModel { CorrelationId = Guid.NewGuid().ToString() };

        _mockIdValidator.Setup(v => v.IsValidGuid(request.RedirectPurpose)).Returns(true);
        _mockIdValidator.Setup(v => v.IsValidGuid(request.UserSessionId)).Returns(true);
        _mockIdValidator.Setup(v => v.IsValidGuid(header.CorrelationId)).Returns(true);
        _mockTokenUtility.Setup(t => t.GenerateJwt(It.IsAny<CustomClaimDataModel>())).Returns("mocked_token");
        _mockPkceGenerator.Setup(p => p.GeneratePkce()).Returns((codeVerifier: "mocked_verifier", codeChallenge: "mocked_challenge"));

        // Act
        var result = _controller.RedirectDetails(request, header);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<RedirectResponseModel>(okResult.Value);
        Assert.Equal("mocked_token", response.Rqp);
        Assert.Equal("mocked_challenge", response.CodeChallenge);
        Assert.Equal("mocked_verifier", response.CodeVerifier);
        Assert.Equal("https://mocked-redirect.com", response.RedirectTargetUrl);
    }
}