using MhpdCommon.Constants.HttpClient;
using MhpdCommon.Models.MHPDModels;
using Moq;
using Moq.Protected;
using PensionRequestFunction.HttpClient;
using System.Net;
using System.Net.Http.Json;

namespace PensionRequestFunctionUnitTests;

public class HolderNameClientTests
{
    private readonly Mock<IHttpClientFactory> _httpClientFactory;

    public HolderNameClientTests()
    {
        _httpClientFactory = new Mock<IHttpClientFactory>();
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(1, false)]
    [InlineData(2, true)]
    public async Task WhenHolderNameClientIsInvoked_ModelDataIsReturned(int size, bool isNullExpected)
    {
        // Arrange
        var handler = CreateHttpHandler(size);
        _httpClientFactory.Setup(x => x.CreateClient(HttpClientNames.CdaService))
                .Returns(new HttpClient(handler.Object)
                {
                    BaseAddress = new Uri("http://localhost:1234"!)
                });
        var client = new HolderNameClient(_httpClientFactory.Object);

        // Act
        var result = await client.GetViewDataUrlAsync(Guid.NewGuid().ToString());

        //Assert
        Assert.True(isNullExpected ? result == null : result != null);
    }

    private static Mock<HttpMessageHandler> CreateHttpHandler(int responseSize)
    {
        var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        var configurations = new List<HolderNameConfigurationModel>();
        var start = 0;
        while (start++ < responseSize)
        {
            configurations.Add(new()
            {
                HolderNameGuid = Guid.NewGuid().ToString(),
                Id = Guid.NewGuid().ToString(),
                ViewDataUrl = "https://viewdata.pdp.com"
            });
        }

        var response = new HolderNameViewDataResponse
        {
            Configurations = configurations
        };

        httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()).ReturnsAsync(CreateHttpResponse(HttpStatusCode.OK, response));

        return httpMessageHandlerMock;
    }

    private static HttpResponseMessage CreateHttpResponse(HttpStatusCode statusCode, HolderNameViewDataResponse? content = null)
    {
        var response = new HttpResponseMessage(statusCode);
        if (content != null)
            response.Content = JsonContent.Create(content);

        return response;
    }
}
