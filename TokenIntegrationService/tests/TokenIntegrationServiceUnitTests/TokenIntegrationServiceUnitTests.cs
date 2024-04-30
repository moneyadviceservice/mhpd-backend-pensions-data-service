using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TokenIntegrationService.Models;
using TokenIntegrationService.Controllers;
using System.Net;
using System.Net.Http;

namespace TokenIntegrationServiceUnitTests
{
    public class TokenIntegrationServiceUnitTests
    {
        private readonly DefaultHttpContext _httpContext;
        private readonly TokenController _controller;
        public TokenIntegrationServiceUnitTests()
        {
            _httpContext = new DefaultHttpContext();
            _controller = new TokenController()
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = _httpContext
                }
            };
        }
       
        [Fact]
        public async void WhenControllerIsCalled_WithValidRequestBody_ThenItShouldReturn_OKReques200Response()
        {
            // Arrange
           
            var request = new TokenIntegrationRequestModel
            {
                rqp = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c",
                ticket = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c",
                as_uri = "http://localhost:5044"
            };

            // Act
            var result = await _controller.PostAsync(request);

            // Assert
            Assert.True(result.GetType() == typeof(OkObjectResult));        
            
        }
        [Fact]
        public async void WhenControllerIsCalled_EmptyRqp_ThenItShouldReturn_BadRequest400Response()
        {
            // Arrange

            var request = new TokenIntegrationRequestModel
            {
                rqp = string.Empty,
                ticket = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c",
                as_uri = "http://localhost:5044"
            };

            // Act
            var result = await _controller.PostAsync(request);
            BadRequestObjectResult badResult = (BadRequestObjectResult)result;

            // Assert
            Assert.True(result.GetType() == typeof(BadRequestObjectResult));
            Assert.True(badResult.StatusCode == (int)HttpStatusCode.BadRequest);
        }
        [Fact]
        public async void WhenControllerIsCalled_EmptyTicket_ThenItShouldReturn_BadRequest400Response()
        {
            // Arrange

            var request = new TokenIntegrationRequestModel
            {
                rqp = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c",
                ticket = string.Empty,
                as_uri = "http://localhost:5044"
            };

            // Act
            var result = await _controller.PostAsync(request);
            BadRequestObjectResult badResult = (BadRequestObjectResult)result;

            // Assert
            Assert.True(result.GetType() == typeof(BadRequestObjectResult));
            Assert.True(badResult.StatusCode == (int)HttpStatusCode.BadRequest);
        }
        [Fact]
        public async void WhenControllerIsCalled_EmptyAsUri_ThenItShouldReturn_BadRequest400Response()
        {
            // Arrange

            var request = new TokenIntegrationRequestModel
            {
                rqp = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c",
                ticket = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c",
                as_uri = string.Empty
            };

            // Act
            var result = await _controller.PostAsync(request);
            BadRequestObjectResult badResult = (BadRequestObjectResult)result;

            // Assert
            Assert.True(result.GetType() == typeof(BadRequestObjectResult));
            Assert.True(badResult.StatusCode == (int)HttpStatusCode.BadRequest);
        }
        [Fact]
        public async void WhenControllerIsCalled_NoRqp_ThenItShouldReturn_BadRequest400Response()
        {
            // Arrange

            var request = new TokenIntegrationRequestModel
            {                
                ticket = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c",
                as_uri = "http://localhost:5044"
            };

            // Act
            var result = await _controller.PostAsync(request);
            BadRequestObjectResult badResult = (BadRequestObjectResult)result;

            // Assert
            Assert.True(result.GetType() == typeof(BadRequestObjectResult));
            Assert.True(badResult.StatusCode == (int)HttpStatusCode.BadRequest);
        }
        [Fact]
        public async void WhenControllerIsCalled_NoTicket_ThenItShouldReturn_BadRequest400Response()
        {
            // Arrange

            var request = new TokenIntegrationRequestModel
            {
                rqp = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c",
                as_uri = "http://localhost:5044"
            };

            // Act
            var result = await _controller.PostAsync(request);
            BadRequestObjectResult badResult = (BadRequestObjectResult)result;

            // Assert
            Assert.True(result.GetType() == typeof(BadRequestObjectResult));
            Assert.True(badResult.StatusCode == (int)HttpStatusCode.BadRequest);
        }
        [Fact]
        public async void WhenControllerIsCalled_NoAsUri_ThenItShouldReturn_BadRequest400Response()
        {
            // Arrange

            var request = new TokenIntegrationRequestModel
            {
                rqp = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c",
                ticket = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c"
               
            };

            // Act
            var result = await _controller.PostAsync(request);
            BadRequestObjectResult badResult = (BadRequestObjectResult)result;

            // Assert
            Assert.True(result.GetType() == typeof(BadRequestObjectResult));
            Assert.True(badResult.StatusCode == (int)HttpStatusCode.BadRequest);
        }
    }
}