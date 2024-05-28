using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PDPViewDataServicedEmulator.Controllers;

namespace PDPViewDataServiceEmulatorUnitTests
{
    public class PDPViewDataServiceEmulatorUnitTests
    {
        private readonly DefaultHttpContext _httpContext;
        private readonly PDPViewDataController _controller;
        private const string CDATokenEmulatorEndPoint = "https://auth-server/";

        public PDPViewDataServiceEmulatorUnitTests()
        {
            _httpContext = new DefaultHttpContext();
            _httpContext.Request.Headers["X-Request-ID"] = "35cfcfb0-d98d-451f-83f1-e59933078555";

            _controller = new PDPViewDataController()
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = _httpContext
                }
            };
        }
        
        [Fact]
        public async void WhenControllerIsCalled_WithoutAuthHeader_ThenItShouldReturn_401UnauthorisedResponse()
        {
            // Arrange
            string? asset_guid = null;
            string? scope = null;

            // Act
            var result = await _controller.GetAsync(asset_guid!, scope);
            UnauthorizedObjectResult unAuthorisedObjectResult = (UnauthorizedObjectResult)result;

            // Assert
            Assert.NotNull(result);
            Assert.True(result.GetType() == typeof(UnauthorizedObjectResult));
            Assert.True(unAuthorisedObjectResult.StatusCode == (int)HttpStatusCode.Unauthorized);

            // check for response headers
            Assert.True(_httpContext.Response.Headers.ContainsKey("WWW-Authenticate"));

            // check for response headers values
            Assert.True(_httpContext.Response.Headers["WWW-Authenticate"].ToString().Equals("realm=\"PensionDashboard\", " +
                    $"as_uri=\"{CDATokenEmulatorEndPoint}\", " +
                    "ticket=\"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.cThIIoDvwdueQB468K5xDc5633seEFoqwxjF_xSJyQQ\""));
        }

        [Fact]
        public async void WhenControllerIsCalled_WithEmptyAuthHeader_ThenItShouldReturn_401UnauthorisedResponse()
        {
            // Arrange
            string? asset_guid = null;
            string? scope = null;
            _httpContext.Request.Headers["Authorisation"] = string.Empty;

            // Act
            var result = await _controller.GetAsync(asset_guid!, scope);
            UnauthorizedObjectResult unAuthorisedObjectResult = (UnauthorizedObjectResult)result;

            // Assert
            Assert.NotNull(result);
            Assert.True(result.GetType() == typeof(UnauthorizedObjectResult));
            Assert.True(unAuthorisedObjectResult.StatusCode == (int)HttpStatusCode.Unauthorized);
            
            // check for response headers
            Assert.True(_httpContext.Response.Headers.ContainsKey("WWW-Authenticate"));

            // check for response headers values
            Assert.True(_httpContext.Response.Headers["WWW-Authenticate"].ToString().Equals("realm=\"PensionDashboard\", " +
                    $"as_uri=\"{CDATokenEmulatorEndPoint}\", " +
                    "ticket=\"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.cThIIoDvwdueQB468K5xDc5633seEFoqwxjF_xSJyQQ\""));
        }
    }
}