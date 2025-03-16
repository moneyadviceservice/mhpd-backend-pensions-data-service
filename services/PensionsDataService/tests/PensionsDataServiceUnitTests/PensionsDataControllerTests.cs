using System.Net;
using System.Text.Json;
using MhpdCommon.Constants;
using MhpdCommon.Models.Configuration;
using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.Models.MHPDModels;
using MhpdCommon.Models.RequestHeaderModel;
using MhpdCommon.TokenValidation;
using MhpdCommon.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using PensionsDataService;
using PensionsDataService.Controllers;
using PensionsDataService.CosmosRepository;
using PensionsDataService.HttpClients;
using PensionsDataService.Models;

namespace PensionsDataServiceUnitTests;

public class PensionsDataControllerTests
{
    private readonly Mock<IIdValidator> _mockIdValidator;
    private readonly Mock<ITokenIntegrationServiceClient> _mockTokenIntegrationServiceClient;
    private readonly Mock<IRetrievalRecordServiceClient> _mockRetrievalRecordFunctionClient;
    private readonly Mock<IRetrievedPensionsRecordClient> _mockRetrievedPensionsRecordClient;
    private readonly PensionsDataController _controller;
    private readonly Mock<Container> _mockUserSessionDataContainer;
    private const string ValidPeisId = "0001f518264ba44564b186f42af6659b5822eb6e";

    private readonly RequestHeaderModel _validRequestHeader = new()
    {
        Iss = "valid-iss", 
        UserSessionId = "123e4567-e89b-12d3-a456-426614174000",
        CorrelationId = "bc8174f0-5ff8-40a1-aa4b-d15c235dd2c5"
    };

    public PensionsDataControllerTests()
    {
        Mock<ILogger<PensionsDataController>> mockLogger = new();
        _mockIdValidator = new Mock<IIdValidator>();
        Mock<IMessagingService> mockMessagingService = new();
        Mock<IOptions<CommonServiceBusConfiguration>> mockServiceBusOptions = new();
        Mock<IOptions<PeiOrchestrationSettings>> mockOrchestrationSettings = new();
        Mock<UserSessionDataRepository> mockUserSessionDataRepository = new();
        
        Mock<CosmosClient> mockCosmosClient = new();
        _mockUserSessionDataContainer = new();
        Mock<Database> mockDatabase = new();
        
        var configuration = new CosmosBusinessConfiguration
        {
            DatabaseId = "TestDatabase",
            UserSessionDataContainer = "TestUserSessionDataContainer",
        };
        
        Mock<IOptions<CosmosBusinessConfiguration>> mockCosmosConfigOptions = new();
        mockCosmosConfigOptions.Setup(x => x.Value).Returns(configuration);

        mockCosmosClient.Setup(mock => mock.GetDatabase(configuration.DatabaseId))
            .Returns(mockDatabase.Object);

        mockDatabase.Setup(mock => mock.GetContainer(configuration.UserSessionDataContainer))
            .Returns(_mockUserSessionDataContainer.Object);
        
        var mockScenarioModelRepository = new Mock<UserSessionDataRepository>(
            mockCosmosClient.Object,
            configuration.DatabaseId,
            configuration.UserSessionDataContainer
        );

        // Arrange: Set up the mocks for the dependencies
        _mockTokenIntegrationServiceClient = new Mock<ITokenIntegrationServiceClient>();
        _mockRetrievalRecordFunctionClient = new Mock<IRetrievalRecordServiceClient>();
        _mockRetrievedPensionsRecordClient = new Mock<IRetrievedPensionsRecordClient>();

        // Create an instance of PensionServiceClients with mocked dependencies
        Mock<PensionServiceClients> mockServiceClients = new(
            _mockTokenIntegrationServiceClient.Object,
            _mockRetrievalRecordFunctionClient.Object,
            _mockRetrievedPensionsRecordClient.Object
        );

        // Set up the CommonServiceBusConfiguration with your required values
        var serviceBusConfig = new CommonServiceBusConfiguration
        {
            InboundQueue = "",
            OutboundQueue = "mhpd-pensions-retrieval-job-sb-queue-dev"
        };

        var peiOrchestrationSettings = new PeiOrchestrationSettings
        {
            PeiRetrievalDuration = 60,
            ViewDataRetrievalDuration = 10
        };

        // Return this configuration when accessing the Value property
        mockServiceBusOptions.Setup(s => s.Value).Returns(serviceBusConfig);
        mockOrchestrationSettings.Setup(s => s.Value).Returns(peiOrchestrationSettings);

        // Get ordered validators
        var validators = Helper.GetOrderedValidators();

        // Create the PensionsDataRequestValidatorPipeline with the mock validators
        Mock<PensionsDataRequestValidatorPipeline> mockValidatorPipeline = new(validators);

        _controller = new PensionsDataController(
            mockLogger.Object, 
            _mockIdValidator.Object, 
            mockValidatorPipeline.Object, 
            mockServiceClients.Object, 
            mockOrchestrationSettings.Object,
            mockScenarioModelRepository.Object
        );
    }

