using MhpdCommon.Constants;
using MhpdCommon.Models.Configuration;
using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.Models.MHPDModels;
using MhpdCommon.Models.RequestHeaderModel;
using MhpdCommon.Repository;
using MhpdCommon.SharedHttpClient;
using MhpdCommon.TokenValidation;
using MhpdCommon.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using PensionsDataService.Controllers;
using PensionsDataService.HttpClients;
using PensionsDataService.Models;
using PensionsDataService.Utilities;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using static MhpdCommon.ViewData.EvaluationConstants;

namespace PensionsDataServiceUnitTests;

public class PensionsDataControllerTests
{
    private readonly Mock<IIdValidator> _mockIdValidator = new();
    private readonly Mock<ITokenIntegrationServiceClient> _mockTokenIntegrationServiceClient = new();
    private readonly Mock<IMapsCdaServiceClient> _mockMapsCdaServiceClient = new();
    private readonly Mock<IRetrievalRecordServiceClient> _mockRetrievalRecordFunctionClient = new();
    private readonly Mock<IRetrievedPensionsRecordClient> _mockRetrievedPensionsRecordClient = new();
    private readonly Mock<ICosmosDbRepository<UserSessionData>> _mockUserSessionDataRepository = new();
    private readonly Mock<IPensionAnonymizer> _mockPensionAnonymizer = new();

    private readonly PensionsDataController _controller;

    private const string ValidPeisId = "0001f518264ba44564b186f42af6659b5822eb6e";
    private const string ValidClientId = "clientId";

    private readonly RequestHeaderModel _validRequestHeader = new()
    {
        Iss = "valid-iss", 
        UserSessionId = "123e4567-e89b-12d3-a456-426614174000",
        CorrelationId = "bc8174f0-5ff8-40a1-aa4b-d15c235dd2c5"
    };

    public PensionsDataControllerTests()
    {
        Mock<ILogger<PensionsDataController>> mockLogger = new();
        Mock<IOptions<CommonServiceBusConfiguration>> mockServiceBusOptions = new();
        Mock<IOptions<PeiOrchestrationSettings>> mockOrchestrationSettings = new();
        Mock<ICardDataRuleEngine> mockCardDataRuleEngine = new();
        Mock<ISummaryDataRuleEngine> mockSummaryDataRuleEngine = new();
        Mock<ITimelineSeriesBuilder> mockTimeSeriesBuilder = new();
        Mock<IDetailDataRuleEngine> mockDetailDataRuleEngine = new();

        var configuration = new CosmosBusinessConfiguration
        {
            DatabaseId = "TestDatabase",
            UserSessionDataContainer = "TestUserSessionDataContainer",
        };
        
        Mock<IOptions<CosmosBusinessConfiguration>> mockCosmosConfigOptions = new();
        mockCosmosConfigOptions.Setup(x => x.Value).Returns(configuration);

        
        UserSessionData? model = null;

        var response = new Mock<ItemResponse<UserSessionData>>();
        response.Setup(r => r.Resource).Returns(model!);

        // Arrange: Set up the mocks for the dependencies
        _mockRetrievalRecordFunctionClient.Setup(m => m.PostAsync(It.IsAny<RequestHeaderModel>(), It.IsAny<PensionRetrievalPayload>()))
            .ReturnsAsync(new PensionsRetrievalRecord());

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

        // Create an instance of PensionServiceClients with mocked dependencies
        Mock<PensionServiceClients> mockServiceClients = new(
            _mockTokenIntegrationServiceClient.Object,
            _mockRetrievalRecordFunctionClient.Object,
            _mockRetrievedPensionsRecordClient.Object,
            _mockMapsCdaServiceClient.Object,
            mockServiceBusOptions.Object     
        );
        
        // Get ordered validators
        var validators = Helper.GetOrderedValidators();

        // Create the PensionsDataRequestValidatorPipeline with the mock validators
        Mock<PensionsDataRequestValidatorPipeline> mockValidatorPipeline = new(validators);

        _mockPensionAnonymizer.Setup(p => p.Anonymize(It.IsAny<string>()))
            .Returns((string data) => data);

        mockCardDataRuleEngine.Setup(c => c.Evaluate(It.IsAny<JsonNode>(), It.IsAny<string>()))
            .Returns(new CardData());

        mockSummaryDataRuleEngine.Setup(s => s.Evaluate(It.IsAny<RetrievedPensionRecord>(), It.IsAny<IReadOnlyList<RetrievedPensionRecord>>()))
            .Returns(new SummaryData());

        mockTimeSeriesBuilder.Setup(t => t.Build(It.IsAny<IEnumerable<RetrievedPensionRecord>>(), false))
            .Returns(new TimelineSeries());

        mockDetailDataRuleEngine.Setup(d => d.Evaluate(It.IsAny<RetrievedPensionRecord>()))
            .Returns(new DetailData());

        Mock<PensionServiceUtilities> mockServiceUtilities = new(
            _mockPensionAnonymizer.Object,
            mockCardDataRuleEngine.Object,
            mockSummaryDataRuleEngine.Object, 
            mockTimeSeriesBuilder.Object,
            mockDetailDataRuleEngine.Object
            );

        _controller = new PensionsDataController(
            mockLogger.Object, 
            _mockIdValidator.Object, 
            mockValidatorPipeline.Object, 
            mockServiceClients.Object, 
            mockOrchestrationSettings.Object,
            _mockUserSessionDataRepository.Object,
            mockServiceUtilities.Object
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
        _mockTokenIntegrationServiceClient.Setup(client => client.PostIdTokenAsync(It.IsAny<PensionsDataRequestModel>(), It.IsAny<string>()))
            .ReturnsAsync(new PeiRetrievalDetailsResponseModel
            {
                PeisId = ValidPeisId
            });

        // Act
        var result = await _controller.PostPensionsDataAsync(request, _validRequestHeader);

        // Assert
        var statusCodeResult = Assert.IsType<AcceptedResult>(result);
        Assert.Equal((int)HttpStatusCode.Accepted, statusCodeResult.StatusCode);
        _mockTokenIntegrationServiceClient.Verify(client => client.PostIdTokenAsync(It.IsAny<PensionsDataRequestModel>(), 
            It.Is<string>(id => id == _validRequestHeader.CorrelationId)), Times.Once);
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
        _mockTokenIntegrationServiceClient.Setup(client => client.PostIdTokenAsync(It.IsAny<PensionsDataRequestModel>(), It.IsAny<string>()))
            .ReturnsAsync(new PeiRetrievalDetailsResponseModel());

        // Act
        var result = await _controller.PostPensionsDataAsync(request, _validRequestHeader);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal((int)HttpStatusCode.InternalServerError, statusCodeResult.StatusCode);
        _mockTokenIntegrationServiceClient.Verify(client => client.PostIdTokenAsync(It.IsAny<PensionsDataRequestModel>(), 
            It.Is<string>(id => id == _validRequestHeader.CorrelationId)), Times.Once);
    }

    [Fact]
    public async Task GetPensionsStatusAsync_WhenUserSessionIdIsMissing_ThenReturnsBadRequest()
    {
        // Arrange
        var requestHeader = new RequestHeaderModel { UserSessionId = null };  // Invalid UserSessionId

        // Act
        var result = await _controller.GetPensionsStatusAsync(requestHeader.UserSessionId, requestHeader.CorrelationId);

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
        var result = await _controller.GetPensionsStatusAsync(requestHeader.UserSessionId, requestHeader.CorrelationId);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(TokenValidationMessages.InvalidUserSessionId, badRequestResult.Value);
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
            await _controller.GetPensionSummaryAsync(requestHeader.UserSessionId, requestHeader.CorrelationId));
    }

