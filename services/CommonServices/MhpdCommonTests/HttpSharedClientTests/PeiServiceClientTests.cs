using MhpdCommon.Constants.HttpClient;
using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.Models.MHPDModels;
using MhpdCommon.SharedHttpClient;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http.Json;

namespace MhpdCommonTests.HttpSharedClientTests;

public class PeiServiceClientTests
{
    private readonly Mock<IHttpClientFactory> _httpClientFactory;
    private readonly Mock<ILogger<PeiServiceClient>> _logger;

    public PeiServiceClientTests()
    {
        _httpClientFactory = new Mock<IHttpClientFactory>();
        _logger = new Mock<ILogger<PeiServiceClient>>();
    }

    [Theory]
    [InlineData("Some", "", "Test", "Data")]
    [InlineData("Some", "Test", "\t", "Data")]
    [InlineData("Some", "Test", "Data", "    ")]
    [InlineData("Some", "\n", "Test", "Data")]
    public async Task WhenHttpClientIsExecutedWithInvalidParameters_ThrowsException(string rpt, string iss, string peisId, string userSessionId)
    {
        //Arrange
        var client = new PeiServiceClient(_httpClientFactory.Object, _logger.Object);

        //Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => client.GetPeiDataAsync(CreatePeiRequest(rpt, iss, peisId, userSessionId)));
    }

    [Theory]
    [InlineData(null, "Test", "Data")]
    [InlineData("Data", null, "Data")]
    [InlineData("Test", "Data", null)]
    public async Task WhenHttpClientIsExecutedWithNullParameters_ThrowsException(string iss, string peisId, string userSessionId)
    {
        //Arrange
        var client = new PeiServiceClient(_httpClientFactory.Object, _logger.Object);

        //Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => client.GetPeiDataAsync(CreatePeiRequest("rpt", iss, peisId, userSessionId)));
    }

    [Fact]
    private async Task WhenHttpClientIsExecutedWithPayload_ReturnsResponse()
    {
        //Arrange
        var handler = CreateHttpHandlerWithRetry();
        _httpClientFactory.Setup(x => x.CreateClient(HttpClientNames.PeiIntegrationService))
            .Returns(new HttpClient(handler.Object)
            {
                BaseAddress = new Uri("http://localhost:1234")
            });
        var client = new PeiServiceClient(_httpClientFactory.Object, _logger.Object);

        //Act
        var response = await client.GetPeiDataAsync(CreatePeiRequest("Some", "Sample", "Test", "Data"));

        //Assert
        handler.Protected().Verify("SendAsync", Times.Once(), ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());

        Assert.Single(response.Peis!);
    }

    private static Mock<HttpMessageHandler> CreateHttpHandlerWithRetry()
    {
        var httpMessageHandlerMock = new Mock<HttpMessageHandler>();

        var sequence = httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());

        var response = new CdaPeisServiceResponseModel
        {
            Peis = [
                    new PeiDataModel
                    {
                        Description = "Test",
                        Pei = Guid.NewGuid().ToString(),
                        RetrievalRequestedTimestamp = DateTime.UtcNow,
                        RetrievalStatus = "Started"
                    }
                ]
        };

        sequence.ReturnsAsync(CreateHttpResponse(HttpStatusCode.OK, response));

        return httpMessageHandlerMock;
    }

    private static PeiRequestModel CreatePeiRequest(string rpt, string iss, string peisId, string userSessionId)
    {
        return new PeiRequestModel
        {
            CorrelationId = Guid.NewGuid().ToString(),
            Iss = iss,
            PeisId = peisId,
            Rpt = rpt,
            UserSessionId = userSessionId
        };
    }

    private static HttpResponseMessage CreateHttpResponse(HttpStatusCode statusCode, CdaPeisServiceResponseModel content)
    {
        var response = new HttpResponseMessage(statusCode)
        {
            Content = JsonContent.Create(content)
        };

        return response;
    }
}
