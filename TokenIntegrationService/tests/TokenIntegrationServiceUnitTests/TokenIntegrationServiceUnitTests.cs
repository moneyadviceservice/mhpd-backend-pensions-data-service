using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TokenIntegrationService.Controllers;
using TokenIntegrationService.HttpClients;
using TokenIntegrationService.Models;
using Moq;

namespace TokenIntegrationServiceUnitTests
{
    public class TokenIntegrationServiceUnitTests
    {
        private readonly DefaultHttpContext _httpContext;
        private readonly TokenController _controller;
        private readonly Mock<ICDATokenService> _iCDAToken = new Mock<ICDATokenService>();
        private const string RqpValue = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";
        private const string TicketValue = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";
        private const string As_UriValue = "http://localhost:5044";
        private const string InvalidRqpValue = "ABC123InvalidRqpValue";
        private const string InvalidTicketValue = "ZYZ123InvalidTicketValue";
        private const string SpecialCharAs_UriValue = "http://localhost:1234@#$%^&&&&.net";
        private const string SpecialCharRqpValue = "SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJVadQssw5c@@##$%_+!!**({{^?,,,@";
        private const string SpecialCharTicketValue = "eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.ASDFG@@##$%_+!!**({{^?,,,@";

        public TokenIntegrationServiceUnitTests()
        {
            _httpContext = new DefaultHttpContext();
            _controller = new TokenController(_iCDAToken.Object)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = _httpContext
                }
            };

