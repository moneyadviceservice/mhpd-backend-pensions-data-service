using System.Net;
using System.Net.Http.Json;
using MhpdCommon.Constants.HttpClient;
using MhpdCommon.CustomExceptions;
using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.Models.RequestHeaderModel;
using MhpdCommon.TokenValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using PensionsDataService.HttpClients;
using PensionsDataService.Models;

namespace PensionsDataServiceUnitTests;

public class TokenIntegrationServiceClientTests
{
    private readonly TokenIntegrationServiceClient _sut;
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock = new();
    private readonly Mock<HttpMessageHandler> _handlerMoq = new();
    private readonly Mock<ILogger<TokenIntegrationServiceClient>> _logger = new();
    private const string TokenIntegrationServicesEndpointUrl = "https://localhost/";
    private const string PeisId = "123e4567-e89b-12d3-a456-42661417400";

    public TokenIntegrationServiceClientTests()
    {
        Mock<IConfiguration> mockConfiguration = new();

        // Mock IConfiguration behavior for "CdaServiceEndpoint"
        mockConfiguration.Setup(c => c[HttpClientUrlVariables.TokenIntegrationServiceUrl])
            .Returns(TokenIntegrationServicesEndpointUrl);
            
        // Set up the mock HttpClientFactory to return a client that uses the mocked HttpMessageHandler
        _httpClientFactoryMock.Setup(x => x.CreateClient(HttpClientNames.TokenIntegrationService))
            .Returns(new HttpClient(_handlerMoq.Object)
            {
                BaseAddress = new Uri(TokenIntegrationServicesEndpointUrl)
            });
            
        _sut = new TokenIntegrationServiceClient(_httpClientFactoryMock.Object, mockConfiguration.Object, _logger.Object);
    }

    [Fact]
    public async Task PostRpt_Should_Return_Response_When_Successful()
    {
        var request = new CdaTokenRequestModel
        {
            GrantType = TokenQueryParams.UmaGrantType,
            ClaimTokenFormat = TokenQueryParams.PensionDashboardRqp,
            ClaimToken = TokenQueryParams.ValidJwtToken,
            Scope = TokenQueryParams.Owner,
            Ticket = TokenQueryParams.ValidJwtToken
        };

        var response = new HttpResponseMessage
        {
            Content = JsonContent.Create(new PeiRetrievalDetailsResponseModel { PeisId = PeisId }),
            StatusCode = HttpStatusCode.OK,
        };

        // Mock SendAsync method of the HttpMessageHandler to return the mocked response
        _handlerMoq.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        var result = await _sut.PostAsync(request, new RequestHeaderModel { XRequestId = "123e4567-e89b-12d3-a456-426614174000" });

        // Asserting the result is not null and is of the correct type
        Assert.NotNull(result);
        Assert.IsType<PeiRetrievalDetailsResponseModel>(result);
        Assert.Equal(PeisId, result.PeisId);  // Further verification of content
    }
    
    [Fact]
    public async Task PostRpt_Should_Return_Response_When_PeisId_Is_Present_Successful()
    {
        var request = new CdaTokenRequestModel
        {
            GrantType = TokenQueryParams.UmaGrantType,
            ClaimTokenFormat = TokenQueryParams.PensionDashboardRqp,
            ClaimToken = TokenQueryParams.ValidJwtToken,
            Scope = TokenQueryParams.Owner,
            Ticket = TokenQueryParams.ValidJwtToken
        };

        var response = new HttpResponseMessage
        {
            Content = JsonContent.Create(new PeiRetrievalDetailsResponseModel { PeisId = PeisId }),
            StatusCode = HttpStatusCode.OK,
        };

        // Mock SendAsync method of the HttpMessageHandler to return the mocked response
        _handlerMoq.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);
        
        var result = await _sut.PostAsync(request, new RequestHeaderModel { XRequestId = "123e4567-e89b-12d3-a456-426614174000" });

        // Asserting the result is not null and is of the correct type
        Assert.NotNull(result);
        Assert.IsType<PeiRetrievalDetailsResponseModel>(result);
        Assert.Equal(PeisId, result.PeisId);  // Further verification of content
    }


    [Fact]
    public async Task PostRptAsync_Should_Throw_InvalidOperationException_When_Endpoint_Is_Not_Configured()
    {
        // Arrange
        var mockConfiguration = new Mock<IConfiguration>();
        mockConfiguration.Setup(c => c[HttpClientUrlVariables.CdaServiceUrl]).Returns(string.Empty);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            Task.FromResult(new TokenIntegrationServiceClient(_httpClientFactoryMock.Object, mockConfiguration.Object, _logger.Object))
        );

        Assert.Equal("TokenIntegrationService endpoint is not configured.", ex.Message);
    }

    [Fact]
    public async Task PostRpt_Should_Throw_ServiceCommunicationException_When_HttpRequestException_Occurs()
    {
        var request = new CdaTokenRequestModel();

        // Simulate HttpRequestException
        _handlerMoq.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        var ex = await Assert.ThrowsAsync<ServiceCommunicationException>(async () =>
            await _sut.PostAsync(request, new RequestHeaderModel { XRequestId = "123" })
        );

        Assert.Equal("Error communicating with token integration service", ex.Message);
        Assert.IsType<HttpRequestException>(ex.InnerException);
    }

    [Fact]
    public async Task PostRpt_Should_Throw_InvalidOperationException_When_Response_Content_Is_Null()
    {
        var request = new CdaTokenRequestModel();

        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("") // Content is empty
        };

        _handlerMoq.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        var ex = await Assert.ThrowsAsync<ServiceCommunicationException>(async () =>
            await _sut.PostAsync(request, new RequestHeaderModel { XRequestId = "123" })
        );

        Assert.IsType<System.Text.Json.JsonException>(ex.InnerException);
    }

    [Fact]
    public async Task PostRpt_Should_Throw_ServiceCommunicationException_When_Unexpected_Exception_Occurs()
    {
        var request = new CdaTokenRequestModel();

        // Simulate a generic exception
        _handlerMoq.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new Exception("Unexpected error"));

        var ex = await Assert.ThrowsAsync<ServiceCommunicationException>(async () =>
            await _sut.PostAsync(request, new RequestHeaderModel { XRequestId = "123" })
        );

        Assert.Equal("An unexpected error occurred during token integration service communication.", ex.Message);
        Assert.IsType<Exception>(ex.InnerException);
    }
}