using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using PeiIntegrationService.HttpClients.Implementation;
using PeiIntegrationService.Models.MapsCdaService;

namespace PeiIntegrationService.UnitTests
{
    public class MapsCdaServiceClientUnitTests
    {
        private MapsCdaServiceClient _sut;
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock = new();
        private readonly Mock<HttpMessageHandler> _handlerMoq = new();
        private readonly IConfiguration _configuration;

        public MapsCdaServiceClientUnitTests()
        {
            _configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
               .Build();
            _configuration["MapsCdaServiceEndpoint"] = "http://localhost:1234";

            _sut = new MapsCdaServiceClient(_httpClientFactoryMock.Object, _configuration!);
        }

        [Fact]
        public async void When_Service_Is_Called_It_Should_Return_Response()
        {
            // Arrange
            var request = new MapsRqpServiceRequestModel
            {
                Iss = "https://maps.com",
                UserSessionId = "askdj902139012ekasdlasdj"
            };

            var response = new MapsRqpServiceResponseModel
            {
                Rqp = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJsb2dnZWRJbkFzIjoiYWRtaW4iLCJpYXQiOjE0MjI3Nzk2Mzh9.gzSraSYS8EXBxLN_oWnFSRgCzcmJmMjLiuyu5CSpyHI"
            };

            var httpResponse = new HttpResponseMessage
            {
                Content = JsonContent.Create(response),
                StatusCode = HttpStatusCode.OK,
            };

            _handlerMoq.Protected()
                .Setup<Task<HttpResponseMessage>>(
                 "SendAsync",
                 ItExpr.IsAny<HttpRequestMessage>(),
                 ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            _httpClientFactoryMock.Setup(x => x.CreateClient("MapsCdaService"))
                .Returns(new System.Net.Http.HttpClient(_handlerMoq.Object)
                {
                    BaseAddress = new Uri(_configuration["MapsCdaServiceEndpoint"]!)
                });

            // Act
            var result = await _sut.PostRqp(request);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.GetType() == typeof(MapsRqpServiceResponseModel));
        }
    }
}
