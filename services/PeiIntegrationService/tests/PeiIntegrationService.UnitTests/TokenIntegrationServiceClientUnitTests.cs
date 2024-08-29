using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using PeiIntegrationService.HttpClients.Implementation;
using PeiIntegrationService.Models.TokenIntegrationService;

namespace PeiIntegrationService.UnitTests
{
    public class TokenIntegrationServiceClientUnitTests
    {
        private TokenIntegrationServiceClient _sut;
        private readonly IConfiguration _configuration;
        private readonly Mock<HttpMessageHandler> _handlerMoq = new();
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock = new();

        public TokenIntegrationServiceClientUnitTests()
        {
            _configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();
            _configuration["CdaTokenServicesEndpoint"] = "http://localhost:9876";
            _configuration["TokenIntegrationServiceEndpoint"] = "http://localhost:1234";

            _sut = new TokenIntegrationServiceClient(_httpClientFactoryMock.Object, _configuration!);
        }

        [Fact]
        public async void When_Service_Is_Called_It_Should_Return_Response()
        {
            // Arrange
            var request = new TokenIntegrationServiceRequestModel
            {
               Rqp = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJsb2dnZWRJbkFzIjoiYWRtaW4iLCJpYXQiOjE0MjI3Nzk2Mzh9.gzSraSYS8EXBxLN_oWnFSRgCzcmJmMjLiuyu5CSpyHI",
               As_Uri = "http://localhost:YYYY",
               Ticket = "askdj902139012ekasdlasdj"
            };

            var response = new TokenIntegrationResponseModel
            {
                Rpt = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJsb2dnZWRJbkFzIjoiYWRtaW4iLCJpYXQiOjE0MjI3Nzk2Mzh9.gzSraSYS8EXBxLN_oWnFSRgCzcmJmMjLiuyu5CSpyHI"
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

            _httpClientFactoryMock.Setup(x => x.CreateClient("TokenIntegrationService"))
                .Returns(new HttpClient(_handlerMoq.Object)
                {
                    BaseAddress = new Uri(_configuration["TokenIntegrationServiceEndpoint"]!)
                });

            // Act
            var result = await _sut.PostRpt(request);

            // Assert
            Assert.NotNull(result);
        }
    }
}
