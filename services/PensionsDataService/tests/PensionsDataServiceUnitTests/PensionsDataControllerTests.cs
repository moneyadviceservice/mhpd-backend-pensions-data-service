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
    private readonly Mock<IRetrievalRecordFunctionClient> _mockRetrievalRecordFunctionClient;
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
        _mockRetrievalRecordFunctionClient = new Mock<IRetrievalRecordFunctionClient>();
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
            _mockRetrievalRecordFunctionClient.Object,
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
    
    [Fact]
    public async Task GetPensionsDataAsync_WhenUserSessionIdIsMissing_ThenReturnsBadRequest()
    {
        // Arrange
        var requestHeader = new RequestHeaderModel { UserSessionId = null };  // Invalid UserSessionId

        // Act
        var result = await _controller.GetPensionsDataAsync(requestHeader);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(TokenValidationMessages.MissingUserSessionId, badRequestResult.Value);
    }

    [Fact]
    public async Task GetPensionsDataAsync_WhenUserSessionIdIsInvalid_ThenReturnsBadRequest()
    {
        // Arrange
        var requestHeader = new RequestHeaderModel { UserSessionId = "invalid-guid" };  // Invalid UserSessionId

        _mockIdValidator.Setup(v => v.IsValidGuid(It.IsAny<string>())).Returns(false);

        // Act
        var result = await _controller.GetPensionsDataAsync(requestHeader);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(TokenValidationMessages.InvalidUserSessionId, badRequestResult.Value);
    }

    [Fact]
    public async Task GetPensionsDataAsync_WhenRetrievalRecordClientReturnsSuccess_ThenReturnsOkResult()
    {
        // Arrange
        var requestHeader = new RequestHeaderModel { UserSessionId = "123e4567-e89b-12d3-a456-426614174000" };

        // Simulate a successful response from the retrieval record function client
        _mockRetrievalRecordFunctionClient
            .Setup(client => client.GetAsync(It.IsAny<RequestHeaderModel>()))
            .ReturnsAsync(new OkResult());

        _mockIdValidator.Setup(v => v.IsValidGuid(It.IsAny<string>())).Returns(true);

        // Act
        var result = await _controller.GetPensionsDataAsync(requestHeader);

        // Assert
        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task GetPensionsDataAsync_WhenRetrievalRecordClientThrowsHttpRequestException_ThenReturnsHttpRequestException()
    {
        // Arrange
        var requestHeader = new RequestHeaderModel { UserSessionId = "123e4567-e89b-12d3-a456-426614174000" };

        // Simulate an HttpRequestException from the retrieval record function client
        _mockRetrievalRecordFunctionClient
            .Setup(client => client.GetAsync(It.IsAny<RequestHeaderModel>()))
            .ThrowsAsync(new HttpRequestException("Error calling external service"));

        _mockIdValidator.Setup(v => v.IsValidGuid(It.IsAny<string>())).Returns(true);

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(async () => 
            await _controller.GetPensionsDataAsync(requestHeader));
    }

    [Fact]
    public async Task GetPensionsDataAsync_WhenRetrievalRecordClientThrowsException_ThenLogsErrorAndThrowsException()
    {
        // Arrange
        var requestHeader = new RequestHeaderModel { UserSessionId = "123e4567-e89b-12d3-a456-426614174000" };

        // Simulate a generic exception from the retrieval record function client
        _mockRetrievalRecordFunctionClient
            .Setup(client => client.GetAsync(It.IsAny<RequestHeaderModel>()))
            .ThrowsAsync(new Exception("Unexpected error"));

        _mockIdValidator.Setup(v => v.IsValidGuid(It.IsAny<string>())).Returns(true);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(async () =>
            await _controller.GetPensionsDataAsync(requestHeader));
    }
}
