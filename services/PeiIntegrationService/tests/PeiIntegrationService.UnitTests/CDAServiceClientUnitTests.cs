using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using PeiIntegrationService.HttpClients.Implementation;
using PeiIntegrationService.Models.CdaPeisServiceClient;
using PeiIntegrationService.Models.CdaPiesService;

namespace PeiIntegrationService.UnitTests;

public class CDAPiesServiceClientUnitTests
{
    private CdaPiesServiceClient _sut;
    private readonly IConfiguration _configuration;
    private readonly Mock<HttpMessageHandler> _handlerMoq = new();
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock = new();

    public CDAPiesServiceClientUnitTests()
    {
        _configuration = new ConfigurationBuilder()
                        .AddJsonFile("appsettings.json")
                       .Build();
        _configuration["CdaPeisServiceEndpoint"] = "http://localhost:1234";

        _sut = new CdaPiesServiceClient(_httpClientFactoryMock.Object, _configuration);
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

        _httpClientFactoryMock.Setup(x => x.CreateClient("CdaPiesService"))
            .Returns(new HttpClient(_handlerMoq.Object)
            {
                BaseAddress = new Uri(_configuration["CdaPeisServiceEndpoint"]!)
            });

        var result = await _sut.GetPiesAsync(request);

        Assert.NotNull(result);
        Assert.True(result.GetType() == typeof(CdaPiesServiceResponseModel));
    }
}
