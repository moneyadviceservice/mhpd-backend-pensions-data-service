using System.Net;
using MhpdCommon.Models.Configuration;
using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.Models.RequestHeaderModel;
using MhpdCommon.TokenValidation;
using MhpdCommon.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using PensionsDataService.Controllers;
using PensionsDataService.HttpClients;
using PensionsDataService.Models;

namespace PensionsDataServiceUnitTests;

public class PensionsDataControllerTests
{
    private readonly Mock<IIdValidator> _mockIdValidator;
    private readonly Mock<ITokenIntegrationServiceClient> _mockTokenIntegrationServiceClient;
    private readonly PensionsDataController _controller;
    private readonly RequestHeaderModel _validRequestHeader = new()
    {
        Iss = "valid-iss", 
        UserSessionId = "123e4567-e89b-12d3-a456-426614174000"
    };

    public PensionsDataControllerTests()
    {
        Mock<ILogger<PensionsDataController>> mockLogger = new();
        _mockIdValidator = new Mock<IIdValidator>();
        _mockTokenIntegrationServiceClient = new Mock<ITokenIntegrationServiceClient>();
        Mock<IMessagingService> mockMessagingService = new();
        Mock<IOptions<CommonServiceBusConfiguration>> mockServiceBusOptions = new();

        // Set up the CommonServiceBusConfiguration with your required values
        var serviceBusConfig = new CommonServiceBusConfiguration
        {
            InboundQueue = "",
            OutboundQueue = "mhpd-pensions-retrieval-job-sb-queue-dev"
        };

        // Return this configuration when accessing the Value property
        mockServiceBusOptions.Setup(s => s.Value).Returns(serviceBusConfig);


        // Get ordered validators
        var validators = Helper.GetOrderedValidators();

        // Create the PensionsDataRequestValidatorPipeline with the mock validators
        Mock<PensionsDataRequestValidatorPipeline> mockValidatorPipeline = new(validators);

        _controller = new PensionsDataController(
            mockLogger.Object, 
            _mockIdValidator.Object, 
            mockValidatorPipeline.Object, 
            _mockTokenIntegrationServiceClient.Object, 
            mockServiceBusOptions.Object, 
            mockMessagingService.Object
        );
    }

    [Fact]
    public async Task PostPensionsDataAsync_WhenIssIsMissing_ThenReturnsBadRequest()
    {
        // Arrange
        var request = new PensionsDataRequestModel();
        var requestHeader = new RequestHeaderModel { Iss = null };  // Invalid Iss

        _mockIdValidator.Setup(v => v.IsValidGuid(It.IsAny<string>())).Returns(false);

        // Act
        var result = await _controller.PostPensionsDataAsync(request, requestHeader);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(TokenValidationMessages.MissingIss, badRequestResult.Value);
    }

    [Fact]
    public async Task PostPensionsDataAsync_WhenUserSessionIdIsMissing_ThenReturnsBadRequest()
    {
        // Arrange
        var request = new PensionsDataRequestModel();
        var requestHeader = new RequestHeaderModel { Iss = "valid-iss", UserSessionId = null };  // Invalid UserSessionId

        _mockIdValidator.Setup(v => v.IsValidGuid(It.IsAny<string>())).Returns(false);

        // Act
        var result = await _controller.PostPensionsDataAsync(request, requestHeader);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(TokenValidationMessages.MissingUserSessionId, badRequestResult.Value);
    }

    [Fact]
    public async Task PostPensionsDataAsync_WhenUserSessionIdIsInvalid_ThenReturnsBadRequest()
    {
        // Arrange
        var request = new PensionsDataRequestModel();
        var requestHeader = new RequestHeaderModel { Iss = "valid-iss", UserSessionId = "132" };  // Invalid UserSessionId

        _mockIdValidator.Setup(v => v.IsValidGuid(It.IsAny<string>())).Returns(false);

        // Act
        var result = await _controller.PostPensionsDataAsync(request, requestHeader);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(TokenValidationMessages.InvalidUserSessionId, badRequestResult.Value);
    }

