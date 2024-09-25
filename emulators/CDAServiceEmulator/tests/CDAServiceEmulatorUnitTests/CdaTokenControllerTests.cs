using CDAServiceEmulator.Controllers;
using CDAServiceEmulator.Models.Peis;
using CDAServiceEmulator.Models.Token;
using CDAServiceEmulator.TokenValidation;
using MhpdCommon.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace CDAServiceEmulatorUnitTests;

public class CdaTokenControllerTests
{
    private readonly Mock<ILogger<CdaTokenController>> _mockLogger;
    private readonly Mock<IIdValidator> _mockIdValidator;
    private readonly CdaTokenController _controller;

    public CdaTokenControllerTests()
    {
        _mockLogger = new Mock<ILogger<CdaTokenController>>();
        _mockIdValidator = new Mock<IIdValidator>();

        // Create a list of mocked ITokenRequestValidators
        var mockValidator1 = new Mock<ITokenRequestValidator>();
        var mockValidator2 = new Mock<ITokenRequestValidator>();

        // Create the TokenRequestValidatorPipeline with the mock validators
        var tokenRequestValidators = new TokenRequestValidatorPipeline(new List<ITokenRequestValidator> 
        { 
            mockValidator1.Object, 
            mockValidator2.Object 
        });

        // Create the controller with real TokenRequestValidatorPipeline instance
        _controller = new CdaTokenController(_mockLogger.Object, _mockIdValidator.Object, tokenRequestValidators);
    }

    [Fact]
    public async Task GenerateTokenAsync_InvalidXRequestId_ReturnsBadRequest()
    {
        // Arrange
        var request = new CdaTokenRequestModel();
        var requestHeader = new RequestHeaderModel { XRequestId = null };  // Invalid XRequestId

        _mockIdValidator.Setup(v => v.IsValidGuid(It.IsAny<string>())).Returns(false);

        // Act
        var result = await _controller.GenerateTokenAsync(request, requestHeader);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(TokenValidationMessages.InvalidXRequestId, badRequestResult.Value);
    }

    [Fact]
    public async Task GenerateTokenAsync_ValidationFailure_ReturnsBadRequest()
    {
        // Arrange
        var request = new CdaTokenRequestModel();
        var requestHeader = new RequestHeaderModel { XRequestId = "valid-guid" };

        _mockIdValidator.Setup(v => v.IsValidGuid(It.IsAny<string>())).Returns(true);

        // Set up a mock validator to return a failure
        var mockValidator = new Mock<ITokenRequestValidator>();
        mockValidator.Setup(v => v.Validate(It.IsAny<CdaTokenRequestModel>()))
            .Returns(ValidationResult.Failure(TokenValidationMessages.InvalidGrantType));

        var tokenRequestValidators = new TokenRequestValidatorPipeline(new List<ITokenRequestValidator> { mockValidator.Object });
        var controller = new CdaTokenController(_mockLogger.Object, _mockIdValidator.Object, tokenRequestValidators);

        // Act
        var result = await controller.GenerateTokenAsync(request, requestHeader);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(TokenValidationMessages.InvalidGrantType, badRequestResult.Value);
    }

    [Fact]
    public async Task GenerateTokenAsync_ValidRequest_ReturnsOk()
    {
        // Arrange
        var request = new CdaTokenRequestModel();
        var requestHeader = new RequestHeaderModel { XRequestId = "valid-guid" };

        _mockIdValidator.Setup(v => v.IsValidGuid(It.IsAny<string>())).Returns(true);

        // Set up a mock validator to return success
        var mockValidator = new Mock<ITokenRequestValidator>();
        mockValidator.Setup(v => v.Validate(It.IsAny<CdaTokenRequestModel>()))
            .Returns(ValidationResult.Success());

        var tokenRequestValidators = new TokenRequestValidatorPipeline(new List<ITokenRequestValidator> { mockValidator.Object });
        var controller = new CdaTokenController(_mockLogger.Object, _mockIdValidator.Object, tokenRequestValidators);

        // Act
        var result = await controller.GenerateTokenAsync(request, requestHeader);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<CdaTokenResponseModel>(okResult.Value);
        Assert.NotNull(response.AccessToken);
        Assert.Equal(TokenQueryParams.PensionDashboardRqp, response.TokenType);
    }
}