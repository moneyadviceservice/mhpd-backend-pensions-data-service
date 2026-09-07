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
using PensionsDataService.Models;
using System.Net;
using System.Net.Http.Json;

namespace PensionsDataServiceUnitTests;

public class RetrievedPensionsRecordClientTests
{
    private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
    private readonly Mock<ILogger<RetrievedPensionsRecordClient>> _mockLogger;
    private readonly Mock<IErrorResolver> _mockErrorResolver;
    private readonly RetrievedPensionsRecordClient _client;
    private readonly RequestHeaderModel _requestHeaderModel;

    public RetrievedPensionsRecordClientTests()
    {
        _mockHttpClientFactory = new Mock<IHttpClientFactory>();
        _mockLogger = new Mock<ILogger<RetrievedPensionsRecordClient>>();
        _mockErrorResolver = new Mock<IErrorResolver>();
        Mock<IConfiguration> mockConfiguration = new();
        _requestHeaderModel = new RequestHeaderModel
        {
            CorrelationId = Guid.NewGuid().ToString(),
            UserSessionId = Guid.NewGuid().ToString(),
        };

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
        _client = new RetrievedPensionsRecordClient(_mockHttpClientFactory.Object, _mockLogger.Object, _mockErrorResolver.Object);
    }

    [Fact]
    public async Task GetAsync_SuccessfulRequest_ReturnsOkResult()
    {
        // Arrange
        var expectedRecords = new List<RetrievedPensionRecord>();
        
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

        _mockHttpClientFactory.Setup(x => x.CreateClient(HttpClientNames.RetrievedPensionsService))
            .Returns(new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("http://localhost:1234")
            });
        
        // Act
        var result = await _client.GetRetrievedPensionsAsync(new RetrievedPensionsRequest(), _requestHeaderModel);

        // Assert
        Assert.IsType<List<RetrievedPensionRecord>>(result);
    }

    [Fact]
    public async Task GetPeisAsync_SuccessfulRequest_ReturnsOkResult()
    {
        // Arrange
        var expectedPeis = new List<string>();

        var httpResponse = new HttpResponseMessage
        {
            Content = JsonContent.Create(expectedPeis),
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

        _mockHttpClientFactory.Setup(x => x.CreateClient(HttpClientNames.RetrievedPensionsService))
            .Returns(new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("http://localhost:1234")
            });

        // Act
        var result = await _client.GetRetrievedPeisAsync(_requestHeaderModel);

        // Assert
        Assert.IsType<List<string>>(result);
    }

    [Fact]
    public async Task GetAsync_HttpRequestException_ThrowsServiceCommunicationException()
    {
        // Arrange
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
        var exception = await Assert.ThrowsAsync<ServiceCommunicationException>(() => _client.GetRetrievedPensionsAsync(new RetrievedPensionsRequest(), _requestHeaderModel));
    }

    [Fact]
    public async Task GetAsync_InvalidOperationDuringRequest_ThrowsInvalidOperationException()
    {
        // Arrange
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
        await Assert.ThrowsAsync<ServiceCommunicationException>(() => _client.GetRetrievedPensionsAsync(new RetrievedPensionsRequest(), _requestHeaderModel));
    }

    [Fact]
    public async Task GetAsync_UnhandledException_ThrowsServiceCommunicationException()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ThrowsAsync(new Exception("Unhandled exception"));

        _mockHttpClientFactory.Setup(x => x.CreateClient(HttpClientNames.RetrievedPensionsService))
            .Returns(new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("http://localhost:1234")
            });

        // Act & Assert
        await Assert.ThrowsAsync<ServiceCommunicationException>(() => _client.GetRetrievedPensionsAsync(new RetrievedPensionsRequest(), _requestHeaderModel));
    }

    [Fact]
    public async Task GetAsync_EnsureSuccessStatusCode_Failure_ThrowsServiceCommunicationException()
    {
        // Arrange
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

        _mockHttpClientFactory.Setup(x => x.CreateClient(HttpClientNames.RetrievedPensionsService))
            .Returns(new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("http://localhost:1234")
            });

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => _client.GetRetrievedPensionsAsync(new RetrievedPensionsRequest(), _requestHeaderModel));
    }

    [Fact]
    public async Task DeleteAsync_Request_ReturnsResult()
    {
        // Arrange

        var httpResponse = new HttpResponseMessage
        {
            Content = JsonContent.Create(0),
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

        _mockHttpClientFactory.Setup(x => x.CreateClient(HttpClientNames.RetrievedPensionsService))
            .Returns(new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("http://localhost:1234")
            });

        // Act
        await _client.DeleteAsync(new RequestHeaderModel { UserSessionId = "user-session-Id", CorrelationId = "correlation-Id" });

        // Assert
        handlerMock.Protected().Verify(
        "SendAsync",
        Times.Once(),
        ItExpr.Is<HttpRequestMessage>(req =>
            req.Method == HttpMethod.Delete &&
            req.Headers.Contains(HeaderConstants.UserSessionId) &&
            req.Headers.GetValues(HeaderConstants.UserSessionId).Single() == "user-session-Id" &&
            req.Headers.Contains(HeaderConstants.CorrelationId) &&
            req.Headers.GetValues(HeaderConstants.CorrelationId).Single() == "correlation-Id"
        ),
        ItExpr.IsAny<CancellationToken>()
    );
    }
}