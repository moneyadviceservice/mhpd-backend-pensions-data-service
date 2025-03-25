using MhpdCommon.Constants.HttpClient;
using MhpdCommon.Models.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http.Json;
using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.SharedHttpClient;

namespace PensionRequestFunctionUnitTests;

public class TokenIntegrationServiceClientTests
{
    private readonly Mock<IHttpClientFactory> _httpClientFactory = new();

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
                    BaseAddress = new Uri("http://localhost:1234")
                });
        var logger = new Mock<ILogger<TokenIntegrationServiceClient>>();
        var client = new TokenIntegrationServiceClient(logger.Object, _httpClientFactory.Object);

        var request = new TokenClientRequestModel
        {
            AsUri = "https://url.auth.com",
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

        var response = new CdaTokenResponseModel
        {
            AccessToken = "JustARandomEncodedString.NothingToSeeHere"
        };

        httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()).ReturnsAsync(CreateHttpResponse(response));

        return httpMessageHandlerMock;
    }

    private static HttpResponseMessage CreateHttpResponse(CdaTokenResponseModel? content = null)
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK);
        if (content != null)
            response.Content = JsonContent.Create(content);

        return response;
    }
}
