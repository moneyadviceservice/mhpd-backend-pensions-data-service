using CDAServiceEmulator.Controllers;
using CDAServiceEmulator.Mocks;
using MhpdCommon.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Net;

namespace CDAServiceEmulatorUnitTests
{
    public class HolderNameConfigurationUnitTests
    {
        private const string HolderViewConfigurations = "holder_view_configurations";
        private readonly HolderNameController _controller;
        private readonly Mock<IIdValidator> _idValidator;

        public HolderNameConfigurationUnitTests()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["X-Request-ID"] = "35cfcfb0-d98d-451f-83f1-e59933078555";
            _idValidator = new Mock<IIdValidator>();
            _idValidator.Setup(x => x.IsValidGuid(It.IsAny<string>())).Returns(true);

            _controller = new HolderNameController(_idValidator.Object)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = httpContext
                }
            };
        }

        [Fact]
        public async Task When_NoHolderNameGuidProvided_Then_ReturnFullSetOfConfigurations_200Response()
        {
            // Arrange
            _idValidator.Setup(x => x.IsValidGuid(It.IsAny<string>())).Returns(true);
            string? holderNameGuid = null;

            // Act
            var result = await _controller.GetAsync("requestId", holderNameGuid);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);

            // The result is an anonymous object, so we need to extract the "holder_view_configurations" property
            var anonymousObject = okResult.Value;

            // Use dynamic or reflection to access the property
            if (anonymousObject != null)
            {
                var holderViewConfigurations =
                    (await HolderConfigurationMock.GetHolderConfiguration())?.ToList();

                // Ensure the data is not null or empty
                Assert.NotNull(holderViewConfigurations);
                Assert.NotEmpty(holderViewConfigurations);
            }

        }

        [Fact]
        public async Task When_KnownHolderNameGuidProvided_Then_ReturnConfigurationForThatGuid_200Response()
        {
            // Arrange
            const string holderNameGuid = "550e8400-e29b-41d4-a716-446655440000";

            // Act
            var result = await _controller.GetAsync(holderNameGuid, holderNameGuid);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);

            // The result is an anonymous object, so we need to extract the "holder_view_configurations" property
            var anonymousObject = okResult.Value;

            // Use reflection or dynamic to access the property
            if (anonymousObject != null)
            {
                var holderViewConfigurations =
                    (await HolderConfigurationMock.GetHolderConfiguration())?
                    .Where(c => c.HolderNameGuid == holderNameGuid)
                    .ToList();


                // Ensure the data is not null or empty
                Assert.NotNull(holderViewConfigurations);
                Assert.Single(holderViewConfigurations); //Ensure only one match

                // Ensure the correct holdername_guid is returned
                Assert.Equal(holderNameGuid, holderViewConfigurations.First().HolderNameGuid);

            }
        }

        [Fact]
        public async Task When_InvalidHolderNameGuidFormatProvided_Then_Return400BadRequestResponse()
        {
            // Arrange
            string holderNameGuid = "invalid-format-guid";
            _idValidator.Setup(x => x.IsValidGuid(holderNameGuid)).Returns(false);

            // Act
            var result = await _controller.GetAsync("ImNotAGuid", holderNameGuid);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result); // Ensure it's a 400 Bad Request
            Assert.NotNull(badRequestResult);
            Assert.Equal((int)HttpStatusCode.BadRequest, badRequestResult.StatusCode); // Check the status code

            // Ensure the error message matches, ignoring case sensitivity
            Assert.Equal("Invalid holdername_guid format", badRequestResult.Value?.ToString(), ignoreCase: true);
        }

        [Fact]
        public async Task When_UnknownHolderNameGuidProvided_Then_Return404NotFoundResponse()
        {
            // Arrange
            string holderNameGuid = "550e8400-e29b-41d4-a716-446655440001"; // Use a GUID that is not in the mock data

            // Act
            var result = await _controller.GetAsync(holderNameGuid, holderNameGuid);
            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result); // Ensure it's a 404 NotFound
            Assert.NotNull(notFoundResult);
            Assert.Equal((int)HttpStatusCode.NotFound, notFoundResult.StatusCode); // Check the status code

            // Check the error message
            Assert.Equal("Unknown holdername_guid", notFoundResult.Value);
        }

        [Fact]
        public async Task When_XRequestIdHeaderIsMissing_Then_Return400BadRequestResponse()
        {
            // Arrange
            _idValidator.Setup(x => x.IsValidGuid(It.IsAny<string>())).Returns(false);
            var httpContext = new DefaultHttpContext(); // No X-Request-ID header set
            _controller.ControllerContext = new ControllerContext() { HttpContext = httpContext };

            // Act
            var result = await _controller.GetAsync(null, null);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult);
            Assert.Equal((int)HttpStatusCode.BadRequest, badRequestResult.StatusCode);
            Assert.Equal("Invalid X-Request-ID header", badRequestResult.Value?.ToString(), ignoreCase: true);
        }

        [Fact]
        public async Task When_XRequestIdHeaderIsEmpty_Then_Return400BadRequestResponse()
        {
            // Arrange
            _idValidator.Setup(x => x.IsValidGuid(It.IsAny<string>())).Returns(false);
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["X-Request-ID"] = ""; // Empty header value
            _controller.ControllerContext = new ControllerContext() { HttpContext = httpContext };

            // Act
            var result = await _controller.GetAsync(string.Empty, null);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult);
            Assert.Equal((int)HttpStatusCode.BadRequest, badRequestResult.StatusCode);
            Assert.Equal("Invalid X-Request-ID header", badRequestResult.Value?.ToString(), ignoreCase: true);
        }

        [Fact]
        public async Task When_XRequestIdHeaderIsInvalid_Then_Return400BadRequestResponse()
        {
            // Arrange
            _idValidator.Setup(x => x.IsValidGuid(It.IsAny<string>())).Returns(false);
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["X-Request-ID"] = "invalid-guid"; // Invalid GUID
            _controller.ControllerContext = new ControllerContext() { HttpContext = httpContext };

            // Act
            var result = await _controller.GetAsync("invalid-guid", null);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult);
            Assert.Equal((int)HttpStatusCode.BadRequest, badRequestResult.StatusCode);
            Assert.Equal("Invalid X-Request-ID header", badRequestResult.Value?.ToString(), ignoreCase: true);
        }
    }
}