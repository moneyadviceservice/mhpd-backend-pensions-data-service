using System.Net;
using System.Net.Http.Json;
using Moq;
using Moq.Protected;
using PeiIntegratioinService.HttpClients;
using PeiIntegrationService.Models;

namespace PeiIntegrationService.UnitTests
{
    public class CDAServiceUnitTests
    {
        private CDAService _sut;
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock = new();
        private readonly Mock<HttpMessageHandler> _handlerMoq = new();

        public CDAServiceUnitTests()
        {
            _sut = new CDAService(_httpClientFactoryMock.Object);
        }

        [Fact]
        public async void When_Service_Is_Called_It_Should_Return_Response()
        {
            var request = new CDAServiceRequestModel
            {
                CdaUserGuid = "cd0e4fdc-8586-4483-9899-17dd85af9074",
                Issuer = "https://maps.com",
                UserSessionId = "askdj902139012ekasdlasdj",
                Authorization = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJsb2dnZWRJbkFzIjoiYWRtaW4iLCJpYXQiOjE0MjI3Nzk2Mzh9.gzSraSYS8EXBxLN_oWnFSRgCzcmJmMjLiuyu5CSpyHI",
                CdaServiceUrl = "http://localhost:5089",
                Scope = "owner"
            };

            PeiModel[] peis = [ new PeiModel {Pei = "asas",
                Description = "Description",
                RetrievalStatus = "Ok",
                RetrievalRequestedTimestamp = DateTime.Now,}
            ];

            var response = new HttpResponseMessage
            {
                Content = JsonContent.Create(peis),
                StatusCode = HttpStatusCode.OK,
            };

            _handlerMoq.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);

            _httpClientFactoryMock.Setup(x => x.CreateClient("CDAService"))
                .Returns(new System.Net.Http.HttpClient(_handlerMoq.Object)
                {
                    BaseAddress = new Uri(request.CdaServiceUrl!)
                }) ;

            var result = await _sut.GetPies(request);
            
            Assert.NotNull(result);
            Assert.True(result.GetType() == typeof(PeiModel[]));
        }
    }
}