    [Fact]
    public async Task PostPensionsDataAsync_WhenIssIsMissing_ThenReturnsBadRequest()
    {
        // Arrange
        var request = new PensionsDataRequestModel();
        var id = Guid.NewGuid().ToString();
        var requestHeader = new RequestHeaderModel { Iss = null, UserSessionId = id, CorrelationId = id };  // Invalid Iss

        _mockIdValidator.Setup(v => v.IsValidGuid(It.IsAny<string>())).Returns(false);
        _mockIdValidator.Setup(v => v.IsValidGuid(id)).Returns(true);

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
        var id = Guid.NewGuid().ToString();
        var requestHeader = new RequestHeaderModel { Iss = "valid-iss", UserSessionId = null, CorrelationId = id };  // Invalid UserSessionId

        _mockIdValidator.Setup(v => v.IsValidGuid(It.IsAny<string>())).Returns(false);
        _mockIdValidator.Setup(v => v.IsValidGuid(id)).Returns(true);

        // Act
        var result = await _controller.PostPensionsDataAsync(request, requestHeader);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(TokenValidationMessages.InvalidUserSessionId, badRequestResult.Value);
    }

    [Fact]
    public async Task PostPensionsDataAsync_WhenCorrelationIdInvalid_ThenReturnsBadRequest()
    {
        // Arrange
        var request = new PensionsDataRequestModel();
        var id = Guid.NewGuid().ToString();
        var requestHeader = new RequestHeaderModel { Iss = "valid-iss", UserSessionId = id, CorrelationId = "NotAGuid" };

        _mockIdValidator.Setup(v => v.IsValidGuid(It.IsAny<string>())).Returns(false);
        _mockIdValidator.Setup(v => v.IsValidGuid(id)).Returns(true);

        // Act
        var result = await _controller.PostPensionsDataAsync(request, requestHeader);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(Constants.InvalidCorrelationId, badRequestResult.Value);
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
            RedirectUrl = "https://example.com"
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
            ClientId = TokenQueryParams.ValidClientId,
            ClientSecret = TokenQueryParams.ValidClientSecret,
            AuthorisationCode = TokenQueryParams.ValidCode,
            RedirectUrl = "https://example.com",
            CodeVerifier = TokenQueryParams.ValidCodeVerifier
        };

        _mockIdValidator.Setup(v => v.IsValidGuid(It.IsAny<string>())).Returns(true);

        // Set up a mock validator to return a success
        var mockValidator = new Mock<ITokenRequestValidator<PensionsDataRequestModel>>();
        mockValidator.Setup(v => v.Validate(It.IsAny<PensionsDataRequestModel>()))
            .Returns(ValidationResult.Success);

        // Mock the token integration service client to simulate a successful response
        _mockTokenIntegrationServiceClient.Setup(client => client.PostAsync(It.IsAny<PensionsDataRequestModel>(), It.IsAny<RequestHeaderModel>()))
            .ReturnsAsync(new PeiRetrievalDetailsResponseModel
            {
                PeisId = ValidPeisId
            });

        // Act
        var result = await _controller.PostPensionsDataAsync(request, _validRequestHeader);

