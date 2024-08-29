using System.Net;
using System.Text.Json;
using CDAServiceEmulator.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CDAServiceEmulatorUnitTests
{
    public class HolderNameConfigurationUnitTests
    {
        private readonly DefaultHttpContext _httpContext;
        private readonly HolderNameController _controller;

        public HolderNameConfigurationUnitTests()
        {
            _httpContext = new DefaultHttpContext();
            _httpContext.Request.Headers["X-Request-ID"] = "35cfcfb0-d98d-451f-83f1-e59933078555";

            _controller = new HolderNameController()
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = _httpContext
                }
            };
        }

        [Fact]
        public async void WhenControllerIsCalled_WithCorrectHeader_WithoutPeiQueryString_ThenItShouldReturn_Ok200Response()
        {
            // Arrange
            string? pei = null;

            // Act
            var result = await _controller.GetAsync(pei);
            OkObjectResult okResult = (OkObjectResult)result;
            var data = (string)okResult!.Value!;

            // Assert
            Assert.NotNull(result);
            Assert.True(result.GetType() == typeof(OkObjectResult));
            Assert.True(okResult.StatusCode == (int)HttpStatusCode.OK);

            // Assert that the type is string and that the json is valid
            Assert.True(data.GetType() == typeof(string));
            Assert.True(data.IsJson());
        }

        [Fact]
        public async void WhenControllerIsCalled_WithCorrectHeaders_WithCorrectPeiQueryString_ThenItShouldReturn_Ok200Response()
        {
            // Arrange
            string pei = "728f9722-88c1-42f3-965a-d2faab8967e8:26d93ebc-0dfd-43c0-bfee-2b8f8ad7a742";

            // Act
            var result = await _controller.GetAsync(pei);
            OkObjectResult okResult = (OkObjectResult)result;

            // Assert
            Assert.NotNull(result);
            Assert.True(result.GetType() == typeof(OkObjectResult));
            Assert.True(okResult.StatusCode == (int)HttpStatusCode.OK);
        }

        [Fact]
        public async void WhenControllerIsCalled_WithOutCorrectHeader_WithNoPeiQueryString_ThenItShouldReturn_BadRequest400Response()
        {
            // Arrange
            string? pei = null;
            _httpContext.Request.Headers["X-Request-ID"] = string.Empty;

            // Act
            var result = await _controller.GetAsync(pei);
            BadRequestObjectResult okResult = (BadRequestObjectResult)result;

            // Assert
            Assert.True(result.GetType() == typeof(BadRequestObjectResult));
            Assert.True(okResult.StatusCode == (int)HttpStatusCode.BadRequest);
            Assert.NotNull(result);
        }

        [Fact]
        public async void WhenControllerIsCalled_WithOutCorrectHeader_WithCorrectPeiQueryString_ThenItShouldReturn_BadRequest400Response()
        {
            // Arrange
            string pei = "728f9722-88c1-42f3-965a-d2faab8967e8:26d93ebc-0dfd-43c0-bfee-2b8f8ad7a742";
            _httpContext.Request.Headers["X-Request-ID"] = string.Empty;

            // Act
            var result = await _controller.GetAsync(pei);
            BadRequestObjectResult okResult = (BadRequestObjectResult)result;

            // Assert
            Assert.NotNull(result);
            Assert.True(result.GetType() == typeof(BadRequestObjectResult));
            Assert.True(okResult.StatusCode == (int)HttpStatusCode.BadRequest);
        }

        [Fact]
        public async void WhenControllerIsCalled_WithCorrectHeader_WithInCorrectPeiHavingLengthtGreaterThan73_ThenItShouldReturn_BadRequest400Response()
        {
            // Arrange
            string pei = "728f9722-88c1-42f3-965a-d2faab8967e8:26d93ebc-0dfd-43c0-bfee-2b8f8ad7a742XXXXXXXXXXXXXXXXXXXXXXXXXXX";
            
            // Act
            var result = await _controller.GetAsync(pei);
            BadRequestObjectResult okResult = (BadRequestObjectResult)result;

            // Assert
            Assert.NotNull(result);
            Assert.True(result.GetType() == typeof(BadRequestObjectResult));
            Assert.True(okResult.StatusCode == (int)HttpStatusCode.BadRequest);
        }

    }

    public static class StringExtensions
    {
        public static bool IsJson(this string source)
        {
            if (source == null)
                return false;

            try
            {
                using (JsonDocument doc = JsonDocument.Parse(source))
                {
                    var rootElement = doc.RootElement;
                    var holderConfigurationsJsonElement = rootElement.GetProperty("holder_configurations");

                    if (holderConfigurationsJsonElement.ValueKind != JsonValueKind.Array)
                        return false;

                    foreach (var address in holderConfigurationsJsonElement.EnumerateArray())
                    {
                        var holderNameGuidJsonElement = address.GetProperty("holdername_guid");
                        var veiwDataUrlJsonElement = address.GetProperty("veiw_data_url");

                        if (holderConfigurationsJsonElement.EnumerateArray().Count() != 2)
                            return false;

                        if (!(holderNameGuidJsonElement.ValueEquals("7075aa11-10ad-4b2f-a9f5-1068e79119bf") 
                            || holderNameGuidJsonElement.ValueEquals("550e8400-e29b-41d4-a716-446655440000")))
                            return false;

                        if (!(veiwDataUrlJsonElement.ValueEquals("https://local.exampleprovider/pensiondataprovider")
                            || veiwDataUrlJsonElement.ValueEquals("https://local.exampleprovider2/pensiondataprovider")))
                            return false;
                    }

                    // dispose any created doc
                }
                
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }
    }
}
