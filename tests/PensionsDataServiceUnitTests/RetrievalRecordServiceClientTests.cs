using MhpdCommon.Constants;
using MhpdCommon.Constants.HttpClient;
using MhpdCommon.CustomExceptions;
using MhpdCommon.ErrorHandling;
using MhpdCommon.Models.MHPDModels;
using MhpdCommon.Models.RequestHeaderModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using PensionsDataService.HttpClients;
using System.Net;
using System.Net.Http.Json;

namespace PensionsDataServiceUnitTests;

public class RetrievalRecordServiceClientTests
{
    private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
    private readonly Mock<ILogger<RetrievalRecordServiceClient>> _mockLogger;
    private readonly Mock<IErrorResolver> _errorResolver;
    private readonly RetrievalRecordServiceClient _client;

    public RetrievalRecordServiceClientTests()
    {
        _mockHttpClientFactory = new Mock<IHttpClientFactory>();
        _mockLogger = new Mock<ILogger<RetrievalRecordServiceClient>>();
        _errorResolver = new Mock<IErrorResolver>();
        Mock<IConfiguration> mockConfiguration = new();

        // Mock the HttpClient
        var handlerMock = new Mock<HttpMessageHandler>();
        var mockHttpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("https://mockendpoint.com/")
        };
        
        _mockHttpClientFactory.Setup(factory => factory.CreateClient(It.IsAny<string>())).Returns(mockHttpClient);

        mockConfiguration.Setup(config => config[HttpClientUrlVariables.RetrievedPensionsServiceUrl])
            .Returns("https://mockendpoint.com/");

        // Initialize the client
        _client = new RetrievalRecordServiceClient(_mockHttpClientFactory.Object, _mockLogger.Object, _errorResolver.Object);
    }

    [Fact]
    public async Task GetAsync_SuccessfulRequest_ReturnsOkResult()
    {
        // Arrange
        var requestHeader = new RequestHeaderModel { UserSessionId = "test-session-id" };
        
        var expectedRecords = new PensionsRetrievalRecord();
        
        var httpResponse = new HttpResponseMessage
        {
            Content = JsonContent.Create(expectedRecords),
            StatusCode = HttpStatusCode.OK,
        };

        // Mock a successful HTTP response
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(httpResponse);

        _mockHttpClientFactory.Setup(x => x.CreateClient(HttpClientNames.PensionRetrievalService))
            .Returns(new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("http://localhost:1234")
            });

        
        // Act
        var result = await _client.GetAsync(requestHeader);

        // Assert
        Assert.IsType<PensionsRetrievalRecord>(result);
    }

    [Fact]
    public async Task GetAsync_HttpRequestException_ThrowsServiceCommunicationException()
    {
        // Arrange
        var requestHeader = new RequestHeaderModel { UserSessionId = "test-session-id" };

        // Mock a failure HTTP response
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ThrowsAsync(new HttpRequestException("Request failed"));

        var httpClient = new HttpClient(handlerMock.Object);
        _mockHttpClientFactory.Setup(factory => factory.CreateClient(It.IsAny<string>())).Returns(httpClient);

        // Act & Assert
        await Assert.ThrowsAsync<ServiceCommunicationException>(() => _client.GetAsync(requestHeader));
    }

    [Fact]
    public async Task GetAsync_InvalidOperationDuringRequest_ThrowsInvalidOperationException()
    {
        // Arrange
        var requestHeader = new RequestHeaderModel { UserSessionId = "test-session-id" };

        // Mock a failure during request
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ThrowsAsync(new InvalidOperationException("Invalid operation during HTTP request"));

        var httpClient = new HttpClient(handlerMock.Object);
        _mockHttpClientFactory.Setup(factory => factory.CreateClient(It.IsAny<string>())).Returns(httpClient);

        // Act & Assert
        await Assert.ThrowsAsync<ServiceCommunicationException>(() => _client.GetAsync(requestHeader));
    }

    [Fact]
    public async Task GetAsync_UnhandledException_ThrowsServiceCommunicationException()
    {
        // Arrange
        var requestHeader = new RequestHeaderModel { UserSessionId = "test-session-id" };

        // Mock an unhandled exception
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ThrowsAsync(new Exception("Unhandled exception"));

        _mockHttpClientFactory.Setup(x => x.CreateClient(HttpClientNames.PensionRetrievalService))
            .Returns(new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("http://localhost:1234")
            });

        // Act & Assert
        await Assert.ThrowsAsync<ServiceCommunicationException>(() => _client.GetAsync(requestHeader));
    }

    [Fact]
    public async Task GetAsync_EnsureSuccessStatusCode_Failure_ThrowsServiceCommunicationException()
    {
        // Arrange
        var requestHeader = new RequestHeaderModel { UserSessionId = "test-session-id" };

        // Mock a failed HTTP response
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest // Simulate failure
            });

        _mockHttpClientFactory.Setup(x => x.CreateClient(HttpClientNames.PensionRetrievalService))
            .Returns(new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("http://localhost:1234")
            });

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => _client.GetAsync(requestHeader));
    }

    [Fact]
    public async Task DeleteAsync_Request_ReturnsResult()
    {
        // Arrange
        var expectedCount = 2;

        var httpResponse = new HttpResponseMessage
        {
            Content = JsonContent.Create(expectedCount),
            StatusCode = HttpStatusCode.OK,
        };

        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(httpResponse);

        _mockHttpClientFactory.Setup(x => x.CreateClient(HttpClientNames.PensionRetrievalService))
            .Returns(new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("http://localhost:1234")
            });

        // Act
        await _client.DeleteAsync(new RequestHeaderModel { UserSessionId = "test-session-id", CorrelationId = "corr-Id" });

        // Assert
        handlerMock.Protected().Verify(
        "SendAsync",
        Times.Once(),
        ItExpr.Is<HttpRequestMessage>(req =>
            req.Method == HttpMethod.Delete &&
            req.Headers.Contains(HeaderConstants.UserSessionId) &&
            req.Headers.GetValues(HeaderConstants.UserSessionId).Single() == "test-session-id" &&
            req.Headers.Contains(HeaderConstants.CorrelationId) &&
            req.Headers.GetValues(HeaderConstants.CorrelationId).Single() == "corr-Id"
        ),
        ItExpr.IsAny<CancellationToken>()
    );
    }
}