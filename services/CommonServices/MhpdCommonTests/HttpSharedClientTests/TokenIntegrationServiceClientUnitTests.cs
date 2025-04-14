using MhpdCommon.Constants.HttpClient;
using MhpdCommon.CustomExceptions;
using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.Models.MHPDModels;
using MhpdCommon.SharedHttpClient;
using MhpdCommon.TokenValidation;
using MhpdCommonTests.TokenValidationTests;
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
    private const string PeisId = "123e4567-e89b-12d3-a456-42661417400";
    private const string TokenIntegrationServicesEndpointUrl = "https://localhost/";

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
                BaseAddress = new Uri(TokenIntegrationServicesEndpointUrl)
            });

        // Act
        var result = await _sut.PostAccessTokenAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result.AccessToken));
    }

    [Fact]
    public async void When_PeisId_IsRequested_It_Should_Return_Response()
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
        Assert.False(string.IsNullOrWhiteSpace(result.PeisId));
    }

    private static Mock<HttpMessageHandler> CreateHttpHandler(bool isForIdToken)
    {
        var rptsResponse = new CdaTokenResponseModel
        {
            AccessToken = TokenQueryParams.ValidJwtToken,
            Pct = TokenQueryParams.ValidPersistedClaimsToken,
            StatusCode = HttpStatusCode.OK
        };

        var peisResponse = new PeiRetrievalDetailsResponseModel
        {
            PeisId = TokenQueryParams.ValidJwtToken
        };

        var httpResponse = new HttpResponseMessage
        {
            Content = isForIdToken? JsonContent.Create(peisResponse) : JsonContent.Create(rptsResponse),
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

    [Fact]
    public async Task PostRpt_Should_Return_Response_When_Successful()
    {
        var request = new PensionsDataRequestModel
        {
            ClientId = TokenQueryParams.ValidClientId,
            ClientSecret = TokenQueryParams.ValidClientSecret,
            AuthorisationCode = TokenQueryParams.ValidCode,
            RedirectUrl = Helper.ValidRedirectUri,
            CodeVerifier = TokenQueryParams.ValidCodeVerifier
        };

        var response = new HttpResponseMessage
        {
            Content = JsonContent.Create(new PeiRetrievalDetailsResponseModel { PeisId = PeisId }),
            StatusCode = HttpStatusCode.OK,
        };

        // Mock SendAsync method of the HttpMessageHandler to return the mocked response
        var handlerMoq = new Mock<HttpMessageHandler>();

        handlerMoq.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        _httpClientFactoryMock.Setup(x => x.CreateClient(HttpClientNames.TokenIntegrationService))
            .Returns(new HttpClient(handlerMoq.Object)
            {
                BaseAddress = new Uri(TokenIntegrationServicesEndpointUrl)
            });

        var result = await _sut.PostIdTokenAsync(request, It.IsAny<string>());

        // Asserting the result is not null and is of the correct type
        Assert.NotNull(result);
        Assert.IsType<PeiRetrievalDetailsResponseModel>(result);
        Assert.Equal(PeisId, result.PeisId);  // Further verification of content
    }

    [Fact]
    public async Task PostRpt_Should_Return_Response_When_PeisId_Is_Present_Successful()
    {
        var request = new PensionsDataRequestModel
        {
            ClientId = TokenQueryParams.ValidClientId,
            ClientSecret = TokenQueryParams.ValidClientSecret,
            AuthorisationCode = TokenQueryParams.ValidCode,
            RedirectUrl = Helper.ValidRedirectUri,
            CodeVerifier = TokenQueryParams.ValidCodeVerifier
        };

        var response = new HttpResponseMessage
        {
            Content = JsonContent.Create(new PeiRetrievalDetailsResponseModel { PeisId = PeisId }),
            StatusCode = HttpStatusCode.OK,
        };

        // Mock SendAsync method of the HttpMessageHandler to return the mocked response
        var handlerMoq = new Mock<HttpMessageHandler>();

        handlerMoq.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        _httpClientFactoryMock.Setup(x => x.CreateClient(HttpClientNames.TokenIntegrationService))
            .Returns(new HttpClient(handlerMoq.Object)
            {
                BaseAddress = new Uri(TokenIntegrationServicesEndpointUrl)
            });

        var result = await _sut.PostIdTokenAsync(request, It.IsAny<string>());

        // Asserting the result is not null and is of the correct type
        Assert.NotNull(result);
        Assert.IsType<PeiRetrievalDetailsResponseModel>(result);
        Assert.Equal(PeisId, result.PeisId);  // Further verification of content
    }

    [Fact]
    public async Task PostRpt_Should_Throw_ServiceCommunicationException_When_HttpRequestException_Occurs()
    {
        var request = new PensionsDataRequestModel();

        // Simulate HttpRequestException
        var handlerMoq = new Mock<HttpMessageHandler>();

        handlerMoq.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        _httpClientFactoryMock.Setup(x => x.CreateClient(HttpClientNames.TokenIntegrationService))
            .Returns(new HttpClient(handlerMoq.Object)
            {
                BaseAddress = new Uri(TokenIntegrationServicesEndpointUrl)
            });

        var ex = await Assert.ThrowsAsync<ServiceCommunicationException>(async () =>
            await _sut.PostIdTokenAsync(request, It.IsAny<string>())
        );

        Assert.IsType<HttpRequestException>(ex.InnerException);
    }

    [Fact]
    public async Task PostRpt_Should_Throw_InvalidOperationException_When_Response_Content_Is_Null()
    {
        var request = new PensionsDataRequestModel();

        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("") // Content is empty
        };

        var handlerMoq = new Mock<HttpMessageHandler>();

        handlerMoq.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        _httpClientFactoryMock.Setup(x => x.CreateClient(HttpClientNames.TokenIntegrationService))
            .Returns(new HttpClient(handlerMoq.Object)
            {
                BaseAddress = new Uri(TokenIntegrationServicesEndpointUrl)
            });

        var ex = await Assert.ThrowsAsync<ServiceCommunicationException>(async () =>
            await _sut.PostIdTokenAsync(request, It.IsAny<string>())
        );

        Assert.IsType<System.Text.Json.JsonException>(ex.InnerException);
    }

    [Fact]
    public async Task PostRpt_Should_Throw_ServiceCommunicationException_When_Unexpected_Exception_Occurs()
    {
        var request = new PensionsDataRequestModel();

        // Simulate a generic exception
        var handlerMoq = new Mock<HttpMessageHandler>();

        handlerMoq.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new Exception("Unexpected error"));

        _httpClientFactoryMock.Setup(x => x.CreateClient(HttpClientNames.TokenIntegrationService))
            .Returns(new HttpClient(handlerMoq.Object)
            {
                BaseAddress = new Uri(TokenIntegrationServicesEndpointUrl)
            });

        var ex = await Assert.ThrowsAsync<ServiceCommunicationException>(async () =>
            await _sut.PostIdTokenAsync(request, It.IsAny<string>())
        );

        Assert.IsType<Exception>(ex.InnerException);
    }
}