            _iCDAToken.Setup(x => x.PostRpt(It.IsAny<CDATokenRequestModel>())).Returns(Task.FromResult<RptsModel>(new RptsModel { AccessToken = RqpValue }));
        }       
        
        [Fact]
        public async void WhenControllerIsCalled_WithValidRequestBody_ThenItShouldReturn_OKReques200Response()
        {
            // Arrange           
            var request = new TokenIntegrationRequestModel
            {
                Rqp = RqpValue,
                Ticket = TicketValue,
                As_Uri = As_UriValue
            };

            // Act
            var result = await _controller.PostAsync(request); 
            OkObjectResult okResult = (OkObjectResult)result;
            var data = (TokenIntegrationResponseModel)okResult!.Value!;

            // Assert
            Assert.NotNull(result);
            Assert.True(result.GetType() == typeof(OkObjectResult));
            Assert.True(data.GetType() == typeof(TokenIntegrationResponseModel));
            Assert.True(okResult.StatusCode == (int)HttpStatusCode.OK);
        }

        [Fact]
        public async void WhenControllerIsCalled_EmptyRqp_ThenItShouldReturn_BadRequest400Response()
        {
            // Arrange
            var request = new TokenIntegrationRequestModel
            {
                Rqp = string.Empty,
                Ticket = TicketValue,
                As_Uri = As_UriValue
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
                Rqp = RqpValue,
                Ticket = string.Empty,
                As_Uri = As_UriValue
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
                Rqp = RqpValue,
                Ticket = TicketValue,
                As_Uri = string.Empty
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
                Ticket = TicketValue,
                As_Uri = As_UriValue
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
                Rqp = RqpValue,
                As_Uri = As_UriValue
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
                Rqp = RqpValue,
                Ticket = TicketValue
            };

            // Act
            var result = await _controller.PostAsync(request);
            BadRequestObjectResult badResult = (BadRequestObjectResult)result;

            // Assert
            Assert.True(result.GetType() == typeof(BadRequestObjectResult));
            Assert.True(badResult.StatusCode == (int)HttpStatusCode.BadRequest);
        }

        [Fact]
        public async void WhenControllerIsCalled_NoValues_ThenItShouldReturn_BadRequest400Response()
        {
            // Arrange
            var request = new TokenIntegrationRequestModel { };

            // Act
            var result = await _controller.PostAsync(request);
            BadRequestObjectResult badResult = (BadRequestObjectResult)result;

            // Assert
            Assert.True(result.GetType() == typeof(BadRequestObjectResult));
            Assert.True(badResult.StatusCode == (int)HttpStatusCode.BadRequest);
        }

        [Fact]
        public async void WhenControllerIsCalled_AllEmptyValues_ThenItShouldReturn_BadRequest400Response()
        {
            // Arrange
            var request = new TokenIntegrationRequestModel
            {
                Rqp = string.Empty,
                Ticket = string.Empty,
                As_Uri = string.Empty
            };

            // Act
            var result = await _controller.PostAsync(request);
            BadRequestObjectResult badResult = (BadRequestObjectResult)result;

            // Assert
            Assert.True(result.GetType() == typeof(BadRequestObjectResult));
            Assert.True(badResult.StatusCode == (int)HttpStatusCode.BadRequest);
        }

        [Fact]
        public async void WhenControllerIsCalled_InvalidRqp_ThenItShouldReturn_OKReques200Response()
        {
            // Arrange
            var request = new TokenIntegrationRequestModel
            {
                Rqp = InvalidRqpValue,
                Ticket = TicketValue,
                As_Uri = As_UriValue
            };

            // Act
            var result = await _controller.PostAsync(request);
            OkObjectResult okResult = (OkObjectResult)result;
            var data = (TokenIntegrationResponseModel)okResult!.Value!;

            // Assert
            Assert.NotNull(result);
            Assert.True(result.GetType() == typeof(OkObjectResult));
            Assert.True(data.GetType() == typeof(TokenIntegrationResponseModel));
            Assert.True(okResult.StatusCode == (int)HttpStatusCode.OK);
        }

        [Fact]
        public async void WhenControllerIsCalled_InvalidTicket_ThenItShouldReturn_OKReques200Response()
        {
            // Arrange
            var request = new TokenIntegrationRequestModel
            {
                Rqp = RqpValue,
                Ticket = InvalidTicketValue,
                As_Uri = As_UriValue
            };

            // Act
            var result = await _controller.PostAsync(request);
            OkObjectResult okResult = (OkObjectResult)result;
            var data = (TokenIntegrationResponseModel)okResult!.Value!;

            // Assert
            Assert.NotNull(result);
            Assert.True(result.GetType() == typeof(OkObjectResult));
            Assert.True(data.GetType() == typeof(TokenIntegrationResponseModel));
            Assert.True(okResult.StatusCode == (int)HttpStatusCode.OK);
        }

        [Fact]
        public async void WhenControllerIsCalled_InvalidRQPandTicket_ThenItShouldReturn_OKReques200Response()
        {
            // Arrange
            var request = new TokenIntegrationRequestModel
            {
                Rqp = InvalidRqpValue,
                Ticket = InvalidTicketValue,
                As_Uri = As_UriValue
            };

            // Act
            var result = await _controller.PostAsync(request);
            OkObjectResult okResult = (OkObjectResult)result;
            var data = (TokenIntegrationResponseModel)okResult!.Value!;

            // Assert
            Assert.NotNull(result);
            Assert.True(result.GetType() == typeof(OkObjectResult));
            Assert.True(data.GetType() == typeof(TokenIntegrationResponseModel));
            Assert.True(okResult.StatusCode == (int)HttpStatusCode.OK);
        }

        [Fact]
        public async void WhenControllerIsCalled_SpecialCharRqp_ThenItShouldReturn_BadRequest400Response()
        {
            // Arrange
            var request = new TokenIntegrationRequestModel
            {
                Rqp = SpecialCharRqpValue,
                Ticket = TicketValue,
                As_Uri = As_UriValue
            };

            // Act
            var result = await _controller.PostAsync(request);
            BadRequestObjectResult badResult = (BadRequestObjectResult)result;

            // Assert
            Assert.True(result.GetType() == typeof(BadRequestObjectResult));
            Assert.True(badResult.StatusCode == (int)HttpStatusCode.BadRequest);
        }

        [Fact]
        public async void WhenControllerIsCalled_SpecialCharAs_Uri_ThenItShouldReturn_BadRequest400Response()
        {
            // Arrange
            var request = new TokenIntegrationRequestModel
            {
                Rqp = RqpValue,
                Ticket = TicketValue,
                As_Uri = SpecialCharAs_UriValue
            };

            // Act
            var result = await _controller.PostAsync(request);
            BadRequestObjectResult badResult = (BadRequestObjectResult)result;

            // Assert
            Assert.True(result.GetType() == typeof(BadRequestObjectResult));
            Assert.True(badResult.StatusCode == (int)HttpStatusCode.BadRequest);
        }

        [Fact]
        public async void WhenControllerIsCalled_SpecialCharTicketValue_ThenItShouldReturn_BadRequest400Response()
        {
            // Arrange
            var request = new TokenIntegrationRequestModel
            {
                Rqp = RqpValue,
                Ticket = SpecialCharTicketValue,
                As_Uri = As_UriValue
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