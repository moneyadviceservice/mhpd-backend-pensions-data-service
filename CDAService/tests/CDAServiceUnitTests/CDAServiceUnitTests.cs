using System.Net;
using CDAService.Controllers;
using CDAService.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using static CDAService.Utils.RSA256TokenUtils;

namespace CDAServiceUnitTests
{
    public class CDAServiceUnitTests
    {
        private readonly DefaultHttpContext _httpContext;
        private readonly CDAServiceController _controller;
     
        public CDAServiceUnitTests()
        {
            _httpContext = new DefaultHttpContext();
            _controller = new CDAServiceController()
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = _httpContext
                }
            };
        } 

        [Fact]
        public async Task GivenValidInput_WhenPostIsCalled_ThenReturnsOk()
        {
            // Arrange
            var validRequest = new RPQRequestModel
            {
                Iss = "myapp.com",
                UserSessionId = "mySessionId-123abcd"
            };

            // Act
            var result = await _controller.PostAsync(validRequest) as OkObjectResult;

            //Assert
            Assert.NotNull(result);
            Assert.IsType<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.NotNull(okResult);
            var response = okResult.Value; 
            Assert.NotNull(response);
        }

        [Fact]
        public async Task GivenValidInput_WhenPostIsCalledAndTokenValidated_ThenTokenValidatesSuccessfully()
        {
            // Arrange
            var validRequest = new RPQRequestModel
            {
                Iss = "myapp.com",
                UserSessionId = "mySessionId-123abcd"
            };

            var _tokenManager = new RQPTokenManager(validRequest.Iss, validRequest.UserSessionId);

            // Act
            var result = await _controller.PostAsync(validRequest) as OkObjectResult;
            var generatedToken = ((RQPResponseModel)result!.Value!).Rqp;
            var valid = _tokenManager.ValidateToken(generatedToken!, out RQPModel rqpModel);

            //Assert
            Assert.NotNull(result);
            Assert.True(valid == true);
            Assert.True(rqpModel.Issuer == validRequest.Iss);
            Assert.Contains(validRequest.UserSessionId, rqpModel.Subject!);
        }

        [Fact]
        public async Task GivenInValidInputs_WhenPostIsCalled_ThenReturnsBadRequest()
        {
            //Arrange
            var request = new RPQRequestModel
            {
                Iss = string.Empty,
                UserSessionId = string.Empty
            };

            //Act
            var result = await _controller.PostAsync(request);
            BadRequestObjectResult badResult = (BadRequestObjectResult)result;

            //Assert
            Assert.True(badResult.StatusCode == (int)HttpStatusCode.BadRequest);
            
        }

        [Fact]
        public async Task GivenInvalidIss_WhenPostIsCalled_ThenReturnsBadRequest ()
        {
            //Arrange
            var request = new RPQRequestModel
            {
                Iss = string.Empty,
                UserSessionId = "mySessionId - 123abcd"
            };

            //Act
            var result = await _controller.PostAsync(request);
            BadRequestObjectResult badResult = (BadRequestObjectResult)result;

            //Assert
            Assert.True(badResult.StatusCode == (int)HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GivenInvalidUserSessionId_WhenPostIsCalled_ThenReturnsBadRequest()
        {
            //Arrange
            var request = new RPQRequestModel
            {
                Iss = "myapp.com",
                UserSessionId = string.Empty
            };

            //Act
            var result =await _controller.PostAsync(request);
            BadRequestObjectResult badResult = (BadRequestObjectResult)result;

            //Assert
            Assert.True(badResult.StatusCode == (int)HttpStatusCode.BadRequest);
        }
    }
}