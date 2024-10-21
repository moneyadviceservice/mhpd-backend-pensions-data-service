using MhpdCommon.Constants.HttpClient;
using MhpdCommon.Models.Configuration;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using PensionRequestFunction.HttpClient.Implementation;
using PensionRequestFunction.Models.TokenIntegrationServiceClient;
using System.Net;
using System.Net.Http.Json;

namespace PensionRequestFunctionUnitTests;

public class TokenIntegrationServiceClientTests
{
    private readonly Mock<IHttpClientFactory> _httpClientFactory;

    public TokenIntegrationServiceClientTests()
    {
        _httpClientFactory = new Mock<IHttpClientFactory>();
    }

    [Fact]
    public async Task WhenTokenClientIsInvoked_ModelDataIsReturned()
    {
        // Arrange
        var config = new CommonHttpConfiguration
        {
            CdaServiceUrl = "https://cda.service.com"
        };

        var options = Options.Create(config);

        var handler = CreateHttpHandler();
        _httpClientFactory.Setup(x => x.CreateClient(HttpClientNames.TokenIntegrationService))
                .Returns(new HttpClient(handler.Object)
                {
                    BaseAddress = new Uri("http://localhost:1234"!)
                });
        var client = new TokenIntegrationServiceClient(_httpClientFactory.Object, options);

        var request = new TokenIntegrationServiceRequestModel
        {
            As_Uri = "https://url.auth.com",
            Rqp = "RandomString",
            Ticket = "ticketValue"
        };

        // Act
        var result = await client.PostRptAsync(request);

        //Assert
        Assert.NotNull(result);
    }

    private static Mock<HttpMessageHandler> CreateHttpHandler()
    {
        var httpMessageHandlerMock = new Mock<HttpMessageHandler>();

        var response = new TokenIntegrationResponseModel
        {
            Rpt = "JustARandomEncodedString.NothingToSeeHere"
        };

        httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()).ReturnsAsync(CreateHttpResponse(response));

        return httpMessageHandlerMock;
    }

    private static HttpResponseMessage CreateHttpResponse(TokenIntegrationResponseModel? content = null)
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK);
        if (content != null)
            response.Content = JsonContent.Create(content);

        return response;
    }
}
