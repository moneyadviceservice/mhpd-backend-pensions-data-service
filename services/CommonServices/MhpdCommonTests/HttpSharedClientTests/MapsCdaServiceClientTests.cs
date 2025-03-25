using System.Net;
using System.Net.Http.Json;
using MhpdCommon.CustomExceptions;
using MhpdCommon.Models.MHPDModels;
using MhpdCommon.Models.RequestHeaderModel;
using MhpdCommon.SharedHttpClient;
using MhpdCommon.TokenValidation;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;

namespace MhpdCommonTests.HttpSharedClientTests;

public class MapsCdaServiceClientTests
{
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly Mock<ILogger<MapsCdaServiceClient>> _loggerMock;
    private readonly MapsCdaServiceClient _serviceClient;
    private readonly HttpClient _httpClient;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;

    public MapsCdaServiceClientTests()
    {
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _loggerMock = new Mock<ILogger<MapsCdaServiceClient>>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("https://test-api.com")
        };
        
        _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(_httpClient);
        _serviceClient = new MapsCdaServiceClient(_httpClientFactoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task PostRqp_ShouldReturnValidResponse_OnSuccess()
    {
        // Arrange
        var expectedResponse = new MapsRqpServiceResponseModel
        {
            Rqp = TokenQueryParams.ValidJwtToken
        };
        
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(expectedResponse)
            });

        // Act
        var result = await _serviceClient.PostRqp(new RequestHeaderModel());

        // Assert
        Assert.NotNull(result);
        Assert.IsType<MapsRqpServiceResponseModel>(result);
        Assert.Equal(TokenQueryParams.ValidJwtToken, result.Rqp);
    }

    [Fact]
    public async Task PostRqp_ShouldThrowServiceCommunicationException_OnHttpRequestException()
    {
        // Arrange
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act & Assert
        await Assert.ThrowsAsync<ServiceCommunicationException>(() => _serviceClient.PostRqp(new RequestHeaderModel()));
    }

    [Fact]
    public async Task PostRqp_ShouldThrowInvalidOperationException_WhenResponseIsNull()
    {
        // Arrange
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(string.Empty) // Simulating empty response
            });

        // Act & Assert
        await Assert.ThrowsAsync<ServiceCommunicationException>(() => _serviceClient.PostRqp(new RequestHeaderModel()));
    }
}