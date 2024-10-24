using System.Net;
using System.Net.Http.Json;
using MhpdCommon.Constants.HttpClient;
using MhpdCommon.CustomExceptions;
using MhpdCommon.Models.MHPDModels;
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

    public RetrievedPensionsRecordClientTests()
    {
        _mockHttpClientFactory = new Mock<IHttpClientFactory>();
        _mockLogger = new Mock<ILogger<RetrievedPensionsRecordClient>>();
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
        _client = new RetrievedPensionsRecordClient(_mockHttpClientFactory.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetAsync_SuccessfulRequest_ReturnsOkResult()
    {
        // Arrange
        var request = new PensionsRetrievalRecordIdModel { PensionsRetrievalRecordId = "test-session-id" };
        
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
        var result = await client.GetAsync(request);

        // Assert
        Assert.IsType<List<RetrievedPensionRecord>>(result);
    }

    [Fact]
    public async Task GetAsync_HttpRequestException_ThrowsServiceCommunicationException()
    {
        // Arrange
        var request = new PensionsRetrievalRecordIdModel { PensionsRetrievalRecordId = "test-session-id" };

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
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _client.GetAsync(request));
        Assert.Equal("An invalid operation occurred during retrieved record service communication", exception.Message);
    }

    [Fact]
    public async Task GetAsync_InvalidOperationDuringRequest_ThrowsInvalidOperationException()
    {
        // Arrange
        var request = new PensionsRetrievalRecordIdModel { PensionsRetrievalRecordId = "test-session-id" };

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
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _client.GetAsync(request));
        Assert.Equal("An invalid operation occurred during retrieved record service communication", exception.Message);
    }

    [Fact]
    public async Task GetAsync_UnhandledException_ThrowsServiceCommunicationException()
    {
        // Arrange
        var request = new PensionsRetrievalRecordIdModel { PensionsRetrievalRecordId = "test-session-id" };

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

        _mockHttpClientFactory.Setup(x => x.CreateClient(HttpClientNames.RetrievedPensionsService))
            .Returns(new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("http://localhost:1234")
            });
        
        var client = new RetrievedPensionsRecordClient(_mockHttpClientFactory.Object, _mockLogger.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServiceCommunicationException>(() => client.GetAsync(request));
        Assert.Equal("An unexpected error occurred during retrieved record service communication", exception.Message);
    }

    [Fact]
    public async Task GetAsync_EnsureSuccessStatusCode_Failure_ThrowsServiceCommunicationException()
    {
        // Arrange
        var request = new PensionsRetrievalRecordIdModel { PensionsRetrievalRecordId = "test-session-id" };

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

        _mockHttpClientFactory.Setup(x => x.CreateClient(HttpClientNames.RetrievedPensionsService))
            .Returns(new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("http://localhost:1234")
            });
        
        var client = new RetrievedPensionsRecordClient(_mockHttpClientFactory.Object, _mockLogger.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServiceCommunicationException>(() => client.GetAsync(request));
        Assert.Equal("Error communicating with retrieved record endpoint", exception.Message);
    }
}