using System.Net;
using System.Net.Http.Json;
using MhpdCommon.Constants.HttpClient;
using Moq;
using Moq.Protected;
using PeiIntegrationService.HttpClients.Implementation;
using PeiIntegrationService.Models.CdaPeisServiceClient;
using PeiIntegrationService.Models.CdaPiesService;

namespace PeiIntegrationService.UnitTests;

public class CDAPiesServiceClientUnitTests
{
    private readonly CdaPiesServiceClient _sut;
    private readonly Mock<HttpMessageHandler> _handlerMoq = new();
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock = new();

    public CDAPiesServiceClientUnitTests()
    {
        _sut = new CdaPiesServiceClient(_httpClientFactoryMock.Object);
    }

    [Fact]
    public async void When_Service_Is_Called_It_Should_Return_Response()
    {
        var request = new CdaPiesServiceRequestModel
        {
            PeisId = "cd0e4fdc-8586-4483-9899-17dd85af9074",
            RequestId = "askdj902139012ekasdlasdj",
            Rpt = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJsb2dnZWRJbkFzIjoiYWRtaW4iLCJpYXQiOjE0MjI3Nzk2Mzh9.gzSraSYS8EXBxLN_oWnFSRgCzcmJmMjLiuyu5CSpyHI",
        };

        var apiResponse = new CdaPeiApiResponse 
        { 
            PeiList =
            [
                new() {
                    Pei = "asas",
                    Description = "Description",
                    RetrievalStatus = "Ok",
                    RetrievalRequestedTimestamp = DateTime.UtcNow,
                }
            ]
        };

        var httpResponse = new HttpResponseMessage
        {
            Content = JsonContent.Create(apiResponse),
            StatusCode = HttpStatusCode.OK,
        };

        _handlerMoq.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        _httpClientFactoryMock.Setup(x => x.CreateClient(HttpClientNames.CdaService))
            .Returns(new HttpClient(_handlerMoq.Object)
            {
                BaseAddress = new Uri("http://localhost:1234")
            });

        var result = await _sut.GetPiesAsync(request);

        Assert.NotNull(result);
        Assert.True(result.GetType() == typeof(CdaPiesServiceResponseModel));
    }
}
