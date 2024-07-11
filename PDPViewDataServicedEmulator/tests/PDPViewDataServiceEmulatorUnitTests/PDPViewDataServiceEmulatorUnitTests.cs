using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Moq;
using PDPViewDataServicedEmulator.Controllers;
using PDPViewDataServicedEmulator.CosmosRepository;
using PDPViewDataServicedEmulator.Mocks;
using PDPViewDataServicedEmulator.Models;

namespace PDPViewDataServiceEmulatorUnitTests
{
    public class PDPViewDataServiceEmulatorUnitTests
    {
        public const string Scope = "owner";
        public const string InValidScope = "owner123abc";
        public static string EmptyScope = string.Empty;
        private readonly DefaultHttpContext _httpContext;
        private readonly PDPViewDataController _controller;
        public static string EmptyAsset_Guid = string.Empty;
        public static string EmptyResponseHeaderValue = string.Empty;
        private readonly Mock<IViewDataRepository> mockviewDataRepository;
        public static string ValidAsset_Guid = "1ba03e25-659a-43b8-ae77-b956df168969";
        public static string InValidAsset_Guid = "a39507c2-ce90-4970-9a15-f771f9ac648f";

       public PDPViewDataServiceEmulatorUnitTests()
        {
            mockviewDataRepository = new Mock<IViewDataRepository>();
            _httpContext = new DefaultHttpContext();
            _controller = new PDPViewDataController(mockviewDataRepository.Object)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = _httpContext
                }
            };

