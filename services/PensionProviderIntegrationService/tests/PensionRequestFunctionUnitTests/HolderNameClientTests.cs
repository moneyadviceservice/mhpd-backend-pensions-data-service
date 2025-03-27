using MhpdCommon.Caching;
using MhpdCommon.Constants.HttpClient;
using MhpdCommon.Models.MHPDModels;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using PensionRequestFunction.HttpClient;
using System.Net;
using System.Net.Http.Json;

namespace PensionRequestFunctionUnitTests;

public class HolderNameClientTests
{
    private readonly Mock<IHttpClientFactory> _httpClientFactory;
    private readonly Mock<ILogger<HolderNameClient>> _logger;
    private readonly Mock<IHolderNameConfigurationCache<HolderNameViewDataResponse>> _cache;

    public HolderNameClientTests()
    {
        _httpClientFactory = new Mock<IHttpClientFactory>();
        _logger = new Mock<ILogger<HolderNameClient>>();
        _cache = new Mock<IHolderNameConfigurationCache<HolderNameViewDataResponse>>();
        _cache.Setup(mock => mock.InsertItemAsync(It.IsAny<HolderNameViewDataResponse>(), It.IsAny<string>())).Verifiable();
    }

    [Fact]
    public async Task WhenRepositoryHasNoMatch_ClientModelIsReturned()
    {
        // Arrange
        var handler = CreateHttpHandler();
        _httpClientFactory.Setup(x => x.CreateClient(HttpClientNames.CdaService))
                .Returns(new HttpClient(handler.Object)
                {
                    BaseAddress = new Uri("http://localhost:1234"!)
                });

        HolderNameViewDataResponse? model = null;
        _cache.Setup(mock => mock.GetByIdAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(model);

        var client = new HolderNameClient(_httpClientFactory.Object, _cache.Object, _logger.Object);
        var correlationId = Guid.NewGuid().ToString();
        // Act
        var result = await client.GetViewDataUrlAsync(Guid.NewGuid().ToString(), correlationId);

        //Assert
        Assert.NotNull(result);
        _httpClientFactory.Verify(x => x.CreateClient(HttpClientNames.CdaService), Times.Once);

        _cache.Verify(mock => mock.InsertItemAsync(It.IsAny<HolderNameViewDataResponse>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task WhenRepositoryHasAMatch_CacheModelIsReturned()
    {
        // Arrange
        var handler = CreateHttpHandler();
        _httpClientFactory.Setup(x => x.CreateClient(HttpClientNames.CdaService))
                .Returns(new HttpClient(handler.Object)
                {
                    BaseAddress = new Uri("http://localhost:1234"!)
                });

        var model = new HolderNameViewDataResponse
        {
            HolderNameGuid = Guid.NewGuid().ToString(),
            Id = Guid.NewGuid().ToString(),
            Configuration = new HolderNameConfigurationModel { ViewDataUrl = "https://viewdata.pdp.com" },
        };

        _cache.Setup(mock => mock.GetByIdStreamAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(model);

        var client = new HolderNameClient(_httpClientFactory.Object, _cache.Object, _logger.Object);
        var correlationId = Guid.NewGuid().ToString();

        // Act
        var result = await client.GetViewDataUrlAsync(Guid.NewGuid().ToString(), correlationId);

        //Assert
        Assert.NotNull(result);

        _httpClientFactory.Verify(x => x.CreateClient(HttpClientNames.CdaService), Times.Never);
        _cache.Verify(mock => mock.InsertItemAsync(It.IsAny<HolderNameViewDataResponse>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task WhenCacheThrowsException_ClientModelIsReturned()
    {
        // Arrange
        var handler = CreateHttpHandler();
        _httpClientFactory.Setup(x => x.CreateClient(HttpClientNames.CdaService))
                .Returns(new HttpClient(handler.Object)
                {
                    BaseAddress = new Uri("http://localhost:1234"!)
                });

        var model = new HolderNameViewDataResponse
        {
            HolderNameGuid = Guid.NewGuid().ToString(),
            Id = Guid.NewGuid().ToString(),
            Configuration = new HolderNameConfigurationModel { ViewDataUrl = "https://viewdata.pdp.com" },
        };

        _cache.Setup(mock => mock.GetByIdStreamAsync(It.IsAny<string>(), It.IsAny<string>())).Throws<InvalidOperationException>();
        _cache.Setup(mock => mock.GetByIdStreamAsync(It.IsAny<string>(), It.IsAny<string>())).Throws<InvalidOperationException>();

        var client = new HolderNameClient(_httpClientFactory.Object, _cache.Object, _logger.Object);
        var correlationId = Guid.NewGuid().ToString();

        // Act
        var result = await client.GetViewDataUrlAsync(Guid.NewGuid().ToString(), correlationId);

        //Assert
        Assert.NotNull(result);

        _httpClientFactory.Verify(x => x.CreateClient(HttpClientNames.CdaService), Times.Once);
        _cache.Verify(mock => mock.InsertItemAsync(It.IsAny<HolderNameViewDataResponse>(), It.IsAny<string>()), Times.Once);
    }

    private static Mock<HttpMessageHandler> CreateHttpHandler()
    {
        var httpMessageHandlerMock = new Mock<HttpMessageHandler>();

        var model = new HolderNameViewDataResponse
        {
            HolderNameGuid = Guid.NewGuid().ToString(),
            Id = Guid.NewGuid().ToString(),
            Configuration = new HolderNameConfigurationModel { ViewDataUrl = "https://viewdata.pdp.com" },
        };

        httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()).ReturnsAsync(CreateHttpResponse(HttpStatusCode.OK, model));

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
