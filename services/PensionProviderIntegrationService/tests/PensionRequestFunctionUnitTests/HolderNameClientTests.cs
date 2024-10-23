using MhpdCommon.Constants.HttpClient;
using MhpdCommon.Models.MHPDModels;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using PensionRequestFunction.HttpClient;
using PensionRequestFunction.Repository;
using System.Net;
using System.Net.Http.Json;

namespace PensionRequestFunctionUnitTests;

public class HolderNameClientTests
{
    private readonly Mock<IHttpClientFactory> _httpClientFactory;
    private readonly Mock<ILogger<HolderNameClient>> _logger;
    private readonly Mock<IHolderNameConfigurationRepository<HolderNameConfigurationModel>> _repository;

    public HolderNameClientTests()
    {
        _httpClientFactory = new Mock<IHttpClientFactory>();
        _logger = new Mock<ILogger<HolderNameClient>>();
        _repository = new Mock<IHolderNameConfigurationRepository<HolderNameConfigurationModel>>();
        _repository.Setup(mock => mock.InsertItemAsync(It.IsAny<HolderNameConfigurationModel>(), It.IsAny<string>())).Verifiable();
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(1, false)]
    [InlineData(2, true)]
    public async Task WhenRepositoryHasNoMatch_HttpModelIsReturned(int size, bool isNullExpected)
    {
        // Arrange
        var handler = CreateHttpHandler(size);
        _httpClientFactory.Setup(x => x.CreateClient(HttpClientNames.CdaService))
                .Returns(new HttpClient(handler.Object)
                {
                    BaseAddress = new Uri("http://localhost:1234"!)
                });

        HolderNameConfigurationModel? model = null;
        _repository.Setup(mock => mock.GetByIdAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(model);

        var client = new HolderNameClient(_httpClientFactory.Object, _repository.Object, _logger.Object);

        // Act
        var result = await client.GetViewDataUrlAsync(Guid.NewGuid().ToString());

        //Assert
        Assert.True(isNullExpected ? result == null : result != null);
        _httpClientFactory.Verify(x => x.CreateClient(HttpClientNames.CdaService), Times.Once);

        if (!isNullExpected)
        {
            _repository.Verify(mock => mock.InsertItemAsync(It.IsAny<HolderNameConfigurationModel>(), It.IsAny<string>()), Times.Once);
        }
    }

    [Fact]
    public async Task WhenRepositoryHasAMatch_CacheModelIsReturned()
    {
        // Arrange
        var handler = CreateHttpHandler(1);
        _httpClientFactory.Setup(x => x.CreateClient(HttpClientNames.CdaService))
                .Returns(new HttpClient(handler.Object)
                {
                    BaseAddress = new Uri("http://localhost:1234"!)
                });

        var model = new HolderNameConfigurationModel
        {
            HolderNameGuid = Guid.NewGuid().ToString(),
            Id = Guid.NewGuid().ToString(),
            ViewDataUrl = "https://viewdata.pdp.com",
        };
        _repository.Setup(mock => mock.GetByIdAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(model);

        var client = new HolderNameClient(_httpClientFactory.Object, _repository.Object, _logger.Object);

        // Act
        var result = await client.GetViewDataUrlAsync(Guid.NewGuid().ToString());

        //Assert
        Assert.NotNull(result);

        _httpClientFactory.Verify(x => x.CreateClient(HttpClientNames.CdaService), Times.Never);
        _repository.Verify(mock => mock.InsertItemAsync(It.IsAny<HolderNameConfigurationModel>(), It.IsAny<string>()), Times.Never);
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