            mockviewDataRepository.Setup(x => x.GetViewData(ValidAsset_Guid)).Returns(
                new ViewDataPayload
                {
                    AssetGuid = "1ba03e25-659a-43b8-ae77-b956df168969",
                    ViewData = "{\r\n\t\"arrangements\": [\r\n\t\t{\r\n\t\t\t\"pensionProviderSchemeName\": \"My Company Direct Contribution Scheme\",\r\n\t\t\t\"alternateSchemeName\": {\r\n\t\t\t\t\"name\": \"Converted from My Old Direct Contribution Scheme\",\r\n\t\t\t\t\"alternateNameType\": \"FOR\"\r\n\t\t\t},\r\n\t\t\t\"possibleMatch\": true,\r\n\t\t\t\"possibleMatchReference\": \"Q12345\",\r\n\t\t\t\"pensionAdministrator\": {\r\n\t\t\t\t\"name\": \"Pension Company 1\",\r\n\t\t\t\t\"contactMethods\": [\r\n\t\t\t\t\t{\r\n\t\t\t\t\t\t\"preferred\": false,\r\n\t\t\t\t\t\t\"contactMethodDetails\": {\r\n\t\t\t\t\t\t\t\"email\": \"example@examplemyline.com\"\r\n\t\t\t\t\t\t}\r\n\t\t\t\t\t},\r\n\t\t\t\t\t{\r\n\t\t\t\t\t\t\"preferred\": true,\r\n\t\t\t\t\t\t\"contactMethodDetails\": {\r\n\t\t\t\t\t\t\t\"number\": \"+123 1111111111\",\r\n\t\t\t\t\t\t\t\"usage\": [\r\n\t\t\t\t\t\t\t\t\"A\",\r\n\t\t\t\t\t\t\t\t\"M\"\r\n\t\t\t\t\t\t\t]\r\n\t\t\t\t\t\t}\r\n\t\t\t\t\t}\r\n\t\t\t\t]\r\n\t\t\t}\r\n\t\t}\r\n\t]\r\n}"
                });
        }

        [Fact]
        public async void WhenControllerIsCalled_WithValidAuthorizationHeader_ValidAsset_Guid_ValidScope_ThenItShouldReturn_200OK()
        {
            // Arrange
            AddAuthorisationHeader();
            _httpContext.Request.Headers["X-Request-ID"] = "35cfcfb0-d98d-451f-83f1-e59933078555";

            // Act
            var result = await _controller.GetAsync(ValidAsset_Guid!, Scope);
            OkObjectResult okResult = (OkObjectResult)result;
            var data = (ViewDataResponseModel)okResult!.Value!;

            //Assert
            Assert.True(result.GetType() == typeof(OkObjectResult));
            Assert.True(data.GetType() == typeof(ViewDataResponseModel));
            Assert.NotNull(result);
            Assert.True(okResult.StatusCode == (int)HttpStatusCode.OK);
            var okResultValues = result as OkObjectResult;
            Assert.NotNull(okResultValues);
            var response = okResultValues.Value;
            Assert.NotNull(response);
        }

        [Fact]
        public async void WhenControllerIsCalled_WithNoAuthorizationHeader_ThenItShouldReturn_Unauthorised401ResponseAnd_WwwAuthenticateResponseHeader()
        {
            // Act            
            var result = await _controller.GetAsync(InValidAsset_Guid!, Scope);
            UnauthorizedObjectResult unAuthorizedResult = (UnauthorizedObjectResult)result;
            _httpContext.Response.Headers.TryGetValue("WWW-Authenticate", out var wwwAuthenticate);

            // Assert
            Assert.True(result.GetType() == typeof(UnauthorizedObjectResult));
            Assert.True(unAuthorizedResult.StatusCode == (int)HttpStatusCode.Unauthorized);
            Assert.True(wwwAuthenticate == ResponseHeaderValue());
        }

        [Fact]
        public async void WhenControllerIsCalled_WithBearerAuthorizationHeader_AndNoToken_ThenItShouldReturn_Unauthorised401ResponseAnd_WwwAuthenticateResponseHeader()
        {
            // Arrange
            AddAuthorisationHeaderNoToken();

            // Act           
            var result = await _controller.GetAsync(EmptyAsset_Guid!, Scope);
            UnauthorizedObjectResult unAuthorizedResult = (UnauthorizedObjectResult)result;
            _httpContext.Response.Headers.TryGetValue("WWW-Authenticate", out var wwwAuthenticate);

            // Assert
            Assert.True(result.GetType() == typeof(UnauthorizedObjectResult));
            Assert.True(unAuthorizedResult.StatusCode == (int)HttpStatusCode.Unauthorized);
            Assert.True(wwwAuthenticate == ResponseHeaderValue());
        }

        [Fact]
        public async void WhenControllerIsCalled_WithInCorrectAuthorizationHeader_ThenItShouldReturn_Unauthorised401Response_EmptyResponseHeader()
        {
            // Act
            AddInCorrectAuthorisationHeader();
            var result = await _controller.GetAsync(ValidAsset_Guid!, Scope);
            UnauthorizedObjectResult unAuthorizedResult = (UnauthorizedObjectResult)result;
            _httpContext.Response.Headers.TryGetValue("WWW-Authenticate", out var wwwAuthenticate);

            // Assert
            Assert.True(result.GetType() == typeof(UnauthorizedObjectResult));
            Assert.True(unAuthorizedResult.StatusCode == (int)HttpStatusCode.Unauthorized);
            Assert.False(wwwAuthenticate == ResponseHeaderValue());
        }

        [Fact]
        public async void WhenControllerIsCalled_WithAuthorizationHeader_InValidAsset_Guid_ThenItShouldReturn_BadRequest400Response()
        {
            // Arrange
            AddAuthorisationHeader();

            // Act           
            var result = await _controller.GetAsync(InValidAsset_Guid!, Scope);
            BadRequestObjectResult badRequestResult = (BadRequestObjectResult)result;
            _httpContext.Response.Headers.TryGetValue("WWW-Authenticate", out var wwwAuthenticate);

            // Assert
            Assert.True(result.GetType() == typeof(BadRequestObjectResult));
            Assert.True(badRequestResult.StatusCode == (int)HttpStatusCode.BadRequest);
            Assert.False(wwwAuthenticate == ResponseHeaderValue());
        }

        [Fact]
        public async void WhenControllerIsCalled_WithAuthorizationHeader_EmptyAsset_Guid_ThenItShouldReturn_BadRequest400Response()
        {

            // Arrange
            AddAuthorisationHeader();

            // Act           
            var result = await _controller.GetAsync(EmptyAsset_Guid!, Scope);
            BadRequestObjectResult badResult = (BadRequestObjectResult)result;
            _httpContext.Response.Headers.TryGetValue("WWW-Authenticate", out var wwwAuthenticate);

            //Assert
            Assert.True(badResult.StatusCode == (int)HttpStatusCode.BadRequest);
            Assert.False(wwwAuthenticate == ResponseHeaderValue());
        }

        [Fact]
        public async void WhenControllerIsCalled_WithUnknownAsset_Guid_ThenItShouldReturn_NotFound()
        {
            // Arrange
            AddAuthorisationHeader();
            _httpContext.Request.Headers["X-Request-ID"] = "35cfcfb0-d98d-451f-83f1-e59933078555";

            // Act           
            var result = await _controller.GetAsync(InValidAsset_Guid!, Scope);
            NotFoundObjectResult notFoundResult = (NotFoundObjectResult)result;

            // Assert
            Assert.True(result.GetType() == typeof(NotFoundObjectResult));
            Assert.True(notFoundResult.StatusCode == (int)HttpStatusCode.NotFound);
        }

        [Fact]
        public async void WhenControllerIsCalled_InValidScope_WithAuthorizationHeader_ValidAsset_Guid_ThenItShouldReturn_BadRequest400Response()
        {

            // Arrange
            AddAuthorisationHeader();

            // Act           
            var result = await _controller.GetAsync(ValidAsset_Guid!, InValidScope);
            BadRequestObjectResult badResult = (BadRequestObjectResult)result;
            _httpContext.Response.Headers.TryGetValue("WWW-Authenticate", out var wwwAuthenticate);

            //Assert
            Assert.True(badResult.StatusCode == (int)HttpStatusCode.BadRequest);
            Assert.False(wwwAuthenticate == ResponseHeaderValue());
        }

        [Fact]
        public async void WhenControllerIsCalled_EmptyScope_WithAuthorizationHeader_ValidAsset_Guid_ThenItShouldReturn_BadRequest400Response()
        {

            // Arrange
            AddAuthorisationHeader();

            // Act           
            var result = await _controller.GetAsync(ValidAsset_Guid!, EmptyScope);
            BadRequestObjectResult badResult = (BadRequestObjectResult)result;
            _httpContext.Response.Headers.TryGetValue("WWW-Authenticate", out var wwwAuthenticate);

            //Assert
            Assert.True(badResult.StatusCode == (int)HttpStatusCode.BadRequest);
            Assert.False(wwwAuthenticate == ResponseHeaderValue());
        }

        [Fact]
        public async void WhenControllerIsCalled_WithNullViewData_ThenItShouldReturn_NotFound()
        {
            // Arrange
            AddAuthorisationHeader();
            _httpContext.Request.Headers["X-Request-ID"] = "35cfcfb0-d98d-451f-83f1-e59933078555";
            mockviewDataRepository.Setup(x => x.GetViewData(ValidAsset_Guid));

            // Act           
            var result = await _controller.GetAsync(ValidAsset_Guid!, Scope);
            NotFoundObjectResult notFoundResult = (NotFoundObjectResult)result;

            // Assert
            Assert.True(result.GetType() == typeof(NotFoundObjectResult));
            Assert.True(notFoundResult.StatusCode == (int)HttpStatusCode.NotFound);
        }

        private void AddAuthorisationHeader()
        {
            _httpContext.Request.Headers[HeaderNames.Authorization] = "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";
        }
        private void AddAuthorisationHeaderNoToken()
        {
            _httpContext.Request.Headers[HeaderNames.Authorization] = "Bearer ";
        }

        private void AddInCorrectAuthorisationHeader()
        {
            _httpContext.Request.Headers[HeaderNames.Authorization] = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";
        }

        private string ResponseHeaderValue()
        {
            return "realm=\"PensionDashboard\", " +
                "as_uri=\"https://pdp/ig/token\", " +
                "ticket=\"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.cThIIoDvwdueQB468K5xDc5633seEFoqwxjF_xSJyQQ\"";
        }

    }
}