using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using PeisServiceEmulator.Controllers;
using PeIsServiceEmulator.Models.Peis;

namespace PeIsServiceEmulatorUnitTests
{
    public class PeisServiceEmulatorTests
    {
        private readonly DefaultHttpContext _httpContext;
        private readonly PeisController _controller;
        private readonly int totalRecordsInMock = 1;

        public PeisServiceEmulatorTests()
        {
            _httpContext = new DefaultHttpContext();
            //_httpContext.Request.Headers["X-Request-ID"] = "b7301d11-f166-499a-9bf1-0598c2f1af52";
            
            _controller = new PeisController()
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = _httpContext
                }
            };
        }

        [Fact]
        public async void WhenControllerIsCalled_WithCorrectAuthorisationHeader_CorrectRoute_ThenItShouldReturn_Ok200Response()
        {
            // Arrange
            AddAuthorisationHeader();
            _httpContext.Request.Headers["X-Request-ID"] = "b7301d11-f166-499a-9bf1-0598c2f1af52";
            string peis_id = "cd0e4fdc-8586-4483-9899-17dd85af9074";

            // Act
            var result = await _controller.GetAsync(peis_id);
            OkObjectResult okResult = (OkObjectResult)result;
            var data = (PeiModel[])okResult!.Value!;

            // Assert
            Assert.True(result.GetType() == typeof(OkObjectResult));
            Assert.True(data.GetType() == typeof(PeiModel[]));

            Assert.NotNull(result);
            Assert.True(totalRecordsInMock == data.Count());

            Assert.True(okResult.StatusCode == (int)HttpStatusCode.OK);
        }

        [Fact]
        public async void WhenControllerIsCalled_WithCorrectAuthorisationHeader_InCorrectPiesRoute_ThenItShouldReturn_BadRequest400Response()
        {
            // Arrange
            AddAuthorisationHeader();
            _httpContext.Request.Headers["X-Request-ID"] = "b7301d11-f166-499a-9bf1-0598c2f1af52";
            string peis_id = "?><>(*)&&-8586-4483-9899-17dd85af9074";

            // Act
            var result = await _controller.GetAsync(peis_id);
            BadRequestObjectResult okResult = (BadRequestObjectResult)result;

            // Assert
            Assert.True(result.GetType() == typeof(BadRequestObjectResult));
            Assert.True(okResult.StatusCode == (int)HttpStatusCode.BadRequest);
        }

        [Fact]
        public async void WhenControllerIsCalled_WithInCorrectAuthorisationHeader_CorrectRoutePies_ThenItShouldReturn_UnAuthorised401Response()
        {
            // Arrange
            AddInCorrectAuthorisationHeader();
            _httpContext.Request.Headers["X-Request-ID"] = "b7301d11-f166-499a-9bf1-0598c2f1af52";
            string peis_id = "8586-4483-9899-17dd85af9074";

            // Act
            var result = await _controller.GetAsync(peis_id);

            // Assert
            Assert.True(result.GetType() == typeof(UnauthorizedObjectResult));
        }

        [Fact]
        public async void WhenControllerIsCalled_WithCorrectHeaders_CorrectPath_No_X_Request_Id_ThenItShouldReturn_BadRequest400Response()
        {
            // Arrange
            AddAuthorisationHeader();
            string peis_id = "8586-4483-9899-17dd85af9074";

            // Act
            var result = await _controller.GetAsync(peis_id);
            BadRequestObjectResult badResult = (BadRequestObjectResult)result;

            // Assert
            Assert.True(result.GetType() == typeof(BadRequestObjectResult));
            Assert.True(badResult.StatusCode == (int)HttpStatusCode.BadRequest);
        }

        [Fact]
        public async void WhenControllerIsCalled_WithNoAuthorisationHeader_CorrectRoutePies_ThenItShouldReturn_Unauthorised401ResponseAnd_WwwAuthenticateResponseHeader()
        {
            // Arrange
            string peis_id = "8586-4483-9899-17dd85af9074";
            _httpContext.Request.Headers["X-Version"] = "b7301d11-f166-499a-9bf1-0598c2f1af52";
            var responseHeaderValue = "realm=\"PensionDashboard\", as_uri=\"https://as.pdp.com\", ticket=\"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.cThIIoDvwdueQB468K5xDc5633seEFoqwxjF_xSJyQQ\"";

            // Act
            var result = await _controller.GetAsync(peis_id);
            UnauthorizedObjectResult unAuthorizedResult = (UnauthorizedObjectResult)result;
            _httpContext.Response.Headers.TryGetValue("WWW-Authenticate", out var wwwAuthenticate);

            // Assert
            Assert.True(result.GetType() == typeof(UnauthorizedObjectResult));
            Assert.True(unAuthorizedResult.StatusCode == (int)HttpStatusCode.Unauthorized);
            Assert.True(wwwAuthenticate == responseHeaderValue);
        }

        private void AddAuthorisationHeader()
        {
            _httpContext.Request.Headers[HeaderNames.Authorization] = "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";
        }

        private void AddInCorrectAuthorisationHeader()
        {
            _httpContext.Request.Headers[HeaderNames.Authorization] = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";
        }
    }
}