    [Fact]
    public async Task PostPensionsDataAsync_WhenAuthorisationCodeIsMissing_ThenReturnsBadRequest()
    {
        // Arrange
        var request = new PensionsDataRequestModel
        {
            AuthorisationCode = string.Empty
        };

        _mockIdValidator.Setup(v => v.IsValidGuid(It.IsAny<string>())).Returns(true);

        // Act
        var result = await _controller.PostPensionsDataAsync(request, _validRequestHeader);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(TokenValidationMessages.MissingAuthorisationCode, badRequestResult.Value);
    }

    [Fact]
    public async Task PostPensionsDataAsync_WhenRedirectUriIsMissing_ThenReturnsBadRequest()
    {
        // Arrange
        var request = new PensionsDataRequestModel
        {
            AuthorisationCode = "123e4567-e89b-12d3-a456-426614174000"
        };

        _mockIdValidator.Setup(v => v.IsValidGuid(It.IsAny<string>())).Returns(true);

        // Set up a mock validator to return a failure
        var mockValidator = new Mock<ITokenRequestValidator<PensionsDataRequestModel>>();
        mockValidator.Setup(v => v.Validate(It.IsAny<PensionsDataRequestModel>()))
            .Returns(ValidationResult.Failure(TokenValidationMessages.RedirectUriNotPresent));

        // Act
        var result = await _controller.PostPensionsDataAsync(request, _validRequestHeader);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(TokenValidationMessages.RedirectUriNotPresent, badRequestResult.Value);
    }

    [Fact]
    public async Task PostPensionsDataAsync_WhenCodeVerifierIsMissing_ThenReturnsBadRequest()
    {
        // Arrange
        var request = new PensionsDataRequestModel
        {
            AuthorisationCode = "123e4567-e89b-12d3-a456-426614174000",
            RedirectUri = "https://example.com"
        };

        _mockIdValidator.Setup(v => v.IsValidGuid(It.IsAny<string>())).Returns(true);

        // Set up a mock validator to return a failure
        var mockValidator = new Mock<ITokenRequestValidator<PensionsDataRequestModel>>();
        mockValidator.Setup(v => v.Validate(It.IsAny<PensionsDataRequestModel>()))
            .Returns(ValidationResult.Failure(TokenValidationMessages.CodeVerifierNotPresent));

        // Act
        var result = await _controller.PostPensionsDataAsync(request, _validRequestHeader);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(TokenValidationMessages.CodeVerifierNotPresent, badRequestResult.Value);
    }

    [Fact]
    public async Task PostPensionsDataAsync_WhenRequestIsValid_ThenReturnsNoContent()
    {
        // Arrange
        var request = new PensionsDataRequestModel
        {
            AuthorisationCode = TokenQueryParams.ValidCode,
            RedirectUri = "https://example.com",
            CodeVerifier = TokenQueryParams.ValidCodeVerifier
        };

        _mockIdValidator.Setup(v => v.IsValidGuid(It.IsAny<string>())).Returns(true);

        // Set up a mock validator to return a success
        var mockValidator = new Mock<ITokenRequestValidator<PensionsDataRequestModel>>();
        mockValidator.Setup(v => v.Validate(It.IsAny<PensionsDataRequestModel>()))
            .Returns(ValidationResult.Success);

        // Mock the token integration service client to simulate a successful response
        _mockTokenIntegrationServiceClient.Setup(client => client.PostAsync(It.IsAny<CdaTokenRequestModel>(), It.IsAny<RequestHeaderModel>()))
            .ReturnsAsync(new PeiRetrievalDetailsResponseModel());

        // Act
        var result = await _controller.PostPensionsDataAsync(request, _validRequestHeader);

        // Assert
        var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal((int)HttpStatusCode.NoContent, statusCodeResult.StatusCode);
    }
}
