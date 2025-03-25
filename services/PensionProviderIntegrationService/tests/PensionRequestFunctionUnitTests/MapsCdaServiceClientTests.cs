using MhpdCommon.Constants.HttpClient;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http.Json;
using MhpdCommon.Models.MHPDModels;
using MhpdCommon.Models.RequestHeaderModel;
using MhpdCommon.SharedHttpClient;

namespace PensionRequestFunctionUnitTests;

public class MapsCdaServiceClientTests
{
    private readonly Mock<IHttpClientFactory> _httpClientFactory = new();

    [Fact]
    public async Task WhenCdaClientIsInvoked_ModelDataIsReturned()
    {
        // Arrange
        var handler = CreateHttpHandler();
        _httpClientFactory.Setup(x => x.CreateClient(HttpClientNames.MapsCdaService))
                .Returns(new HttpClient(handler.Object)
                {
                    BaseAddress = new Uri("http://localhost:1234"!)
                });
        var logger = new Mock<ILogger<MapsCdaServiceClient>>();
        var client = new MapsCdaServiceClient(_httpClientFactory.Object, logger.Object);

        var request = new RequestHeaderModel
        {
            Iss = Guid.NewGuid().ToString(),
            UserSessionId = Guid.NewGuid().ToString()
        };

        // Act
        var result = await client.PostRqp(request);

        //Assert
        Assert.NotNull(result);
    }

    private static Mock<HttpMessageHandler> CreateHttpHandler()
    {
        var httpMessageHandlerMock = new Mock<HttpMessageHandler>();

        var response = new MapsRqpServiceResponseModel
        {
            Rqp = "JustARandomEncodedString.NothingToSeeHere"
        };

        httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()).ReturnsAsync(CreateHttpResponse(response));

        return httpMessageHandlerMock;
    }

    private static HttpResponseMessage CreateHttpResponse(MapsRqpServiceResponseModel? content = null)
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK);
        if (content != null)
            response.Content = JsonContent.Create(content);

        return response;
    }
}
