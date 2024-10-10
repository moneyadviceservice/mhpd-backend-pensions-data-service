using CDAServiceEmulator.Controllers;
using CDAServiceEmulator.CosmosRepository;
using CDAServiceEmulator.Models;
using CDAServiceEmulator.Models.HolderConfiguration;
using CDAServiceEmulatorUnitTests.Mock;
using MhpdCommon.Constants;
using MhpdCommon.Models.RequestHeaderModel;
using MhpdCommon.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Net;

namespace CDAServiceEmulatorUnitTests
{
    public class HolderNameConfigurationUnitTests
    {
        private readonly HolderNameController _controller;
        private readonly Mock<IIdValidator> _idValidator;
        private readonly Mock<IHolderNameViewDataRepository<HolderNameConfigurationModel>> _repository;

        public HolderNameConfigurationUnitTests()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[HeaderConstants.RequestId] = Guid.NewGuid().ToString();
            _idValidator = new Mock<IIdValidator>();
            _idValidator.Setup(x => x.IsValidGuid(It.IsAny<string>())).Returns(true);

            _repository = new Mock<IHolderNameViewDataRepository<HolderNameConfigurationModel>>();
            _repository.Setup(x => x.GetByIdAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync((string id, string key) => HolderConfigurationMock.FilterConfigurations(id));
            _repository.Setup(x => x.GetHolderNameConfigurationsAsync()).ReturnsAsync(HolderConfigurationMock.GetHolderConfiguration());

            _controller = new HolderNameController(_idValidator.Object, _repository.Object)
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
            var result = await _controller.GetAsync(new RequestHeaderModel { XRequestId = "requestId" }, holderNameGuid);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<HolderNameViewDataResponse>(okResult.Value);
            Assert.NotNull(response);
            Assert.Equal(2, response.Configurations.Count);
        }

        [Fact]
        public async Task When_KnownHolderNameGuidProvided_Then_ReturnConfigurationForThatGuid_200Response()
        {
            // Arrange
            const string holderNameGuid = HolderConfigurationMock.MatchingId;

            // Act
            var result = await _controller.GetAsync(new RequestHeaderModel { XRequestId = holderNameGuid }, holderNameGuid);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<HolderNameViewDataResponse>(okResult.Value);
            Assert.NotNull(response);
            Assert.Single(response.Configurations);
        }

        [Fact]
        public async Task When_InvalidHolderNameGuidFormatProvided_Then_Return400BadRequestResponse()
        {
            // Arrange
            string holderNameGuid = "invalid-format-guid";
            _idValidator.Setup(x => x.IsValidGuid(holderNameGuid)).Returns(false);

            // Act
            var result = await _controller.GetAsync(new RequestHeaderModel { XRequestId = "ImNotAGuid" }, holderNameGuid);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result); // Ensure it's a 400 Bad Request
            Assert.NotNull(badRequestResult);
            Assert.Equal((int)HttpStatusCode.BadRequest, badRequestResult.StatusCode); // Check the status code

            // Ensure the error message matches, ignoring case sensitivity
            Assert.Equal(Constants.HolderNameConstants.InvalidHolderNameId, badRequestResult.Value?.ToString());
        }

        [Fact]
        public async Task When_UnknownHolderNameGuidProvided_Then_Return404NotFoundResponse()
        {
            // Arrange
            string holderNameGuid = "550e8400-e29b-41d4-a716-446655440001"; // Use a GUID that is not in the mock data

            // Act
            var result = await _controller.GetAsync(new RequestHeaderModel { XRequestId = holderNameGuid }, holderNameGuid);
            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result); // Ensure it's a 404 NotFound
            Assert.NotNull(notFoundResult);
            Assert.Equal((int)HttpStatusCode.NotFound, notFoundResult.StatusCode); // Check the status code

            // Check the error message
            Assert.Equal(Constants.HolderNameConstants.UnknownHolderNameId, notFoundResult.Value);
        }

        [Fact]
        public async Task When_XRequestIdHeaderIsMissing_Then_Return400BadRequestResponse()
        {
            // Arrange
            _idValidator.Setup(x => x.IsValidGuid(It.IsAny<string>())).Returns(false);
            var httpContext = new DefaultHttpContext(); // No X-Request-ID header set
            _controller.ControllerContext = new ControllerContext() { HttpContext = httpContext };

            // Act
            var result = await _controller.GetAsync(new RequestHeaderModel(), null);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult);
            Assert.Equal((int)HttpStatusCode.BadRequest, badRequestResult.StatusCode);
            Assert.Equal(Constants.HolderNameConstants.InvalidRequestId, badRequestResult.Value?.ToString());
        }

        [Fact]
        public async Task When_XRequestIdHeaderIsEmpty_Then_Return400BadRequestResponse()
        {
            // Arrange
            _idValidator.Setup(x => x.IsValidGuid(It.IsAny<string>())).Returns(false);
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[HeaderConstants.RequestId] = string.Empty; // Empty header value
            _controller.ControllerContext = new ControllerContext() { HttpContext = httpContext };

            // Act
            var result = await _controller.GetAsync(new RequestHeaderModel { XRequestId = string.Empty }, null);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult);
            Assert.Equal((int)HttpStatusCode.BadRequest, badRequestResult.StatusCode);
            Assert.Equal(Constants.HolderNameConstants.InvalidRequestId, badRequestResult.Value?.ToString());
        }

        [Fact]
        public async Task When_XRequestIdHeaderIsInvalid_Then_Return400BadRequestResponse()
        {
            // Arrange
            _idValidator.Setup(x => x.IsValidGuid(It.IsAny<string>())).Returns(false);
            const string requestId = "invalid-guid";
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[HeaderConstants.RequestId] = requestId; // Invalid GUID
            _controller.ControllerContext = new ControllerContext() { HttpContext = httpContext };

            // Act
            var result = await _controller.GetAsync(new RequestHeaderModel { XRequestId = requestId }, null);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult);
            Assert.Equal((int)HttpStatusCode.BadRequest, badRequestResult.StatusCode);
            Assert.Equal(Constants.HolderNameConstants.InvalidRequestId, badRequestResult.Value?.ToString());
        }
    }
}