        // Assert
        var statusCodeResult = Assert.IsType<AcceptedResult>(result);
        Assert.Equal((int)HttpStatusCode.Accepted, statusCodeResult.StatusCode);
        _mockTokenIntegrationServiceClient.Verify(client => client.PostAsync(It.IsAny<PensionsDataRequestModel>(), 
            It.Is<RequestHeaderModel>(model => model.CorrelationId == _validRequestHeader.CorrelationId)), Times.Once);
    }
    
    [Fact]
    public async Task PostPensionsDataAsync_WhenRequestIsInValid_ThenReturnsNoContent()
    {
        // Arrange
        var request = new PensionsDataRequestModel
        {
            ClientId = TokenQueryParams.ValidClientId,
            ClientSecret = TokenQueryParams.ValidClientSecret,
            AuthorisationCode = TokenQueryParams.ValidCode,
            RedirectUrl = "https://example.com",
            CodeVerifier = TokenQueryParams.ValidCodeVerifier
        };

        _mockIdValidator.Setup(v => v.IsValidGuid(It.IsAny<string>())).Returns(true);

        // Set up a mock validator to return a success
        var mockValidator = new Mock<ITokenRequestValidator<PensionsDataRequestModel>>();
        mockValidator.Setup(v => v.Validate(It.IsAny<PensionsDataRequestModel>()))
            .Returns(ValidationResult.Success);

        // Mock the token integration service client to simulate a successful response
        _mockTokenIntegrationServiceClient.Setup(client => client.PostAsync(It.IsAny<PensionsDataRequestModel>(), It.IsAny<RequestHeaderModel>()))
            .ReturnsAsync(new PeiRetrievalDetailsResponseModel());

        // Act
        var result = await _controller.PostPensionsDataAsync(request, _validRequestHeader);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal((int)HttpStatusCode.InternalServerError, statusCodeResult.StatusCode);
        _mockTokenIntegrationServiceClient.Verify(client => client.PostAsync(It.IsAny<PensionsDataRequestModel>(), 
            It.Is<RequestHeaderModel>(model => model.CorrelationId == _validRequestHeader.CorrelationId)), Times.Once);
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
        Assert.Equal(TokenValidationMessages.InvalidUserSessionId, badRequestResult.Value);
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
            .ReturnsAsync(new PensionsRetrievalRecord());

        _mockIdValidator.Setup(v => v.IsValidGuid(It.IsAny<string>())).Returns(true);

        // Act
        var result = await _controller.GetPensionsDataAsync(requestHeader);

        // Assert
        Assert.IsType<JsonResult>(result);
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
    
    [Fact]
    public async Task GetPensionsDataAsync_WhenPeiDataIsEmpty_ThenReturnsOkWithNullResponse()
    {
        // Arrange
        var requestHeader = new RequestHeaderModel { UserSessionId = "123e4567-e89b-12d3-a456-426614174000" };
        
        // Simulate an empty response from the retrieval record function client
        var retrievalRecord = new PensionsRetrievalRecord
        {
            Id = Guid.NewGuid().ToString(),
            PeiData = [],
            PeiRetrievalComplete = true
        };

        var retrievedRecord = new List<RetrievedPensionRecord>();
        
        _mockRetrievalRecordFunctionClient
            .Setup(client => client.GetAsync(It.IsAny<RequestHeaderModel>()))
            .ReturnsAsync(retrievalRecord);
        
        _mockRetrievedPensionsRecordClient
            .Setup(client => client.GetAsync(It.IsAny<PensionsRetrievalRecordIdModel>(), It.IsAny<RequestHeaderModel>()))
            .ReturnsAsync(retrievedRecord);

        _mockIdValidator.Setup(v => v.IsValidGuid(It.IsAny<string>())).Returns(true);

        // Act
        var result = await _controller.GetPensionsDataAsync(requestHeader);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var responseModel = Assert.IsType<PensionsDataResponseModel>(okResult.Value);
        Assert.NotNull(responseModel.PensionPolicies);
        Assert.Empty(responseModel.PensionPolicies);
    }

    [Fact]
    public async Task GetPensionsDataAsync_WhenRetrievalStatusIsRequested_ThenReturnsOkWithAppropriateResponse()
    {
        // Arrange
        var requestHeader = new RequestHeaderModel 
        { 
            UserSessionId = "123e4567-e89b-12d3-a456-426614174000",
            CorrelationId = "21138f64-4fea-4326-8c32-aca5950ed2b2"
        };
        
        var retrievalRecord = new PensionsRetrievalRecord
        {
            Id = Guid.NewGuid().ToString(),
            PeiData =
            [
                new() { RetrievalStatus = PensionProviderConstants.RetrievalStatus.RetrievalRequested }
            ],
            PeiRetrievalComplete = true
        };
        
        var retrievedRecord = new List<RetrievedPensionRecord>();

        _mockRetrievalRecordFunctionClient
            .Setup(client => client.GetAsync(It.IsAny<RequestHeaderModel>()))
            .ReturnsAsync(retrievalRecord);
        
        _mockRetrievedPensionsRecordClient
            .Setup(client => client.GetAsync(It.IsAny<PensionsRetrievalRecordIdModel>(), It.IsAny<RequestHeaderModel>()))
            .ReturnsAsync(retrievedRecord);

        _mockIdValidator.Setup(v => v.IsValidGuid(It.IsAny<string>())).Returns(true);

        // Act
        var result = await _controller.GetPensionsDataAsync(requestHeader);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var responseModel = Assert.IsType<PensionsDataResponseModel>(okResult.Value);
        Assert.NotNull(responseModel.PensionPolicies);
        Assert.Empty(responseModel.PensionPolicies);
        _mockRetrievalRecordFunctionClient.Verify(client => client.GetAsync(
            It.Is<RequestHeaderModel>(model => model.CorrelationId == requestHeader.CorrelationId)), Times.Once);
        _mockRetrievedPensionsRecordClient
            .Verify(client => client.GetAsync(It.IsAny<PensionsRetrievalRecordIdModel>(),
                It.Is<RequestHeaderModel>(model => model.CorrelationId == requestHeader.CorrelationId)), Times.Once);
    }
    
    [Fact]
    public async Task GetPensionsDataAsync_WhenRetrievalStatusIsRequested_And_NoUserSessionIdFound_ThenReturnsOkJsonObjectWithNullResponse()
    {
        // Arrange
        var requestHeader = new RequestHeaderModel { UserSessionId = "123e4567-e89b-12d3-a456-426614174000" };
        
        var retrievalRecord = new PensionsRetrievalRecord();
        
        var retrievedRecord = new List<RetrievedPensionRecord>();

        _mockRetrievalRecordFunctionClient
            .Setup(client => client.GetAsync(It.IsAny<RequestHeaderModel>()))
            .ReturnsAsync(retrievalRecord);
        
        _mockRetrievedPensionsRecordClient
            .Setup(client => client.GetAsync(It.IsAny<PensionsRetrievalRecordIdModel>(), requestHeader))
            .ReturnsAsync(retrievedRecord);

        _mockIdValidator.Setup(v => v.IsValidGuid(It.IsAny<string>())).Returns(true);

        // Act
        var result = await _controller.GetPensionsDataAsync(requestHeader);

        // Assert
        var okResult = Assert.IsType<JsonResult>(result);
        Assert.Null(okResult.Value);
    }
    
    [Fact]
    public async Task GetPensionsDataAsync_WhenRetrievalRequested_ThenReturnsOkWithMashedData()
    {
        // Arrange
        var requestHeader = new RequestHeaderModel { UserSessionId = "123e4567-e89b-12d3-a456-426614174000" };
        
        var retrievalRecord = new PensionsRetrievalRecord
        {
            Id = Guid.NewGuid().ToString(),
            JobStartTimestamp = DateTime.UtcNow,
            PeiData =
            [
                new() { RetrievalStatus = PensionProviderConstants.RetrievalStatus.RetrievalRequested }
            ],
            PeiRetrievalComplete = false
        };
        
        var retrievedRecord = new List<RetrievedPensionRecord>
        {
            new()
            {
                RetrievalResult = JsonSerializer.Deserialize<JsonElement>("[{\"externalPensionPolicyId\": \"D9267759822\"}]")
            }
        };

        _mockRetrievalRecordFunctionClient
            .Setup(client => client.GetAsync(It.IsAny<RequestHeaderModel>()))
            .ReturnsAsync(retrievalRecord);
        
        _mockRetrievedPensionsRecordClient
            .Setup(client => client.GetAsync(It.IsAny<PensionsRetrievalRecordIdModel>(), It.IsAny<RequestHeaderModel>()))
            .ReturnsAsync(retrievedRecord);

        _mockIdValidator.Setup(v => v.IsValidGuid(It.IsAny<string>())).Returns(true);

        // Act
        var result = await _controller.GetPensionsDataAsync(requestHeader);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var responseModel = Assert.IsType<PensionsDataResponseModel>(okResult.Value);

        // Verify the mashed data output
        Assert.NotNull(responseModel.PensionPolicies);
        Assert.Single(responseModel.PensionPolicies); // Ensure one pension policy was created
        Assert.Equal("D9267759822", responseModel.PensionPolicies[0].PensionArrangements?[0].GetProperty("externalPensionPolicyId").GetString());
    }
    
    [Fact]
    public async Task GetPensionsDataAsync_WhenMultipleExternalPensionPolicyIds_ThenReturnsGroupedMashedData()
    {
        // Arrange
        var requestHeader = new RequestHeaderModel { UserSessionId = "123e4567-e89b-12d3-a456-426614174000" };
        
        var retrievalRecord = new PensionsRetrievalRecord
        {
            Id = Guid.NewGuid().ToString(),
            PeiData = [new PeiDataModel { RetrievalStatus = PensionProviderConstants.RetrievalStatus.RetrievalRequested }],
            PeiRetrievalComplete = true
        };
        
        var retrievedRecord = new List<RetrievedPensionRecord>
        {
            new() { RetrievalResult = JsonSerializer.Deserialize<JsonElement>("[{\"externalPensionPolicyId\": \"D9267759822\"}]") },
            new() { RetrievalResult = JsonSerializer.Deserialize<JsonElement>("[{\"externalPensionPolicyId\": \"D9267759823\"}]") },
            new() { RetrievalResult = JsonSerializer.Deserialize<JsonElement>("[{\"externalPensionPolicyId\": \"D9267759822\"}]") }
        };

        _mockRetrievalRecordFunctionClient
            .Setup(client => client.GetAsync(It.IsAny<RequestHeaderModel>()))
            .ReturnsAsync(retrievalRecord);
        
        _mockRetrievedPensionsRecordClient
            .Setup(client => client.GetAsync(It.IsAny<PensionsRetrievalRecordIdModel>(), It.IsAny<RequestHeaderModel>()))
            .ReturnsAsync(retrievedRecord);

        _mockIdValidator.Setup(v => v.IsValidGuid(It.IsAny<string>())).Returns(true);

        // Act
        var result = await _controller.GetPensionsDataAsync(requestHeader);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var responseModel = Assert.IsType<PensionsDataResponseModel>(okResult.Value);

        // Verify the mashed data output
        Assert.NotNull(responseModel.PensionPolicies);
        Assert.Equal(2, responseModel.PensionPolicies.Count); // Expect two distinct pension policies
        Assert.Equal("D9267759822", responseModel.PensionPolicies[0].PensionArrangements?[0].GetProperty("externalPensionPolicyId").GetString());
        Assert.Equal("D9267759823", responseModel.PensionPolicies[1].PensionArrangements?[0].GetProperty("externalPensionPolicyId").GetString());
    }

    [Fact]
    public async Task GetPensionsDataAsync_WhenNoExternalPensionPolicyIds_ReturnsEmptyPensionPolicies()
    {
        // Arrange
        var requestHeader = new RequestHeaderModel { UserSessionId = "123e4567-e89b-12d3-a456-426614174000" };
        
        var retrievalRecord = new PensionsRetrievalRecord
        {
            Id = Guid.NewGuid().ToString(),
            PeiData = [new PeiDataModel { RetrievalStatus = PensionProviderConstants.RetrievalStatus.RetrievalRequested }],
            PeiRetrievalComplete = true
        };
        
        var retrievedRecord = new List<RetrievedPensionRecord>
        {
            new() { RetrievalResult = JsonSerializer.Deserialize<JsonElement>("[{\"externalPensionPolicyId\": \"D9267759822\"}]") },
            new() { RetrievalResult = JsonSerializer.Deserialize<JsonElement>("[{\"externalPensionPolicyId\": \"D9267759823\"}]") },
            new() { RetrievalResult = JsonSerializer.Deserialize<JsonElement>("[{\"externalPensionPolicyId\": \"D9267759824\"}]") }
        };

        _mockRetrievalRecordFunctionClient
            .Setup(client => client.GetAsync(It.IsAny<RequestHeaderModel>()))
            .ReturnsAsync(retrievalRecord);
        
        _mockRetrievedPensionsRecordClient
            .Setup(client => client.GetAsync(It.IsAny<PensionsRetrievalRecordIdModel>(), It.IsAny<RequestHeaderModel>()))
            .ReturnsAsync(retrievedRecord);

        _mockIdValidator.Setup(v => v.IsValidGuid(It.IsAny<string>())).Returns(true);

        // Act
        var result = await _controller.GetPensionsDataAsync(requestHeader);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var responseModel = Assert.IsType<PensionsDataResponseModel>(okResult.Value);

        // Verify the mashed data output
        Assert.NotNull(responseModel.PensionPolicies);
        Assert.Equal(3, responseModel.PensionPolicies.Count); // Expect two distinct pension policies
        Assert.Equal("D9267759822", responseModel.PensionPolicies[0].PensionArrangements?[0].GetProperty("externalPensionPolicyId").GetString());
        Assert.Equal("D9267759823", responseModel.PensionPolicies[1].PensionArrangements?[0].GetProperty("externalPensionPolicyId").GetString());
        Assert.Equal("D9267759824", responseModel.PensionPolicies[2].PensionArrangements?[0].GetProperty("externalPensionPolicyId").GetString());
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public async Task GetPensionsDataAsync_WhenPeiStatusMixed_ReturnsCorrectRetrievalStatus(bool allPeisMatch, bool expectedStatus)
    {
        // Arrange
        var requestHeader = new RequestHeaderModel { UserSessionId = "123e4567-e89b-12d3-a456-426614174000" };
        var arrangementPei1 = $"{Guid.NewGuid()}:{Guid.NewGuid()}";
        var arrangementPei2 = $"{Guid.NewGuid()}:{Guid.NewGuid()}";
        var arrangementPei3 = $"{Guid.NewGuid()}:{Guid.NewGuid()}";

        var retrievalRecord = new PensionsRetrievalRecord
        {
            Id = Guid.NewGuid().ToString(),
            PeiData = [
                new PeiDataModel {
                    Pei = arrangementPei1,
                    RetrievalStatus = PensionProviderConstants.RetrievalStatus.RetrievalRequested
                },
                new PeiDataModel {
                    Pei = arrangementPei2,
                    RetrievalStatus = PensionProviderConstants.RetrievalStatus.RetrievalRequested
                },
                new PeiDataModel {
                    Pei = allPeisMatch ? arrangementPei3 : $"{Guid.NewGuid()}:{Guid.NewGuid()}",
                    RetrievalStatus = PensionProviderConstants.RetrievalStatus.RetrievalRequested
                }
            ],
            PeiRetrievalComplete = true
        };

        var retrievedRecord = new List<RetrievedPensionRecord>
        {
            new() { 
                Pei = arrangementPei1,
                RetrievalResult = JsonSerializer.Deserialize<JsonElement>("[{\"externalPensionPolicyId\": \"D9267759822\"}]") 
            },
            new() { 
                Pei = arrangementPei2,
                RetrievalResult = JsonSerializer.Deserialize<JsonElement>("{\"errorCode\": \"" + PensionProviderConstants.RetrievalErrorCodes.SystemError + "\"}") 
            },
            new() { 
                Pei = allPeisMatch ? arrangementPei3 : $"{Guid.NewGuid()}:{Guid.NewGuid()}",
                RetrievalResult = JsonSerializer.Deserialize<JsonElement>("[{\"externalPensionPolicyId\": \"D9267759824\"}]") 
            }
        };

        _mockRetrievalRecordFunctionClient
            .Setup(client => client.GetAsync(It.IsAny<RequestHeaderModel>()))
            .ReturnsAsync(retrievalRecord);

        _mockRetrievedPensionsRecordClient
            .Setup(client => client.GetAsync(It.IsAny<PensionsRetrievalRecordIdModel>(), It.IsAny<RequestHeaderModel>()))
            .ReturnsAsync(retrievedRecord);

        _mockIdValidator.Setup(v => v.IsValidGuid(It.IsAny<string>())).Returns(true);

        // Act
        var result = await _controller.GetPensionsDataAsync(requestHeader);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var responseModel = Assert.IsType<PensionsDataResponseModel>(okResult.Value);

        // Verify the mashed data output
        Assert.True(responseModel.PensionsDataRetrievalComplete == expectedStatus);
    }

    [Theory]
    [InlineData (true)]
    [InlineData (false)]
    public async Task DeletePensionsDataAsync_ReturnsResult(bool recordsExist)
    {
        // Arrange
        var requestHeader = new RequestHeaderModel
        {
            UserSessionId = "123e4567-e89b-12d3-a456-426614174000",
            CorrelationId = Guid.NewGuid().ToString()
        };

        var getResult = new PensionsRetrievalRecord
        {
            Id = recordsExist ? Guid.NewGuid().ToString() : string.Empty
        };

        // Simulate a successful response from the retrieval record function client
        _mockRetrievalRecordFunctionClient
            .Setup(client => client.GetAsync(It.IsAny<RequestHeaderModel>()))
            .ReturnsAsync(getResult);

        _mockIdValidator.Setup(v => v.IsValidGuid(It.IsAny<string>())).Returns(true);

        // Act
        var deleteResult = await _controller.DeletePensionsDataAsync(requestHeader);

        // Assert
        Assert.IsType<NoContentResult>(deleteResult);
    }
}
