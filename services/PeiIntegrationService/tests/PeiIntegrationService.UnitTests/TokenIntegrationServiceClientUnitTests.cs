using System.Net;
using System.Net.Http.Json;
using MhpdCommon.Constants.HttpClient;
using MhpdCommon.Models.Configuration;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using PeiIntegrationService.HttpClients.Implementation;
using PeiIntegrationService.Models.TokenIntegrationService;

namespace PeiIntegrationService.UnitTests
{
    public class TokenIntegrationServiceClientUnitTests
    {
        private readonly TokenIntegrationServiceClient _sut;
        private readonly Mock<HttpMessageHandler> _handlerMoq = new();
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock = new();

        public TokenIntegrationServiceClientUnitTests()
        {
            var config = new CommonHttpConfiguration
            {
                CdaServiceUrl = "https://cda.service.com"
            };

            var options = Options.Create(config);

            _sut = new TokenIntegrationServiceClient(_httpClientFactoryMock.Object, options);
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

            _httpClientFactoryMock.Setup(x => x.CreateClient(HttpClientNames.TokenIntegrationService))
                .Returns(new HttpClient(_handlerMoq.Object)
                {
                    BaseAddress = new Uri("http://localhost:1234")
                });

            // Act
            var result = await _sut.PostRpt(request);

            // Assert
            Assert.NotNull(result);
        }
    }
}
