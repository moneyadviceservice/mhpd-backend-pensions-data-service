using CDATokenServices.Controllers;
using CDATokenServices.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.DataCollection;
using System.Net;

namespace CDATokenServicesUnitTests
{
    public class CDATokenServicesUnitTest
    {
        private readonly DefaultHttpContext _httpContext;
        private readonly CDATokenController _controller;       
        
        public CDATokenServicesUnitTest()
        {
            _httpContext = new DefaultHttpContext();
            _httpContext.Request.Headers["X-Request-ID"] = "sdfasdfasdasdadsg";

            _controller = new CDATokenController()
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = _httpContext
                }
            };
        }

        [Fact]
        public async void WhenControllerIsCalled_WithCorrectHeaders_ValidRequestBody_ThenItShouldReturn_OKReques200Response()
        {
            // Arrange
            _httpContext.Request.Headers["X-Request-ID"] = "sdfasdfasdasdadsg";
            var request = new CDATokenRequestModel
            {
                GrantType = "urn:ietf:params:oauth:grant-type:uma-ticket",
                Ticket = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c.",
                ClaimTokenFormat = "pension_dashboad_rqp",
                ClaimToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c",
                Scope = "owner"
            };

            // Act
            var result = await _controller.PostAsync(request);

            // Assert
            Assert.True(result.GetType() == typeof(OkObjectResult));
        }

        [Fact]
        public async void WhenControllerIsCalled_EmptyScope_WithCorrectHeaders_ThenItShouldReturn_BadRequest400Response()
        {
            // Arrange
            AddAuthorisationHeader();          
            var request = new CDATokenRequestModel
            {
                GrantType = "urn:ietf:params:oauth:grant-type:uma-ticket",
                Ticket = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c.",
                ClaimTokenFormat = "pension_dashboad_rqp",
                ClaimToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c",
                Scope =string.Empty
                
            };

            // Act
            var result = await _controller.PostAsync(request);
            BadRequestObjectResult badResult = (BadRequestObjectResult)result;

            // Assert
            Assert.True(result.GetType() == typeof(BadRequestObjectResult));
            Assert.True(badResult.StatusCode == (int)HttpStatusCode.BadRequest);
        }
        [Fact]
        public async void WhenControllerIsCalled_NoScope_WithCorrectHeaders_ThenItShouldReturn_BadRequest400Response()
        {
            // Arrange
            AddAuthorisationHeader();
            var request = new CDATokenRequestModel
            {
                GrantType = "urn:ietf:params:oauth:grant-type:uma-ticket",
                Ticket = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c.",
                ClaimTokenFormat = "pension_dashboad_rqp",
                ClaimToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c"
                
            };

            // Act
            var result = await _controller.PostAsync(request);
            BadRequestObjectResult badResult = (BadRequestObjectResult)result;

            // Assert
            Assert.True(result.GetType() == typeof(BadRequestObjectResult));
            Assert.True(badResult.StatusCode == (int)HttpStatusCode.BadRequest);
        }

        [Fact]
        public async void WhenControllerIsCalled_InvalidScope_WithCorrectHeaders_ThenItShouldReturn_BadRequest400Response()
        {
            // Arrange
            AddAuthorisationHeader();           

            var request = new CDATokenRequestModel
            {
                GrantType = "urn:ietf:params:oauth:grant-type:uma-ticket",
                Ticket = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c.",
                ClaimTokenFormat = "pension_dashboad_rqp",
                ClaimToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c",
                Scope = "abc"
            };

            // Act
            var result = await _controller.PostAsync(request);
            BadRequestObjectResult badResult = (BadRequestObjectResult)result;           

            // Assert
            Assert.True(result.GetType() == typeof(BadRequestObjectResult));
            Assert.True(badResult.StatusCode == (int)HttpStatusCode.BadRequest);
        }
       
        [Fact]
        public async void WhenControllerIsCalled_EmptyClaimToken_WithCorrectHeaders_ThenItShouldReturn_BadRequest400Response()
        {
            // Arrange
           
            var request = new CDATokenRequestModel
            {
                GrantType = "urn:ietf:params:oauth:grant-type:uma-ticket",
                Ticket = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c.",
                ClaimToken = string.Empty,
                ClaimTokenFormat = "pension_dashboad_rqp",
                Scope ="owner"

            };

            // Act
            var result = await _controller.PostAsync(request);
            BadRequestObjectResult badResult = (BadRequestObjectResult)result;

            // Assert
            Assert.True(result.GetType() == typeof(BadRequestObjectResult));
            Assert.True(badResult.StatusCode == (int)HttpStatusCode.BadRequest);
        }
        [Fact]
        public async void WhenControllerIsCalled_NoClaimToken_WithCorrectHeaders_ThenItShouldReturn_BadRequest400Response()
        {
            // Arrange

            var request = new CDATokenRequestModel
            {
                GrantType = "urn:ietf:params:oauth:grant-type:uma-ticket",
                Ticket = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c.",
                ClaimTokenFormat = "pension_dashboad_rqp",
                Scope = "owner"

            };

            // Act
            var result = await _controller.PostAsync(request);
            BadRequestObjectResult badResult = (BadRequestObjectResult)result;

            // Assert
            Assert.True(result.GetType() == typeof(BadRequestObjectResult));
            Assert.True(badResult.StatusCode == (int)HttpStatusCode.BadRequest);
        }
        [Fact]
        public async void WhenControllerIsCalled_EmptyClaimTokenFormat_WithCorrectHeaders_ThenItShouldReturn_BadRequest400Response()
        {
            // Arrange

            var request = new CDATokenRequestModel
            {
                GrantType = "urn:ietf:params:oauth:grant-type:uma-ticket",
                Ticket = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c.",
                ClaimToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c",
                ClaimTokenFormat = string.Empty,
                Scope = "owner"

            };

            // Act
            var result = await _controller.PostAsync(request);
            BadRequestObjectResult badResult = (BadRequestObjectResult)result;

            // Assert
            Assert.True(result.GetType() == typeof(BadRequestObjectResult));
            Assert.True(badResult.StatusCode == (int)HttpStatusCode.BadRequest);
        }
        [Fact]
        public async void WhenControllerIsCalled_InvalidClaimTokenFormat_WithCorrectHeaders_ThenItShouldReturn_BadRequest400Response()
        {
            // Arrange

            var request = new CDATokenRequestModel
            {
                GrantType = "urn:ietf:params:oauth:grant-type:uma-ticket",
                Ticket = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c.",
                ClaimToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c",
                ClaimTokenFormat = "abc",
                Scope = "owner"

            };

            // Act
            var result = await _controller.PostAsync(request);
            BadRequestObjectResult badResult = (BadRequestObjectResult)result;

            // Assert
            Assert.True(result.GetType() == typeof(BadRequestObjectResult));
            Assert.True(badResult.StatusCode == (int)HttpStatusCode.BadRequest);
        }
        [Fact]
        public async void WhenControllerIsCalled_NoClaimTokenFormat_WithCorrectHeaders_ThenItShouldReturn_BadRequest400Response()
        {
            // Arrange

            var request = new CDATokenRequestModel
            {
                GrantType = "urn:ietf:params:oauth:grant-type:uma-ticket",
                Ticket = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c.",
                ClaimToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c",
                Scope = "owner"

            };

            // Act
            var result = await _controller.PostAsync(request);
            BadRequestObjectResult badResult = (BadRequestObjectResult)result;

            // Assert
            Assert.True(result.GetType() == typeof(BadRequestObjectResult));
            Assert.True(badResult.StatusCode == (int)HttpStatusCode.BadRequest);
        }
        [Fact]
        public async void WhenControllerIsCalled_NoGrantType_WithCorrectHeaders_ThenItShouldReturn_BadRequest400Response()
        {
            // Arrange

            var request = new CDATokenRequestModel
            {
                Ticket = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c.",
                ClaimToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c",
                ClaimTokenFormat = "pension_dashboad_rqp",
                Scope = "owner"

            };

            // Act
            var result = await _controller.PostAsync(request);
            BadRequestObjectResult badResult = (BadRequestObjectResult)result;

            // Assert
            Assert.True(result.GetType() == typeof(BadRequestObjectResult));
            Assert.True(badResult.StatusCode == (int)HttpStatusCode.BadRequest);
        }
        [Fact]
        public async void WhenControllerIsCalled_InvalidGrantType_WithCorrectHeaders_ThenItShouldReturn_BadRequest400Response()
        {
            // Arrange

            var request = new CDATokenRequestModel
            {
                GrantType = "abc",
                Ticket = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c.",
                ClaimToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c",
                ClaimTokenFormat = "pension_dashboad_rqp",
                Scope = "owner"

            };

            // Act
            var result = await _controller.PostAsync(request);
            BadRequestObjectResult badResult = (BadRequestObjectResult)result;

            // Assert
            Assert.True(result.GetType() == typeof(BadRequestObjectResult));
            Assert.True(badResult.StatusCode == (int)HttpStatusCode.BadRequest);
        }
        [Fact]
        public async void WhenControllerIsCalled_EmptyGrantType_WithCorrectHeaders_ThenItShouldReturn_BadRequest400Response()
        {
            // Arrange

            var request = new CDATokenRequestModel
            {
                GrantType = "uurn:ietf:params:oauth:grant-type:uma-ticket",
                Ticket = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c.",
                ClaimToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c",
                ClaimTokenFormat = "pension_dashboad_rqp",
                Scope = "owner"

            };

            // Act
            var result = await _controller.PostAsync(request);
            BadRequestObjectResult badResult = (BadRequestObjectResult)result;

            // Assert
            Assert.True(result.GetType() == typeof(BadRequestObjectResult));
            Assert.True(badResult.StatusCode == (int)HttpStatusCode.BadRequest);
        }
        [Fact]
        public async void WhenControllerIsCalled_EmptyTicket_WithCorrectHeaders_ThenItShouldReturn_BadRequest400Response()
        {
            // Arrange

            var request = new CDATokenRequestModel
            {
                GrantType = "urn:ietf:params:oauth:grant-type:uma-ticket",
                Ticket = string.Empty,
                ClaimTokenFormat = "pension_dashboad_rqp",
                ClaimToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c",
                Scope = "owner"

            };

            // Act
            var result = await _controller.PostAsync(request);
            BadRequestObjectResult badResult = (BadRequestObjectResult)result;

            // Assert
            Assert.True(result.GetType() == typeof(BadRequestObjectResult));
            Assert.True(badResult.StatusCode == (int)HttpStatusCode.BadRequest);
        }
        [Fact]
        public async void WhenControllerIsCalled_NoTicket_WithCorrectHeaders_ThenItShouldReturn_BadRequest400Response()
        {
            // Arrange

            var request = new CDATokenRequestModel
            {
                GrantType = "urn:ietf:params:oauth:grant-type:uma-ticket",                
                ClaimTokenFormat = "pension_dashboad_rqp",
                ClaimToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c",
                Scope = "owner"

            };

            // Act
            var result = await _controller.PostAsync(request);
            BadRequestObjectResult badResult = (BadRequestObjectResult)result;

            // Assert
            Assert.True(result.GetType() == typeof(BadRequestObjectResult));
            Assert.True(badResult.StatusCode == (int)HttpStatusCode.BadRequest);
        }

        [Fact]
        public async void WhenControllerIsCalled_EmptyHeaders_ThenItShouldReturn_Unauthorised401Response()
        {
            // Arrange
            _httpContext.Request.Headers["X-Request-ID"] = string.Empty;
            var request = new CDATokenRequestModel
            {
                GrantType = "urn:ietf:params:oauth:grant-type:uma-ticket",
                Ticket = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c.",
                ClaimTokenFormat = "pension_dashboad_rqp",
                ClaimToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c",
                Scope = "owner"

            };

            // Act
            var result = await _controller.PostAsync(request);           
            UnauthorizedObjectResult unAuthorizedResult = (UnauthorizedObjectResult)result;
            // Assert
                       Assert.True(result.GetType() == typeof(UnauthorizedObjectResult));
            Assert.True(unAuthorizedResult.StatusCode == (int)HttpStatusCode.Unauthorized);
        }
        private void AddAuthorisationHeader()
        {
            _httpContext.Request.Headers["Authorisation"] = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";
        }
    }
}