    [Fact]
    public async Task GetPensionsStatusAsync_WhenNoSessionData_ThenReturnsOkWithNullResponse()
    {
        // Arrange
        var requestHeader = new RequestHeaderModel { UserSessionId = "123e4567-e89b-12d3-a456-426614174000" };

        // Simulate an empty response from the retrieval record function client
        var retrievalRecord = new PensionsRetrievalRecord();

        _mockRetrievalRecordFunctionClient
            .Setup(client => client.GetAsync(It.IsAny<RequestHeaderModel>()))
            .ReturnsAsync(retrievalRecord);

        _mockIdValidator.Setup(v => v.IsValidGuid(It.IsAny<string>())).Returns(true);

        // Act
        var result = await _controller.GetPensionsStatusAsync(requestHeader.UserSessionId, requestHeader.CorrelationId);

        // Assert
        Assert.IsType<JsonResult>(result);
    }

    [Fact]
    public async Task GetPensionsStatusAsync_WhenHasPeiData_ThenReturnsOkWithResponse()
    {
        // Arrange
        var requestHeader = new RequestHeaderModel { UserSessionId = "123e4567-e89b-12d3-a456-426614174000" };

        var retrievalRecord = new PensionsRetrievalRecord
        {
            Id = Guid.NewGuid().ToString(),
            PeiData = [
                new PeiDataModel { 
                    Pei = "123e4567-e89b-12d3-a456-426614174000:123e4567-e89b-12d3-a456-426614174001",
                    RetrievalStatus = PensionProviderConstants.RetrievalStatus.RetrievalRequested 
                }
            ],
            PeiRetrievalComplete = true
        };

        List<string> retrievedPeis = ["123e4567-e89b-12d3-a456-426614174000:123e4567-e89b-12d3-a456-426614174001"];

        _mockRetrievalRecordFunctionClient
            .Setup(client => client.GetAsync(It.IsAny<RequestHeaderModel>()))
            .ReturnsAsync(retrievalRecord);

        _mockRetrievedPensionsRecordClient
            .Setup(client => client.GetRetrievedPeisAsync(It.IsAny<RequestHeaderModel>()))
            .ReturnsAsync(retrievedPeis);

        _mockIdValidator.Setup(v => v.IsValidGuid(It.IsAny<string>())).Returns(true);

        // Act
        var result = await _controller.GetPensionsStatusAsync(requestHeader.UserSessionId, requestHeader.CorrelationId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var responseModel = Assert.IsType<PensionsStatusResponseModel>(okResult.Value);
        Assert.True(responseModel.PensionsDataRetrievalComplete);
    }

    [Fact]
    public async Task GetPensionSummaryAsync_WhenHasPeiData_ThenReturnsOkWithResponse()
    {
        // Arrange
        var requestHeader = new RequestHeaderModel { UserSessionId = "123e4567-e89b-12d3-a456-426614174000" };

        var peis = CreatePeisList();
        var requestedPension = CreateRetrievalRecord(peis);
        var retrievedPensions = CreateRetrievedRecords(peis);

        _mockRetrievalRecordFunctionClient
            .Setup(client => client.GetAsync(It.IsAny<RequestHeaderModel>()))
            .ReturnsAsync(requestedPension);

        _mockRetrievedPensionsRecordClient
            .Setup(client => client.GetRetrievedPensionsAsync(It.IsAny<RetrievedPensionsRequest>(), It.IsAny<RequestHeaderModel>()))
            .ReturnsAsync(retrievedPensions);

        _mockIdValidator.Setup(v => v.IsValidGuid(It.IsAny<string>())).Returns(true);

        // Act
        var result = await _controller.GetPensionSummaryAsync(requestHeader.UserSessionId, requestHeader.CorrelationId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var responseModel = Assert.IsType<PensionSummary>(okResult.Value);
        Assert.True(responseModel.IsPensionRetrievalComplete);
        Assert.Equal(peis.Count, responseModel.Pensions.Count);
        Assert.All(responseModel.Pensions, pension =>
        {
            Assert.Equal(PensionProviderConstants.RetrievalStatus.RetrievalComplete, pension.RetrievalStatus);
        });
    }

    [Fact]
    public async Task GetPensionSummaryAsync_WhenPeiHasError_ThenReturnsOkWithResponse()
    {
        // Arrange
        var requestHeader = new RequestHeaderModel { UserSessionId = "123e4567-e89b-12d3-a456-426614174000" };

        var peis = CreatePeisList();
        var errorPei = CreatePei();

        var retrievalPeis = new List<string>(peis)
        {
            errorPei
        };
        var requestedPension = CreateRetrievalRecord(retrievalPeis);
        var retrievedPensions = CreateRetrievedRecords(peis, errorPei);

        _mockRetrievalRecordFunctionClient
            .Setup(client => client.GetAsync(It.IsAny<RequestHeaderModel>()))
            .ReturnsAsync(requestedPension);

        _mockRetrievedPensionsRecordClient
            .Setup(client => client.GetRetrievedPensionsAsync(It.IsAny<RetrievedPensionsRequest>(), It.IsAny<RequestHeaderModel>()))
            .ReturnsAsync(retrievedPensions);

        _mockIdValidator.Setup(v => v.IsValidGuid(It.IsAny<string>())).Returns(true);

        // Act
        var result = await _controller.GetPensionSummaryAsync(requestHeader.UserSessionId, requestHeader.CorrelationId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var responseModel = Assert.IsType<PensionSummary>(okResult.Value);
        Assert.True(responseModel.IsPensionRetrievalComplete);
        Assert.Equal(retrievalPeis.Count, responseModel.Pensions.Count);
        Assert.NotNull(responseModel.Pensions.SingleOrDefault(pension => pension.RetrievalStatus == PensionProviderConstants.RetrievalStatus.RetrievalFailed));
    }

    [Fact]
    public async Task GetPensionSummaryAsync_WhenPeiNotRetrieved_ThenReturnsOkWithResponse()
    {
        // Arrange
        var requestHeader = new RequestHeaderModel { UserSessionId = "123e4567-e89b-12d3-a456-426614174000" };

        var peis = CreatePeisList();
        var missingPei = CreatePei();

        var requestedPension = CreateRetrievalRecord(peis, missingPei);
        var retrievedPensions = CreateRetrievedRecords(peis);

        _mockRetrievalRecordFunctionClient
            .Setup(client => client.GetAsync(It.IsAny<RequestHeaderModel>()))
            .ReturnsAsync(requestedPension);

        _mockRetrievedPensionsRecordClient
            .Setup(client => client.GetRetrievedPensionsAsync(It.IsAny<RetrievedPensionsRequest>(), It.IsAny<RequestHeaderModel>()))
            .ReturnsAsync(retrievedPensions);

        _mockIdValidator.Setup(v => v.IsValidGuid(It.IsAny<string>())).Returns(true);

        // Act
        var result = await _controller.GetPensionSummaryAsync(requestHeader.UserSessionId, requestHeader.CorrelationId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var responseModel = Assert.IsType<PensionSummary>(okResult.Value);
        Assert.False(responseModel.IsPensionRetrievalComplete);
        Assert.Equal(peis.Count + 1, responseModel.Pensions.Count);
        Assert.NotNull(responseModel.Pensions.SingleOrDefault(pension => pension.RetrievalStatus == PensionProviderConstants.RetrievalStatus.RetrievalRequested));
    }

    [Fact]
    public async Task GetPensionTimelineAsync_WhenHasPeiData_ThenReturnsOkWithResponse()
    {
        // Arrange
        var requestHeader = new RequestHeaderModel { UserSessionId = "123e4567-e89b-12d3-a456-426614174000" };

        var peis = CreatePeisList();
        var requestedPension = CreateRetrievalRecord(peis);
        var retrievedPensions = CreateRetrievedRecords(peis);

        _mockRetrievalRecordFunctionClient
            .Setup(client => client.GetAsync(It.IsAny<RequestHeaderModel>()))
            .ReturnsAsync(requestedPension);

        _mockRetrievedPensionsRecordClient
            .Setup(client => client.GetRetrievedPensionsAsync(It.IsAny<RetrievedPensionsRequest>(), It.IsAny<RequestHeaderModel>()))
            .ReturnsAsync(retrievedPensions);

        _mockIdValidator.Setup(v => v.IsValidGuid(It.IsAny<string>())).Returns(true);

        // Act
        var result = await _controller.GetPensionTimelineAsync(requestHeader.UserSessionId, requestHeader.CorrelationId, Constants.TimeSeries.Standard);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var responseModel = Assert.IsType<TimelineSeries>(okResult.Value);
        Assert.True(responseModel.IsPensionRetrievalComplete);
    }

    [Fact]
    public async Task GetPensionTimelineAsync_WhenPeiNotRetrieved_ThenReturnsOkWithResponse()
    {
        // Arrange
        var requestHeader = new RequestHeaderModel { UserSessionId = "123e4567-e89b-12d3-a456-426614174000" };

        var peis = CreatePeisList();
        var missingPei = CreatePei();

        var requestedPension = CreateRetrievalRecord(peis, missingPei);
        var retrievedPensions = CreateRetrievedRecords(peis);

        _mockRetrievalRecordFunctionClient
            .Setup(client => client.GetAsync(It.IsAny<RequestHeaderModel>()))
            .ReturnsAsync(requestedPension);

        _mockRetrievedPensionsRecordClient
            .Setup(client => client.GetRetrievedPensionsAsync(It.IsAny<RetrievedPensionsRequest>(), It.IsAny<RequestHeaderModel>()))
            .ReturnsAsync(retrievedPensions);

        _mockIdValidator.Setup(v => v.IsValidGuid(It.IsAny<string>())).Returns(true);

        // Act
        var result = await _controller.GetPensionTimelineAsync(requestHeader.UserSessionId, requestHeader.CorrelationId, Constants.TimeSeries.Standard);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var responseModel = Assert.IsType<TimelineSeries>(okResult.Value);
        Assert.False(responseModel.IsPensionRetrievalComplete);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task GetPensionByCategoryAsync_ReturnsFilteredPensions(bool queryByContact)
    {
        // Arrange
        var requestHeader = new RequestHeaderModel { UserSessionId = "123e4567-e89b-12d3-a456-426614174000" };

        var peis = CreatePeisList();
        var contactPei = CreatePei();

        var retrievalPeis = new List<string>(peis)
        {
            contactPei
        };

        var requestedPension = CreateRetrievalRecord(retrievalPeis);
        var retrievedPensions = CreateRetrievedRecords(peis);

        retrievedPensions.Add(CreateValidRetrievedPension(contactPei, Category.Contact));

        _mockRetrievalRecordFunctionClient
            .Setup(client => client.GetAsync(It.IsAny<RequestHeaderModel>()))
            .ReturnsAsync(requestedPension);

        _mockRetrievedPensionsRecordClient
            .Setup(client => client.GetRetrievedPensionsAsync(It.IsAny<RetrievedPensionsRequest>(), It.IsAny<RequestHeaderModel>()))
            .ReturnsAsync(retrievedPensions);

        _mockIdValidator.Setup(v => v.IsValidGuid(It.IsAny<string>())).Returns(true);

        var category = queryByContact ? Category.Contact : Category.Confirmed;

        // Act
        var result = await _controller.GetPensionsByCategoryAsync(category, requestHeader.UserSessionId, requestHeader.CorrelationId);

        // Assert
        var expectedCount = queryByContact ? 1 : peis.Count;
        var okResult = Assert.IsType<OkObjectResult>(result);
        var responseModel = Assert.IsType<PensionData>(okResult.Value);
        Assert.True(responseModel.IsPensionRetrievalComplete);
        Assert.Equal(expectedCount, responseModel.Arrangements.Count);

        if (queryByContact)
        {
            Assert.Equal(0, responseModel.TotalContactPensions);
        }
        else
        {
            Assert.Equal(1, responseModel.TotalContactPensions);
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task GetPensionDetailAsync_ReturnsOk(bool idHasMatch)
    {
        // Arrange
        var requestHeader = new RequestHeaderModel { UserSessionId = "123e4567-e89b-12d3-a456-426614174000" };

        var detailPei = CreatePei();

        var retrievalPeis = new List<string>
        {
            detailPei
        };

        var requestedPension = CreateRetrievalRecord(retrievalPeis);
        var retrievedPensions = new List<RetrievedPensionRecord>();

        if (idHasMatch)
        {
            retrievedPensions.Add(CreateValidRetrievedPension(detailPei));
        }

        _mockRetrievalRecordFunctionClient
            .Setup(client => client.GetAsync(It.IsAny<RequestHeaderModel>()))
            .ReturnsAsync(requestedPension);

        _mockRetrievedPensionsRecordClient
            .Setup(client => client.GetRetrievedPensionsAsync(It.IsAny<RetrievedPensionsRequest>(), It.IsAny<RequestHeaderModel>()))
            .ReturnsAsync(retrievedPensions);

        _mockIdValidator.Setup(v => v.IsValidGuid(It.IsAny<string>())).Returns(true);

        // Act
        var result = await _controller.GetPensionDetailAsync(detailPei, requestHeader.UserSessionId, requestHeader.CorrelationId);

        // Assert
        if (!idHasMatch)
        {
            Assert.IsType<NotFoundObjectResult>(result);
        }else
        {
            Assert.IsType<OkObjectResult>(result);
        }
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
            .Setup(client => client.GetRetrievedPensionsAsync(It.IsAny<RetrievedPensionsRequest>(), It.IsAny<RequestHeaderModel>()))
            .ReturnsAsync(retrievedRecord);

        _mockIdValidator.Setup(v => v.IsValidGuid(It.IsAny<string>())).Returns(true);

        // Act
        var result = await _controller.GetPensionSummaryAsync(requestHeader.UserSessionId, requestHeader.CorrelationId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var responseModel = Assert.IsType<PensionSummary>(okResult.Value);

        // Verify the mashed data output
        Assert.True(responseModel.IsPensionRetrievalComplete == expectedStatus);
    }

    [Theory]
    [InlineData (true)]
    [InlineData (false)]
    public async Task DeletePensionsDataAsync_ReturnsResult(bool recordsExist)
    {
        // Arrange
        var userSessionId = Guid.NewGuid().ToString();
        var correlationId = Guid.NewGuid().ToString();

        var getResult = new PensionsRetrievalRecord
        {
            Id = recordsExist ? Guid.NewGuid().ToString() : string.Empty
        };

        // Simulate a successful response from the retrieval record function client
        _mockRetrievalRecordFunctionClient
            .Setup(client => client.GetAsync(It.IsAny<RequestHeaderModel>()))
            .ReturnsAsync(getResult);

        _mockUserSessionDataRepository
            .Setup(repo => repo.DeleteByIdAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.FromResult(recordsExist));

        _mockIdValidator.Setup(v => v.IsValidGuid(It.IsAny<string>())).Returns(true);

        // Act
        var deleteResult = await _controller.DeletePensionsDataAsync(userSessionId, correlationId);

        // Assert
        if (recordsExist)
        {
            _mockRetrievalRecordFunctionClient
                .Verify(client => client.DeleteAsync(userSessionId, correlationId), Times.Once);
            _mockRetrievedPensionsRecordClient
                .Verify(client => client.DeleteAsync(userSessionId, correlationId), Times.Once);
        }

        _mockUserSessionDataRepository
            .Verify(repo => repo.DeleteByIdAsync(userSessionId, userSessionId), Times.Once);
        Assert.IsType<NoContentResult>(deleteResult);
    }

    [Fact]
    public async Task PostPensionsDataRetrievalAsync_ReturnsBadRequest_WhenValidationFails()
    {
        // Arrange
        var request = new PensionsDataRetrievalRequest();
        var requestHeader = new RequestHeaderModel { CorrelationId = "123", UserSessionId = "session-123" };
        
        // Act
        var result = await _controller.PostPensionsDataRetrievalAsync(request, requestHeader);
        
        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }
    
    [Fact]
    public async Task PostPensionsDataRetrievalAsync_ReturnsBadRequest_No_ClientId_Provided_ValidationFails()
    {
        // Arrange
        var request = new PensionsDataRetrievalRequest
        {
            Ticket = TokenQueryParams.ValidJwtToken
        };
        
        _mockIdValidator.Setup(v => v.IsValidGuid(It.IsAny<string>())).Returns(true);

        var requestHeader = new RequestHeaderModel { CorrelationId = Guid.NewGuid().ToString(), UserSessionId = Guid.NewGuid().ToString(), Iss = "mhpdIss"};
        
        // Act
        var result = await _controller.PostPensionsDataRetrievalAsync(request, requestHeader);
        
        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task PostPensionsDataRetrievalAsync_ReturnsInternalServerError_WhenTokenServiceFails()
    {
        // Arrange
        var request = new PensionsDataRetrievalRequest();
        var requestHeader = new RequestHeaderModel { CorrelationId = "123", UserSessionId = "session-123" };
        
        _mockMapsCdaServiceClient.Setup(x => x.GetRqp(It.IsAny<RequestHeaderModel>()))
            .ReturnsAsync(new MapsRqpServiceResponseModel());
        
        _mockTokenIntegrationServiceClient.Setup(x => x.PostAccessTokenAsync(It.IsAny<TokenClientRequestModel>(), It.IsAny<string>()))
            .ReturnsAsync(new CdaTokenResponseModel { Pct = null, AccessToken = null });
        
        // Act
        var result = await _controller.PostPensionsDataRetrievalAsync(request, requestHeader);
        
        // Assert
        var objectResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, objectResult.StatusCode);
    }

    [Fact]
    public async Task PostPensionsDataRetrievalAsync_ReturnsAccepted_WhenSuccessful()
    {
        // Arrange
        var request = new PensionsDataRetrievalRequest
        {
            Ticket = TokenQueryParams.ValidJweToken,
            ClientId = ValidClientId
        };
        var requestHeader = new RequestHeaderModel { CorrelationId = Guid.NewGuid().ToString(), UserSessionId = Guid.NewGuid().ToString(), Iss = "mhpd"};

        _mockMapsCdaServiceClient.Setup(x => x.GetRqp(It.IsAny<RequestHeaderModel>()))
            .ReturnsAsync(new MapsRqpServiceResponseModel { Rqp = TokenQueryParams.ValidJwtToken });

        _mockTokenIntegrationServiceClient.Setup(x => x.PostIdTokenAsync(It.IsAny<PensionsDataRequestModel>(), It.IsAny<string>()))
            .ReturnsAsync(new PeiRetrievalDetailsResponseModel { PeisId = "0001f518264ba44564b186f42af6659b5822eb6e" });
        
        _mockTokenIntegrationServiceClient.Setup(x => x.PostAccessTokenAsync(It.IsAny<TokenClientRequestModel>(), It.IsAny<string>()))
            .ReturnsAsync(new CdaTokenResponseModel { Pct = TokenQueryParams.ValidJwtToken, AccessToken = TokenQueryParams.ValidJwtToken });
        
        var testInstanceData = new UserSessionData
        {
            UserSessionId = Guid.NewGuid().ToString(),
            PeisId = "some-test-peis-id",
        };
        
        _mockIdValidator.Setup(v => v.IsValidGuid(It.IsAny<string>())).Returns(true);
        
        _mockUserSessionDataRepository.Setup(x => x.GetByIdAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(testInstanceData);

        _mockUserSessionDataRepository.Setup(x => x.InsertItemAsync(It.IsAny<UserSessionData>(), It.IsAny<string>()))
            .Verifiable();
        
        // Act
        var result = await _controller.PostPensionsDataRetrievalAsync(request, requestHeader);
        
        // Assert
        Assert.IsType<AcceptedResult>(result);
    }
    
    [Fact]
    public async Task PostPensionsDataRetrievalAsync_ReturnsInternalServerError_WhenRqpIsInvalid()
    {
        // Arrange
        var request = new PensionsDataRetrievalRequest { Ticket = TokenQueryParams.ValidJweToken, ClientId = ValidClientId };
        var requestHeader = new RequestHeaderModel 
        { 
            CorrelationId = Guid.NewGuid().ToString(), 
            UserSessionId = Guid.NewGuid().ToString(), 
            Iss = "mhpd"
        };

        _mockIdValidator.Setup(v => v.IsValidGuid(It.IsAny<string>())).Returns(true);
        
        _mockMapsCdaServiceClient
            .Setup(x => x.GetRqp(It.IsAny<RequestHeaderModel>()))
            .ReturnsAsync(new MapsRqpServiceResponseModel { Rqp = string.Empty });

        // Act
        var result = await _controller.PostPensionsDataRetrievalAsync(request, requestHeader);

        // Assert
        Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, ((ObjectResult)result).StatusCode);
    }

    [Fact]
    public async Task PostPensionsDataRetrievalAsync_ReturnsInternalServerError_WhenAccessTokenIsInvalid()
    {
        // Arrange
        var request = new PensionsDataRetrievalRequest { Ticket = TokenQueryParams.ValidJweToken, ClientId = ValidClientId };
        var requestHeader = new RequestHeaderModel 
        { 
            CorrelationId = Guid.NewGuid().ToString(), 
            UserSessionId = Guid.NewGuid().ToString(), 
            Iss = "mhpd"
        };

        _mockIdValidator.Setup(v => v.IsValidGuid(It.IsAny<string>())).Returns(true);
        
        _mockMapsCdaServiceClient
            .Setup(x => x.GetRqp(It.IsAny<RequestHeaderModel>()))
            .ReturnsAsync(new MapsRqpServiceResponseModel { Rqp = TokenQueryParams.ValidJwtToken });

        _mockTokenIntegrationServiceClient
            .Setup(x => x.PostAccessTokenAsync(It.IsAny<TokenClientRequestModel>(), It.IsAny<string>()))
            .ReturnsAsync(new CdaTokenResponseModel 
            { 
                Pct = string.Empty, 
                AccessToken = string.Empty 
            });

        // Act
        var result = await _controller.PostPensionsDataRetrievalAsync(request, requestHeader);

        // Assert
        Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, ((ObjectResult)result).StatusCode);
    }

    [Fact]
    public async Task PostPensionsDataRetrievalAsync_ReturnsInternalServerError_WhenUserSessionDataIsNull()
    {
        // Arrange
        var request = new PensionsDataRetrievalRequest { Ticket = TokenQueryParams.ValidJweToken, ClientId = ValidClientId };
        var requestHeader = new RequestHeaderModel 
        { 
            CorrelationId = Guid.NewGuid().ToString(), 
            UserSessionId = Guid.NewGuid().ToString(), 
            Iss = "mhpd"
        };
        
        _mockIdValidator.Setup(v => v.IsValidGuid(It.IsAny<string>())).Returns(true);

        _mockMapsCdaServiceClient
            .Setup(x => x.GetRqp(It.IsAny<RequestHeaderModel>()))
            .ReturnsAsync(new MapsRqpServiceResponseModel { Rqp = TokenQueryParams.ValidJwtToken });

        _mockTokenIntegrationServiceClient
            .Setup(x => x.PostAccessTokenAsync(It.IsAny<TokenClientRequestModel>(), It.IsAny<string>()))
            .ReturnsAsync(new CdaTokenResponseModel 
            { 
                Pct = TokenQueryParams.ValidJwtToken, 
                AccessToken = TokenQueryParams.ValidJwtToken 
            });

        UserSessionData? userSessionData = null;
        
        _mockIdValidator.Setup(v => v.IsValidGuid(It.IsAny<string>())).Returns(true);

        _mockUserSessionDataRepository.Setup(x => x.GetByIdAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(userSessionData);

        _mockUserSessionDataRepository.Setup(x => x.InsertItemAsync(It.IsAny<UserSessionData>(), It.IsAny<string>()))
            .Verifiable();

        // Act
        var result = await _controller.PostPensionsDataRetrievalAsync(request, requestHeader);

        // Assert
        Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, ((ObjectResult)result).StatusCode);
    }

    [Fact]
    public async Task PostPensionsDataRetrievalAsync_ReturnsInternalServerError_WhenPeisIdIsInvalid()
    {
        // Arrange
        var request = new PensionsDataRetrievalRequest { Ticket = TokenQueryParams.ValidJweToken, ClientId = ValidClientId };
        var requestHeader = new RequestHeaderModel 
        { 
            CorrelationId = Guid.NewGuid().ToString(), 
            UserSessionId = Guid.NewGuid().ToString(), 
            Iss = "mhpd"
        };

        _mockMapsCdaServiceClient
            .Setup(x => x.GetRqp(It.IsAny<RequestHeaderModel>()))
            .ReturnsAsync(new MapsRqpServiceResponseModel { Rqp = TokenQueryParams.ValidJwtToken });

        _mockTokenIntegrationServiceClient
            .Setup(x => x.PostAccessTokenAsync(It.IsAny<TokenClientRequestModel>(), It.IsAny<string>()))
            .ReturnsAsync(new CdaTokenResponseModel 
            { 
                Pct = TokenQueryParams.ValidJwtToken, 
                AccessToken = TokenQueryParams.ValidJwtToken 
            });

        var testInstanceData = new UserSessionData
        {
            UserSessionId = Guid.NewGuid().ToString(),
            PeisId = string.Empty,
        };

        _mockIdValidator.Setup(v => v.IsValidGuid(It.IsAny<string>())).Returns(true);

        _mockUserSessionDataRepository.Setup(x => x.GetByIdAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(testInstanceData);

        _mockUserSessionDataRepository.Setup(x => x.InsertItemAsync(It.IsAny<UserSessionData>(), It.IsAny<string>()))
            .Verifiable();

        // Act
        var result = await _controller.PostPensionsDataRetrievalAsync(request, requestHeader);

        // Assert
        Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, ((ObjectResult)result).StatusCode);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task GetPensionsAnalyticsAsync_ReturnsResponse(bool isRetrievalComplete)
    {
        // Arrange
        var requestHeader = new RequestHeaderModel
        {
            UserSessionId = Guid.NewGuid().ToString(),
            CorrelationId = Guid.NewGuid().ToString()
        };

        var peis = CreatePeisList();
        var errorPei = CreatePei();
        var pendingPei = CreatePei();
        var contactPei = CreatePei();
        var unsupportedPei = CreatePei();

        var retrievalPeis = new List<string>(peis)
        {
            errorPei, pendingPei, contactPei, unsupportedPei
        };

        var requestedPension = CreateRetrievalRecord(retrievalPeis);

        var retrievedPensions = new List<RetrievedPensionRecord>();

        if (isRetrievalComplete)
        {
            retrievedPensions = CreateRetrievedRecords(peis, errorPei);
            retrievedPensions.Add(CreateValidRetrievedPension(contactPei, Category.Contact));
            retrievedPensions.Add(CreateValidRetrievedPension(pendingPei, Category.Pending));
            retrievedPensions.Add(CreateValidRetrievedPension(unsupportedPei, Category.Unsupported));
        }

        _mockIdValidator.Setup(v => v.IsValidGuid(It.IsAny<string>())).Returns(true);

        _mockRetrievalRecordFunctionClient
            .Setup(client => client.GetAsync(It.IsAny<RequestHeaderModel>()))
            .ReturnsAsync(requestedPension);

        _mockRetrievedPensionsRecordClient
            .Setup(client => client.GetRetrievedPensionsAsync(It.IsAny<RetrievedPensionsRequest>(), It.IsAny<RequestHeaderModel>()))
            .ReturnsAsync(retrievedPensions);

        // Act
        var response = await _controller.GetPensionsAnalyticsAsync(requestHeader.UserSessionId, requestHeader.UserSessionId);

        // Assert
        if (isRetrievalComplete)
        {
            var okResult = Assert.IsType<OkObjectResult>(response);
            var responseModel = Assert.IsType<AnalyticsData>(okResult.Value);
            Assert.Equal(1, responseModel.TotalErrorPensions);
            Assert.Equal(5, responseModel.TotalPensions);
            Assert.Equal(1, responseModel.TotalUnsupportedPensions);
            Assert.Equal(3, responseModel.ConfirmedPensions.Count);
            Assert.Single(responseModel.UnconfirmedPensions);
            Assert.Single(responseModel.IncompletePensions);
            Assert.Single(responseModel.UnsupportedPensions);
            Assert.Single(responseModel.ErroredPensions);
        }
        else
        {
            var badResult = Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal("Pension data retrieval is not complete", badResult.Value);
        }
    }

    [Fact]
    public async Task GetPensionsAnalyticsAsync_WhenDeserializationFails_Returns500Response()
    {
        // Arrange
        var requestHeader = new RequestHeaderModel
        {
            UserSessionId = Guid.NewGuid().ToString(),
            CorrelationId = Guid.NewGuid().ToString()
        };

        var detailPei = CreatePei();

        var retrievalPeis = new List<string>
        {
            detailPei
        };

        var requestedPension = CreateRetrievalRecord(retrievalPeis);
        var retrievedPensions = new List<RetrievedPensionRecord>
        {
            CreateValidRetrievedPension(detailPei)
        };

        _mockIdValidator.Setup(v => v.IsValidGuid(It.IsAny<string>())).Returns(true);

        _mockRetrievalRecordFunctionClient
            .Setup(client => client.GetAsync(It.IsAny<RequestHeaderModel>()))
            .ReturnsAsync(requestedPension);

        _mockRetrievedPensionsRecordClient
            .Setup(client => client.GetRetrievedPensionsAsync(It.IsAny<RetrievedPensionsRequest>(), It.IsAny<RequestHeaderModel>()))
            .ReturnsAsync(retrievedPensions);

        _mockPensionAnonymizer
            .Setup(a => a.Anonymize(It.IsAny<string>()))
            .Returns("NOT JSON");

        // Act
        var response = await _controller.GetPensionsAnalyticsAsync(requestHeader.UserSessionId, requestHeader.UserSessionId);

        // Assert
        var result = Assert.IsType<ObjectResult>(response);
        Assert.Equal(500, result.StatusCode);
        Assert.Equal("Unable to collect pension retrieval analytics data", result.Value);
    }

    private static List<string> CreatePeisList()
    {
        return
        [
            CreatePei(),
            CreatePei(),
            CreatePei()
        ];
    }

    private static string CreatePei()
    {
        return $"{Guid.NewGuid()}:{Guid.NewGuid()}";
    }

    private static PensionsRetrievalRecord CreateRetrievalRecord(IEnumerable<string> peis, string? missingPei = null)
    {
        var record = new PensionsRetrievalRecord
        {
            Id = Guid.NewGuid().ToString(),
            PeiData = [.. peis.Select(pei =>
            {
                return new PeiDataModel
                {
                    Pei = pei,
                    RetrievalStatus = PensionProviderConstants.RetrievalStatus.RetrievalRequested
                };
            })],
            PeiRetrievalComplete = true
        };

        if(missingPei != null)
        {
            record.PeiData.Add(new PeiDataModel
            {
                Pei = missingPei,
                RetrievalStatus = PensionProviderConstants.RetrievalStatus.RetrievalRequested
            });
        }

        return record;
    }

    private static List<RetrievedPensionRecord> CreateRetrievedRecords(IEnumerable<string> peis, string? errorPei = null)
    {
        var records = new List<RetrievedPensionRecord>(peis.Select(pei => CreateValidRetrievedPension(pei)));

        if (errorPei != null)
        {
            records.Add(CreateErrorRetrievedPension(errorPei));
        }

        return records;
    }

    private static RetrievedPensionRecord CreateValidRetrievedPension(string pei, string category = Category.Confirmed)
    {
        var arrangement = @"[{""externalPensionPolicyId"": ""D9267759822""}]";

        return new RetrievedPensionRecord
        {
            Pei = pei,
            PensionType = "DB",
            MatchType = "POSS",
            AssetId = "asset-2",
            Category = category,
            SchemeName = "Scheme Two",
            HasIncome = "true",
            Administrator = "Admin Two",
            RetrievalResult = JsonSerializer.Deserialize<dynamic>(arrangement)
        };
    }

    private static RetrievedPensionRecord CreateErrorRetrievedPension(string pei)
    {
        var error = @"{""errorCode"": """ + PensionProviderConstants.RetrievalErrorCodes.SystemError + @"""}";

        return new RetrievedPensionRecord
        {
            Pei = pei,
            PensionType = "DB",
            MatchType = "POSS",
            AssetId = "asset-2",
            Category = Category.Error,
            SchemeName = "",
            HasIncome = "true",
            Administrator = "Admin Two",
            RetrievalResult = JsonSerializer.Deserialize<dynamic>(error)
        };
    }
}
