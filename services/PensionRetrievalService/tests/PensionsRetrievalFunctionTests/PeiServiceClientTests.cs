using MhpdCommon.Constants.HttpClient;
using Moq;
using Moq.Protected;
using PensionsRetrievalFunction.HttpClients;
using PensionsRetrievalFunction.Models;
using System.Net;
using System.Net.Http.Json;

namespace PensionsRetrievalFunctionTests;
public class PeiServiceClientTests
{
    private readonly Mock<IHttpClientFactory> _httpClientFactory;

    public PeiServiceClientTests()
    {
        _httpClientFactory = new Mock<IHttpClientFactory>();
    }

    [Theory]
    [InlineData("", "Test", "Data")]
    [InlineData("Test", "\t", "Data")]
    [InlineData("Test", "Data", "    ")]
    [InlineData("\n", "Test", "Data")]
    public async Task WhenHttpClientIsExecutedWithInvalidParameters_ThrowsException(string iss, string peisId, string userSessionId)
    {
        //Arrange
        var client = new PeiServiceClient(_httpClientFactory.Object);

        //Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => client.GetPeiDataAsync(null, iss, peisId, userSessionId));
    }

    [Theory]
    [InlineData(null, "Test", "Data")]
    [InlineData("Data", null, "Data")]
    [InlineData("Test", "Data", null)]
    public async Task WhenHttpClientIsExecutedWithNullParameters_ThrowsException(string iss, string peisId, string userSessionId)
    {
        //Arrange
        var client = new PeiServiceClient(_httpClientFactory.Object);

        //Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => client.GetPeiDataAsync("rpt", iss, peisId, userSessionId));
    }

    [Theory]
    [InlineData(true, 1)]
    [InlineData(false, 0)]
    private async Task WhenHttpClientIsExecutedWithPayload_ReturnsResponse(bool success, int expectedPeiCount)
    {
        //Arrange
        var handler = CreateHttpHandlerWithRetry(success);
        _httpClientFactory.Setup(x => x.CreateClient(HttpClientNames.PeiIntegrationService))
            .Returns(new HttpClient(handler.Object)
            {
                BaseAddress = new Uri("http://localhost:1234")
            });
        var client = new PeiServiceClient(_httpClientFactory.Object);

        //Act
        var response = await client.GetPeiDataAsync("Some", "Sample", "Test", "Data");

        //Assert
        handler.Protected().Verify("SendAsync", Times.Once(), ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());

        Assert.Equal(expectedPeiCount, response.PeiData.Count);
    }

    private static Mock<HttpMessageHandler> CreateHttpHandlerWithRetry(bool success = false)
    {
        var httpMessageHandlerMock = new Mock<HttpMessageHandler>();

        var sequence = httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());

        if (success)
        {
            var response = new List<PeiData>
            {
                new() {
                    Description = "Test",
                    Pei = Guid.NewGuid().ToString(),
                    RetrievalRequestedTimestamp = DateTime.UtcNow,
                    RetrievalStatus = "Started"
                }
            };

            sequence.ReturnsAsync(CreateHttpResponse(HttpStatusCode.OK, response));
        }
        else
        {
            sequence.ReturnsAsync(CreateHttpResponse(HttpStatusCode.InternalServerError));
        }

        return httpMessageHandlerMock;
    }

    private static HttpResponseMessage CreateHttpResponse(HttpStatusCode statusCode, List<PeiData>? content = null)
    {
        var response = new HttpResponseMessage(statusCode);
        if (content != null)
            response.Content = JsonContent.Create(content);

        return response;
    }
}
