using MhpdCommon.Constants.HttpClient;
using Moq;
using Moq.Protected;
using PensionRequestFunction.HttpClient.Implementation;
using PensionRequestFunction.Models.MapsRqpServiceClient;
using System.Net;
using System.Net.Http.Json;

namespace PensionRequestFunctionUnitTests;

public class MapsCdaServiceClientTests
{
    private readonly Mock<IHttpClientFactory> _httpClientFactory;

    public MapsCdaServiceClientTests()
    {
        _httpClientFactory = new Mock<IHttpClientFactory>();
    }

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
        var client = new MapsCdaServiceClient(_httpClientFactory.Object);

        var request = new MapsRqpServiceRequestModel
        {
            Iss = Guid.NewGuid().ToString(),
            UserSessionId = Guid.NewGuid().ToString()
        };

        // Act
        var result = await client.PostRqpAsync(request);

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
