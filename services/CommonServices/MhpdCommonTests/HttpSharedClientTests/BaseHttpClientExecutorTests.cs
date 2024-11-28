using System.Net;
using MhpdCommon.CustomExceptions;
using MhpdCommon.SharedHttpClient;
using Microsoft.Extensions.Logging;
using Moq;

namespace MhpdCommonTests.HttpSharedClientTests;

public class BaseHttpClientExecutorTests
{
    private readonly Mock<IHttpClientFactory> httpClientFactoryMock = new();
    private readonly Mock<ILogger<BaseHttpClientExecutor>> loggerMock = new();

    [Fact]
    public async Task ExecuteAsync_SuccessfulResponse_ReturnsDeserializedResult()
    {
        // Arrange
        var expectedResponse = new { Message = "Success" };
        var mockHttpMessageHandler = new MockHttpMessageHandler();
        mockHttpMessageHandler.SetupResponse(HttpStatusCode.OK, expectedResponse);

        var httpClient = new HttpClient(mockHttpMessageHandler)
        {
            BaseAddress = new Uri("https://mock.api")
        };

        httpClientFactoryMock
            .Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(httpClient);

        var executor = new TestHttpClientExecutor(httpClientFactoryMock.Object, loggerMock.Object);

        // Act
        var result = await executor.TestExecuteAsync<ApiResponse>("TestOperation");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Success", result.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HttpRequestException_ThrowsServiceCommunicationException()
    {
        // Arrange
        var mockHttpMessageHandler = new MockHttpMessageHandler();
        mockHttpMessageHandler.SetupResponse(HttpStatusCode.InternalServerError);

        var httpClient = new HttpClient(mockHttpMessageHandler)
        {
            BaseAddress = new Uri("https://mock.api")
        };

        httpClientFactoryMock
            .Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(httpClient);

        var executor = new TestHttpClientExecutor(httpClientFactoryMock.Object, loggerMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServiceCommunicationException>(
            () => executor.TestExecuteAsync<object>("TestOperation"));

        Assert.Contains("Error during TestOperation", exception.Message);
    }

    [Fact]
    public async Task ExecuteAsync_NullResponseContent_ThrowsInvalidOperationException()
    {
        // Arrange
        var mockHttpMessageHandler = new MockHttpMessageHandler();
        mockHttpMessageHandler.SetupResponse(HttpStatusCode.OK, (object)null);

        var httpClient = new HttpClient(mockHttpMessageHandler)
        {
            BaseAddress = new Uri("https://mock.api")
        };

        httpClientFactoryMock
            .Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(httpClient);

        var executor = new TestHttpClientExecutor(httpClientFactoryMock.Object, loggerMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServiceCommunicationException>(
            () => executor.TestExecuteAsync<object>("TestOperation"));

        Assert.Contains("An unexpected error occurred during TestO", exception.Message);
    }
}