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

namespace PensionsDataServiceUnitTests;

public class RetrievalRecordServiceClientTests
{
    private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
    private readonly Mock<ILogger<RetrievalRecordServiceClient>> _mockLogger;
    private readonly RetrievalRecordServiceClient _client;

    public RetrievalRecordServiceClientTests()
    {
        _mockHttpClientFactory = new Mock<IHttpClientFactory>();
        _mockLogger = new Mock<ILogger<RetrievalRecordServiceClient>>();
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
        _client = new RetrievalRecordServiceClient(_mockHttpClientFactory.Object, _mockLogger.Object);
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

        var client = new RetrievalRecordServiceClient(_mockHttpClientFactory.Object, _mockLogger.Object);
        
        // Act
        var result = await client.GetAsync(requestHeader);

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
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _client.GetAsync(requestHeader));
        Assert.Equal("An invalid operation occurred during retrieval record function communication", exception.Message);
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
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _client.GetAsync(requestHeader));
        Assert.Equal("An invalid operation occurred during retrieval record function communication", exception.Message);
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
        
        var client = new RetrievalRecordServiceClient(_mockHttpClientFactory.Object, _mockLogger.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServiceCommunicationException>(() => client.GetAsync(requestHeader));
        Assert.Equal("An unexpected error occurred during retrieval record function communication", exception.Message);
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
        
        var client = new RetrievalRecordServiceClient(_mockHttpClientFactory.Object, _mockLogger.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServiceCommunicationException>(() => client.GetAsync(requestHeader));
        Assert.Equal("Error communicating with retrieval record endpoint", exception.Message);
    }
}