using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Net.Http.Headers;
using Moq;
using Newtonsoft.Json.Linq;
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
        private readonly Mock<ICosmosDbRepository<ViewDataPayload>> mockcosmosDbRepository;
        public static string ValidAsset_Guid = "1ba03e25-659a-43b8-ae77-b956df168969";
        public static string InValidAsset_Guid = "a39507c2-ce90-4970-9a15-f771f9ac648f";

        public PDPViewDataServiceEmulatorUnitTests()
        {
            mockcosmosDbRepository = new Mock<ICosmosDbRepository<ViewDataPayload>>();
            _httpContext = new DefaultHttpContext();
            _controller = new PDPViewDataController(mockcosmosDbRepository.Object)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = _httpContext
                }
            };
            
            string relativePath = Path.Combine("Support", "ViewDataPayload", "view_data_sample_1.json");

            var jsonData = Helper.ReadViewDataPayloadFile(relativePath);
            jsonData.TryGetValue("assetGuid", out var assetGuid);
            jsonData.TryGetValue("view_data", out var viewData);
            
            mockcosmosDbRepository.Setup(x => x.GetByIdAsync(ValidAsset_Guid, ValidAsset_Guid)).ReturnsAsync(
                new ViewDataPayload 
                {
                    AssetGuid = assetGuid?.ToString(),
                    ViewData = viewData?.ToObject<JObject>()
                });                
        }

        [Fact]
        public async void WhenControllerIsCalled_WithValidAuthorizationHeader_ValidAsset_Guid_ValidScope_ThenItShouldReturn_200OK()
        {
            // Arrange
             AddAuthorisationHeader();
            _httpContext.Request.Headers["X-Request-ID"] = "35cfcfb0-d98d-451f-83f1-e59933078555";

            // Act
            var result = await _controller.GetAsync(ValidAsset_Guid!,Scope);
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
            mockcosmosDbRepository.Setup(x => x.GetByIdAsync(InValidAsset_Guid, Scope));
            
            // Act           
            var result = await _controller.GetAsync(InValidAsset_Guid!,Scope);
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

    public class CosmosRepositoryUnitTests
    {
        private readonly Mock<CosmosClient> _cosmosClient;
        private readonly Mock<Database> _database;
        private readonly Mock<Container> _container;
        private readonly Mock<ItemResponse<ViewDataPayload>> _databasemockItemResponse;

        public CosmosRepositoryUnitTests()
        {
            _cosmosClient = new Mock<CosmosClient>();
            _database = new Mock<Database>();
            _container = new Mock<Container>();

            var response = new ViewDataPayload { AssetGuid = "1ba03e25-659a-43b8-ae77-b956df168969", ViewData = new JObject() };

            _databasemockItemResponse = new Mock<ItemResponse<ViewDataPayload>>();
            _databasemockItemResponse.Setup(x => x.Resource).Returns(response);
            _container.Setup(x => x.ReadItemAsync<ViewDataPayload>(It.IsAny<string>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>())).ReturnsAsync(_databasemockItemResponse.Object);

            _database.Setup(x => x.GetContainer(It.IsAny<string>())).Returns(_container.Object);
            _cosmosClient.Setup(x => x.GetDatabase(It.IsAny<string>())).Returns(_database.Object!);

            _databasemockItemResponse.Setup(x => x.Equals(It.IsAny<ViewDataPayload>())).Returns(true);
            _databasemockItemResponse.Setup(x => x.Resource).Returns(response);
            _container.Setup(x => x.ReadItemAsync<ViewDataPayload>(It.IsAny<string>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>())).ReturnsAsync(_databasemockItemResponse.Object);
        }


        [Fact]
        public async void WhenGetByIdAsyncIsCalled_WithValidAsset_Guid_ThenItShouldReturn_ViewDataPayload()
        {
            // Arrange
            var cosmosDbRepository = new CosmosDbRepository<ViewDataPayload>(_cosmosClient.Object, "databaseName", "containerName");

            // Act
            var result = await cosmosDbRepository.GetByIdAsync("1ba03e25-659a-43b8-ae77-b956df168969", "1ba03e25-659a-43b8-ae77-b956df168969");

            // Assert
            Assert.NotNull(result);
            Assert.True(result!.AssetGuid == "1ba03e25-659a-43b8-ae77-b956df168969");
        }

        [Fact]
        public async void WhenGetByIdAsyncIsCalled_WithInValidAsset_Guid_ThenItShouldReturn_Null()
        {
            // Arrange
            _databasemockItemResponse.Setup(x => x.Resource).Returns<ViewDataPayload>(null!);
            var cosmosDbRepository = new CosmosDbRepository<ViewDataPayload>(_cosmosClient.Object, "databaseName", "containerName");

            // Act
            var result = await cosmosDbRepository.GetByIdAsync("a39507c2-ce90-4970-9a15-f771f9ac648f", "a39507c2-ce90-4970-9a15-f771f9ac648f");

            // Assert
            Assert.Null(result);
        }
    }
}