using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PeiIntegratioinService.Controllers;
using PeiIntegratioinService.HttpClients;
using PeiIntegrationService.Models;

namespace PeiIntegrationService.UnitTests
{
    public class PeiIntegrationServiceUnitTests
    {
        private readonly DefaultHttpContext _httpContext;
        private readonly PeIController _controller;

        readonly Mock<ICDAService> _iCDAService = new Mock<ICDAService>();

        public PeiIntegrationServiceUnitTests()
        {
            _httpContext = new DefaultHttpContext();

            _controller = new PeIController(_iCDAService.Object)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = _httpContext
                }
            };

            _iCDAService.Setup(x => x.GetPies(It.IsAny<CDAServiceRequestModel>())).Returns(Task.FromResult<PeiModel[]?>(new PeiModel[] { }));
        }

        [Fact]
        public async void WhenControllerIsCalled_WithCorrectHeaders_CorrectBody_ThenItShouldReturn_Ok200Response()
        {
            //Arrange
            AddAuthorisationHeader();
            AddOtherHeaders("cd0e4fdc-8586-4483-9899-17dd85af9074", "https://maps.com", "askdj902139012ekasdlasdj");
            var request = new PeiIntegrationServiceRequestModel { RequestId = "qwertyuoip", PeisBaseUrl = "http://localhost:5089" };

            // Act
            var result = await _controller.GetAsync(request);
            OkObjectResult okResult = (OkObjectResult)result;
            var data = (PeiModel[])okResult!.Value!;

            // Assert
            Assert.NotNull(result);
            Assert.True(result.GetType() == typeof(OkObjectResult));
            Assert.True(data.GetType() == typeof(PeiModel[]));
            Assert.True(okResult.StatusCode == (int)HttpStatusCode.OK);
        }

        [Fact]
        public async void WhenControllerIsCalled_WithoutAuthHeader_ButWithOtherHeaders_CorrectBody_ThenItShouldReturn_Unauthorized401Response()
        {
            //Arrange
            AddOtherHeaders("cd0e4fdc-8586-4483-9899-17dd85af9074", "https://maps.com", "askdj902139012ekasdlasdj");
            var request = new PeiIntegrationServiceRequestModel { RequestId = "qwertyuoip", PeisBaseUrl = "http://localhost:5089" };

            // Act
            var result = await _controller.GetAsync(request);

            // Assert
            Assert.True(result.GetType() == typeof(UnauthorizedObjectResult));

        }

        [Fact]
        public async void WhenControllerIsCalled_WithCorrectHeader_IncompleteBody_ThenItShouldReturn_BadRequest400Response()
        {
            //Arrange
            AddOtherHeaders("cd0e4fdc-8586-4483-9899-17dd85af9074", "https://maps.com", "askdj902139012ekasdlasdj");
            var request = new PeiIntegrationServiceRequestModel { RequestId = "qwertyuoip", PeisBaseUrl = "" };

            // Act
            var result = await _controller.GetAsync(request);

            // Assert
            Assert.True(result.GetType() == typeof(BadRequestObjectResult));

        }

        [Fact]
        public async void WhenControllerIsCalled_WithHeader_AnotherIncompleteBody_ThenItShouldReturn_BadRequest400Response()
        {
            //Arrange
            AddAuthorisationHeader();
            AddOtherHeaders("cd0e4fdc-8586-4483-9899-17dd85af9074", "https://maps.com", "askdj902139012ekasdlasdj");
            var request = new PeiIntegrationServiceRequestModel { RequestId = "", PeisBaseUrl = "http://localhost:5089" };

            // Act
            var result = await _controller.GetAsync(request);

            // Assert
            Assert.True(result.GetType() == typeof(BadRequestObjectResult));

        }

        [Fact]
        public async void WhenControllerIsCalled_WithoutUserGuidHeader_CorrectBody_ThenItShouldReturn_Unauthorized401Response()
        {
            //Arrange
            AddAuthorisationHeader();
            AddOtherHeaders("", "https://maps.com", "askdj902139012ekasdlasdj");
            var request = new PeiIntegrationServiceRequestModel { RequestId = "", PeisBaseUrl = "http://localhost:5089" };

            // Act
            var result = await _controller.GetAsync(request);

            // Assert
            Assert.True(result.GetType() == typeof(UnauthorizedObjectResult));

        }

        [Fact]
        public async void WhenControllerIsCalled_WithoutIssHeader_CorrectBody_ThenItShouldReturn_Unauthorized401Response()
        {
            //Arrange
            AddAuthorisationHeader();
            AddOtherHeaders("cd0e4fdc-8586-4483-9899-17dd85af9074", "", "askdj902139012ekasdlasdj");
            var request = new PeiIntegrationServiceRequestModel { RequestId = "", PeisBaseUrl = "http://localhost:5089" };

            // Act
            var result = await _controller.GetAsync(request);

            // Assert
            Assert.True(result.GetType() == typeof(UnauthorizedObjectResult));

        }

        [Fact]
        public async void WhenControllerIsCalled_WithoutUserSessionHeader_CorrectBody_ThenItShouldReturn_Unauthorized401Response()
        {
            //Arrange
            AddAuthorisationHeader();
            AddOtherHeaders("cd0e4fdc-8586-4483-9899-17dd85af9074", "https://maps.com", "");
            var request = new PeiIntegrationServiceRequestModel { RequestId = "", PeisBaseUrl = "http://localhost:5089" };

            // Act
            var result = await _controller.GetAsync(request);

            // Assert
            Assert.True(result.GetType() == typeof(UnauthorizedObjectResult));

        }

        private void AddAuthorisationHeader()
        {
            _httpContext.Request.Headers["rpt"] = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";
        }

        private void AddOtherHeaders (string guid, string iss, string userSessionId)
        {
            _httpContext.Request.Headers["cdaUserGuid"] = guid;
            _httpContext.Request.Headers["iss"] = iss;
            _httpContext.Request.Headers["userSessionId"] = userSessionId;
        }
    }
}
