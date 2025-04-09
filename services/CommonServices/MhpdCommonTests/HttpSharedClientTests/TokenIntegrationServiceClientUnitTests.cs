using MhpdCommon.Constants.HttpClient;
using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.SharedHttpClient;
using MhpdCommon.TokenValidation;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http.Json;

namespace MhpdCommonTests.HttpSharedClientTests;

public class TokenIntegrationServiceClientUnitTests
{
    private readonly TokenIntegrationServiceClient _sut;
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock = new();

    public TokenIntegrationServiceClientUnitTests()
    {
        var logger = new Mock<ILogger<TokenIntegrationServiceClient>>();

        _sut = new TokenIntegrationServiceClient(logger.Object, _httpClientFactoryMock.Object);
    }

    [Fact]
    public async void When_AccessToken_IsRequested_It_Should_Return_Response()
    {
        // Arrange
        var request = new TokenClientRequestModel
        {
            Rqp = TokenQueryParams.ValidJwtToken,
            AsUri = "http://localhost:YYYY",
            Ticket = TokenQueryParams.ValidJweToken
        };

        var handler = CreateHttpHandler(false);

        _httpClientFactoryMock.Setup(x => x.CreateClient(HttpClientNames.TokenIntegrationService))
            .Returns(new HttpClient(handler.Object)
            {
                BaseAddress = new Uri("http://localhost:1234")
            });

        // Act
        var result = await _sut.PostAccessTokenAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result.AccessToken));
    }

    [Fact]
    public async void When_IdToken_IsRequested_It_Should_Return_Response()
    {
        // Arrange
        var request = new PensionsDataRequestModel
        {
            AuthorisationCode = "CodeA",
            ClientId = TokenQueryParams.ValidClientId,
            ClientSecret = TokenQueryParams.ValidClientSecret,
            CodeVerifier = TokenQueryParams.ValidCodeVerifier,
            RedirectUrl = "http://localhost:XXXX"
        };

        var handler = CreateHttpHandler(true);

        _httpClientFactoryMock.Setup(x => x.CreateClient(HttpClientNames.TokenIntegrationService))
            .Returns(new HttpClient(handler.Object)
            {
                BaseAddress = new Uri("http://localhost:1234")
            });

        // Act
        var result = await _sut.PostIdTokenAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result.IdToken));
    }

    private static Mock<HttpMessageHandler> CreateHttpHandler(bool isForIdToken)
    {
        var response = new CdaTokenResponseModel
        {
            AccessToken = isForIdToken ? string.Empty : TokenQueryParams.ValidJwtToken,
            IdToken = isForIdToken ? TokenQueryParams.ValidJwtToken : string.Empty,
            Pct = isForIdToken ? string.Empty : TokenQueryParams.ValidPersistedClaimsToken,
            StatusCode = HttpStatusCode.OK
        };

        var httpResponse = new HttpResponseMessage
        {
            Content = JsonContent.Create(response),
            StatusCode = HttpStatusCode.OK,
        };

        var httpMessageHandlerMock = new Mock<HttpMessageHandler>();

        httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse); ;

        return httpMessageHandlerMock;
    }
}
