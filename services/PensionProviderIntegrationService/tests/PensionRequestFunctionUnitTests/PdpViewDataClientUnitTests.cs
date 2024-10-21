using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Moq;
using Moq.Protected;
using PensionRequestFunction.HttpClient;

namespace PensionRequestFunctionUnitTests
{
    public class PdpViewDataClientUnitTests
    {
        private readonly PdpViewDataClient _sut;
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock = new();

        public PdpViewDataClientUnitTests()
        {
            _sut = new PdpViewDataClient(_httpClientFactoryMock.Object);
        }

        [Theory]
        [InlineData(true, HttpStatusCode.OK)]
        [InlineData(false, HttpStatusCode.Unauthorized)]
        [InlineData(false, HttpStatusCode.InternalServerError)]
        public async Task When_Service_Is_Called_It_Should_Return_Response(bool success, HttpStatusCode statusCode)
        {
            // Arrange
            var response = GetResponse();
            string assetGuid = Guid.NewGuid().ToString();
            string viewDataUrl = "https://pdpviewdataservicedemulator.azurewebsites.net/view-data/";
            var handler = CreateHttpHandler(success, statusCode);

            _httpClientFactoryMock.Setup(x => x.CreateClient(string.Empty))
                .Returns(new HttpClient(handler.Object)
                {
                    BaseAddress = new Uri("http://localhost:1234"!)
                });

            // Act
            var result = await _sut.GetPdpViewDataAsync(assetGuid, viewDataUrl, string.Empty); 

            // Assert
            Assert.NotNull(result);
            if(success)
                Assert.NotNull(result.ViewDataToken);
            var expectedMessage = success ? ((int)statusCode).ToString() : statusCode.ToString();
            Assert.Equal(expectedMessage, result.ResponseMessage.ResponseStatusCode);
        }

        private static Mock<HttpMessageHandler> CreateHttpHandler(bool success, HttpStatusCode statusCode)
        {
            var httpMessageHandlerMock = new Mock<HttpMessageHandler>();

            httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(CreateHttpResponse(statusCode, success ? GetResponse() : null));

            return httpMessageHandlerMock;
        }

        private static HttpResponseMessage CreateHttpResponse(HttpStatusCode statusCode, string? content = null)
        {
            var response = new HttpResponseMessage(statusCode);
            if (content != null)
                response.Content = JsonContent.Create(content);
            else
                response.Headers.WwwAuthenticate.Add(new AuthenticationHeaderValue("Basic", "realm=\"example\""));

            return response;
        }

        private static string GetResponse()
        {
            return """ "view_data_token": "eyJhbGciOiJSUzI1NiIsImtpZCI6ImMwMGI0MGVhLTZkYTEtNDA4YS1hNmM5LTE3YjFmZjQ1YmI5YSIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIzMjRicWZ3MzQ4ZjlxNDM5OGgzIiwiaWF0IjoxNzIwNzg1NjkwLCJleHAiOjE3MjA3ODkyOTAsImp0aSI6ImU3M2E5MGIzLTg5ODMtNDYwNy04YmZmLWViOGI4YmVjNjdmMiIsImF1ZCI6Imh0dHBzOi8vcGRwL2lnL3Rva2VuIiwiaXNzIjoiREFUQV9QUk9WSURFUl8xZmQxZGE4OC05ZmIzLTQ2MWMtYTQ4YS0zZGJhMjFiZmJhMTciLCJWaWV3RGF0YSI6IntcclxuXHRcImFycmFuZ2VtZW50c1wiOiBbXHJcblx0XHR7XHJcblx0XHRcdFwicGVuc2lvblByb3ZpZGVyU2NoZW1lTmFtZVwiOiBcIk15IENvbXBhbnkgRGlyZWN0IENvbnRyaWJ1dGlvbiBTY2hlbWVcIixcclxuXHRcdFx0XCJhbHRlcm5hdGVTY2hlbWVOYW1lXCI6IHtcclxuXHRcdFx0XHRcIm5hbWVcIjogXCJDb252ZXJ0ZWQgZnJvbSBNeSBPbGQgRGlyZWN0IENvbnRyaWJ1dGlvbiBTY2hlbWVcIixcclxuXHRcdFx0XHRcImFsdGVybmF0ZU5hbWVUeXBlXCI6IFwiRk9SXCJcclxuXHRcdFx0fSxcclxuXHRcdFx0XCJwb3NzaWJsZU1hdGNoXCI6IHRydWUsXHJcblx0XHRcdFwicG9zc2libGVNYXRjaFJlZmVyZW5jZVwiOiBcIlExMjM0NVwiLFxyXG5cdFx0XHRcInBlbnNpb25BZG1pbmlzdHJhdG9yXCI6IHtcclxuXHRcdFx0XHRcIm5hbWVcIjogXCJQZW5zaW9uIENvbXBhbnkgMVwiLFxyXG5cdFx0XHRcdFwiY29udGFjdE1ldGhvZHNcIjogW1xyXG5cdFx0XHRcdFx0e1xyXG5cdFx0XHRcdFx0XHRcInByZWZlcnJlZFwiOiBmYWxzZSxcclxuXHRcdFx0XHRcdFx0XCJjb250YWN0TWV0aG9kRGV0YWlsc1wiOiB7XHJcblx0XHRcdFx0XHRcdFx0XCJlbWFpbFwiOiBcImV4YW1wbGVAZXhhbXBsZW15bGluZS5jb21cIlxyXG5cdFx0XHRcdFx0XHR9XHJcblx0XHRcdFx0XHR9LFxyXG5cdFx0XHRcdFx0e1xyXG5cdFx0XHRcdFx0XHRcInByZWZlcnJlZFwiOiB0cnVlLFxyXG5cdFx0XHRcdFx0XHRcImNvbnRhY3RNZXRob2REZXRhaWxzXCI6IHtcclxuXHRcdFx0XHRcdFx0XHRcIm51bWJlclwiOiBcIisxMjMgMTExMTExMTExMVwiLFxyXG5cdFx0XHRcdFx0XHRcdFwidXNhZ2VcIjogW1xyXG5cdFx0XHRcdFx0XHRcdFx0XCJBXCIsXHJcblx0XHRcdFx0XHRcdFx0XHRcIk1cIlxyXG5cdFx0XHRcdFx0XHRcdF1cclxuXHRcdFx0XHRcdFx0fVxyXG5cdFx0XHRcdFx0fVxyXG5cdFx0XHRcdF1cclxuXHRcdFx0fVxyXG5cdFx0fVxyXG5cdF1cclxufSIsIm5iZiI6MTcyMDc4NTY5MH0.Ke3ku_SUgQXZ1Vq7eTaX7OGKtRp_udiQ_bz-glrdHoLiqnrZZ9B8B8Oqw2wGC-JsYTZVdjLOScc2E0BHWhJdRzvP23FVx-6hy5Xixh0_8I7wpPORp0kC8ng1nObiJyRNm5HScXDGU4gKXSZHWN1ZvuR0RNPa5h8VMQgoFgiIoFpe0hoKyXCp7zRus7lmiMFdtv9rysagVbaWBaBSVwb2WKGaMabcqCXupMXeO8N56LrajbMscFW655jYhC_0MHWlFSboFOligjbrAuoBGwUwWB3W8Ro9paD_oKHKZml8aRuAdklTPVghVm0WB0YVvSozwYM4JE3r8vXVwwqAulR3Nw""";
        }
    }
}
