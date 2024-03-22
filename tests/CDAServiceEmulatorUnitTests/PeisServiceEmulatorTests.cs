using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PeisServiceEmulator.Controllers;
using PeIsServiceEmulator.Models.Peis;

namespace PeIsServiceEmulatorUnitTests
{
    public class PeisServiceEmulatorTests
    {
        private readonly DefaultHttpContext _httpContext;
        private readonly PeisController _controller;

        public PeisServiceEmulatorTests()
        {
            _httpContext = new DefaultHttpContext();
            _httpContext.Request.Headers["X-Request-ID"] = "1111-2222-3333-4444";
            _httpContext.Request.Headers["X-Version"] = "1.0";
            
            _controller = new PeisController()
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = _httpContext
                }
            };
        }

        [Fact]
        public async void WhenControllerIsCalled_WithCorrectHeaders_CorrectPath_CorrectScope_ThenItShouldReturn_Ok200Response()
        {
            // Arrange
            AddAuthorisationHeader();
            int totalRecordsInMock = 1; // Change this if you change the number of records in mock
            string userGuid = "06f4b162-c29f-460c-b9ba-e9251e165798";
            string? scope = "owner";
            
            // Act
            var result = await _controller.GetAsync(userGuid, scope);
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
        public async void WhenControllerIsCalled_WithCorrectHeaders_CorrectPath_VariationOfScope_ThenItShouldReturn_Ok200Response ()
        {
            // Arrange
            AddAuthorisationHeader();
            string userGuid = "a66af766-0500-4ad4-b3ed-f31c973bea82";
            string? scope = "uma_protection";

            // Act
            var result = await _controller.GetAsync(userGuid, scope);

            // Assert
            Assert.True(result.GetType() == typeof(OkObjectResult));
        }

        [Fact]
        public async void WhenControllerIsCalled_WithCorrectHeaders_CorrectPath_WrongScope_ThenItShouldReturn_BadRequest400Response()
        {
            // Arrange
            AddAuthorisationHeader();
            string userGuid = "1f50ddf9-3212-48bc-9762-092cf044bfc3";
            string? scope = "abc";
            _httpContext.Request.QueryString = new QueryString("?scope=abc");

            // Act
            var result = await _controller.GetAsync(userGuid, scope);
            BadRequestObjectResult badResult = (BadRequestObjectResult)result;

            // Assert
            Assert.True(result.GetType() == typeof(BadRequestObjectResult));
            Assert.True(badResult.StatusCode == (int)HttpStatusCode.BadRequest);
        }

        [Fact]
        public async void WhenControllerIsCalled_WithNoAuthorisationHeader_CorrectPath_CorrectScope_ThenItShouldReturn_Unauthorised401Response()
        {
            // Arrange
            string userGuid = "040221bb-5175-4a6e-8d0b-2798f2f378aa";
            string? scope = "uma_protection";
            _httpContext.Request.QueryString = new QueryString("?scope=uma_protection");
            var responseHeaderValue = "realm=\"PensionDashboard\", as_uri=\"https://as.pdp.com\", ticket=\"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.cThIIoDvwdueQB468K5xDc5633seEFoqwxjF_xSJyQQ\"";

            // Act
            var result = await _controller.GetAsync(userGuid, scope);
            UnauthorizedObjectResult unAuthorizedResult = (UnauthorizedObjectResult)result;
            _httpContext.Response.Headers.TryGetValue("WWW-Authenticate", out var wwwAuthenticate);

            // Assert
            Assert.True(result.GetType() == typeof(UnauthorizedObjectResult));
            Assert.True(unAuthorizedResult.StatusCode == (int)HttpStatusCode.Unauthorized);
            Assert.True(wwwAuthenticate == responseHeaderValue);
        }

        [Fact]
        public async void WhenControllerIsCalled_WithNoAuthorisationHeader_CorrectPath_NoScope_ThenItShouldReturn_Unauthorised401Response()
        {
            // Arrange
            string userGuid = "06f4b162-c29f-460c-b9ba-e9251e165798";

            // Act
            var result = await _controller.GetAsync(userGuid, string.Empty);

            // Assert
            Assert.True(result.GetType() == typeof(UnauthorizedObjectResult));
        }

        [Fact]
        public async void WhenControllerIsCalled_WithCorrectHeaders_InvalidVersion_ThenItShouldReturn_BadReques400Response()
        {
            // Arrange
            AddAuthorisationHeader();
            _httpContext.Request.Headers["X-Version"] = "1";
            string userGuid = "bf142408-ca72-4541-96a5-056cb5d8f301";
            string? scope = "owner";

            // Act
            var result = await _controller.GetAsync(userGuid, scope);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.GetType() == typeof(BadRequestObjectResult));
        }

        [Fact]
        public async void WhenControllerIsCalled_WithCorrectHeaders_WrongUserGuid_ThenItShouldReturn_BadReques400Response()
        {
            // Arrange
            AddAuthorisationHeader();
            _httpContext.Request.Headers["X-Version"] = "1";
            string userGuid = "1111-1111-1111-1111";
            string? scope = "owner";

            // Act
            var result = await _controller.GetAsync(userGuid, scope);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.GetType() == typeof(BadRequestObjectResult));
        }

        private void AddAuthorisationHeader()
        {
            _httpContext.Request.Headers["Authorisation"] = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";
        }
    }
}
