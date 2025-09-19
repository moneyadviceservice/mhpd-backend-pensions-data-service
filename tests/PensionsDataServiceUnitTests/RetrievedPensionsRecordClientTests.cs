using System.Net;
using System.Net.Http.Json;
using MhpdCommon.Constants.HttpClient;
using MhpdCommon.CustomExceptions;
using MhpdCommon.Models.MHPDModels;
using MhpdCommon.Models.RequestHeaderModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using PensionsDataService.HttpClients;
using PensionsDataService.Models;

namespace PensionsDataServiceUnitTests;

public class RetrievedPensionsRecordClientTests
{
    private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
    private readonly Mock<ILogger<RetrievedPensionsRecordClient>> _mockLogger;
    private readonly RetrievedPensionsRecordClient _client;
    private readonly RequestHeaderModel _requestHeaderModel;

    public RetrievedPensionsRecordClientTests()
    {
        _mockHttpClientFactory = new Mock<IHttpClientFactory>();
        _mockLogger = new Mock<ILogger<RetrievedPensionsRecordClient>>();
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
        _client = new RetrievedPensionsRecordClient(_mockHttpClientFactory.Object, _mockLogger.Object);
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

        var client = new RetrievedPensionsRecordClient(_mockHttpClientFactory.Object, _mockLogger.Object);
        
        // Act
        var result = await client.GetRetrievedPensionsAsync(new RetrievedPensionsRequest(), _requestHeaderModel);

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

        var client = new RetrievedPensionsRecordClient(_mockHttpClientFactory.Object, _mockLogger.Object);

        // Act
        var result = await client.GetRetrievedPeisAsync(_requestHeaderModel);

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
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _client.GetRetrievedPensionsAsync(new RetrievedPensionsRequest(), _requestHeaderModel));
        Assert.Equal("An invalid operation occurred during retrieved pensions service communication", exception.Message);
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
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _client.GetRetrievedPensionsAsync(new RetrievedPensionsRequest(), _requestHeaderModel));
        Assert.Equal("An invalid operation occurred during retrieved pensions service communication", exception.Message);
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
        
        var client = new RetrievedPensionsRecordClient(_mockHttpClientFactory.Object, _mockLogger.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServiceCommunicationException>(() => client.GetRetrievedPensionsAsync(new RetrievedPensionsRequest(), _requestHeaderModel));
        Assert.Equal("An unexpected error occurred during retrieved pensions service communication", exception.Message);
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
        
        var client = new RetrievedPensionsRecordClient(_mockHttpClientFactory.Object, _mockLogger.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServiceCommunicationException>(() => client.GetRetrievedPensionsAsync(new RetrievedPensionsRequest(), _requestHeaderModel));
        Assert.Equal("Error communicating with retrieved record endpoint", exception.Message);
    }

    [Fact]
    public async Task DeleteAsync_Request_ReturnsResult()
    {
        // Arrange
        var expectedCount = 4;

        var httpResponse = new HttpResponseMessage
        {
            Content = JsonContent.Create(expectedCount),
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

        var client = new RetrievedPensionsRecordClient(_mockHttpClientFactory.Object, _mockLogger.Object);

        // Act
        var result = await client.DeleteAsync("user-session-Id", "correlation-Id");

        // Assert
        Assert.Equal(expectedCount, result);
    }
}