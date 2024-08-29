using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Moq.Protected;
using TokenIntegrationService.Controllers;
using TokenIntegrationService.HttpClients;
using TokenIntegrationService.Models;

namespace TokenIntegrationServiceUnitTests
{
    public class CDATokenServiceUnitTests
    {
        private CDATokenService _sut;
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock = new();
        private readonly Mock<HttpMessageHandler> _handlerMoq = new();

        public CDATokenServiceUnitTests()
        {
            _sut = new CDATokenService(_httpClientFactoryMock.Object);
        }

        [Fact]
        public async void When_Service_Is_Called_It_Should_Return_Response()
        {
            var request = new CDATokenRequestModel
            {
                GrantType = "urn:ietf:params:oauth:grant-type:uma-ticket",
                ClaimTokenFormat = "pension_dashboad_rqp",
                ClaimToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c",
                Scope = "owner",
                RequestId = "sdfasdfasdasdadsg",
                Ticket = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c",                          
               
                Rqp = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c",
                CdaTokenUrl = "http://localhost:5044"
            };

            RptsModel rpts =  new RptsModel { AccessToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c" };             
            
            var response = new HttpResponseMessage
            {
                Content = JsonContent.Create(rpts),
                StatusCode = HttpStatusCode.OK,
            };

            _handlerMoq.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);

            _httpClientFactoryMock.Setup(x => x.CreateClient("CDAToken"))
                .Returns(new System.Net.Http.HttpClient(_handlerMoq.Object)
                {
                    BaseAddress = new Uri(request.CdaTokenUrl!)
                });

            var result = await _sut.PostRpt(request);
            Assert.NotNull(result);
            Assert.True(result.GetType() == typeof(RptsModel));
        }
    }
}
