using MhpdCommon.Constants;
using MhpdCommon.Models.Configuration;
using MhpdCommon.Repository;
using MhpdCommon.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Moq;
using PDPViewDataServiceEmulator.Configuration;
using PDPViewDataServiceEmulator.Controllers;
using PDPViewDataServiceEmulator.Mocks;
using PDPViewDataServiceEmulator.Models;
using PDPViewDataServiceEmulatorUnitTests.Mock.ViewDataPayload;
using System.Net;

namespace PDPViewDataServiceEmulatorUnitTests;

public class PdpViewDataServiceEmulatorUnitTests
{
    private const string Scope = "owner";
    private const string InValidScope = "owner123abc";
    private const string ValidAssetGuid = "1ba03e25-659a-43b8-ae77-b956df168969";
    private const string InValidAssetGuid = "a39507c2-ce90-4970-9a15-f771f9ac648f";
    private const string XRequestId = "35cfcfb0-d98d-451f-83f1-e59933078555";
    private static readonly string EmptyAssetGuid = string.Empty;
    private static readonly string EmptyScope = string.Empty;
    private readonly CommonHttpConfiguration _httpConfiguration;

    private readonly DefaultHttpContext _httpContext;
    private readonly PdpViewDataController _controller;
    private readonly Mock<ITokenUtility> _tokenUtilityMock = new();
    private readonly Mock<IdValidator> _idValidator = new();
    private readonly Mock<ICosmosDbRepository<ViewDataPayload>> _mockViewDataPayloadRepository = new();
    
    public PdpViewDataServiceEmulatorUnitTests()
    {
        var configuration = new MhpdCosmosConfiguration
        {
            DatabaseName = "TestDatabase",
            ViewdatapayloadsContainerName = "viewdatapayloadsContainerName",
        };

        _httpConfiguration = new CommonHttpConfiguration
        {
            CdaServiceUrl = "https://id.pdp.com"
        };
         
        Mock<ILogger<PdpViewDataController>> mockLogger = new();
                 
        Mock<IOptions<MhpdCosmosConfiguration>> mockCosmosConfigOptions = new();
        mockCosmosConfigOptions.Setup(x => x.Value).Returns(configuration);

        Mock<IOptions<CommonHttpConfiguration>> mockHttpConfigOptions = new();
        mockHttpConfigOptions.Setup(x => x.Value).Returns(_httpConfiguration);

        var jsonData = DataProvider.GetPayload<ViewDataPayload>("view_data_sample_1.json");
        
        _mockViewDataPayloadRepository
            .Setup(c => c.GetByIdAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(jsonData);
        
        _httpContext = new DefaultHttpContext();
        _controller = new PdpViewDataController(mockLogger.Object, _mockViewDataPayloadRepository.Object, _idValidator.Object, 
            _tokenUtilityMock.Object, mockHttpConfigOptions.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = _httpContext
            }
        };
    }

    [Fact]
    public async void WhenControllerIsCalled_WithValidAuthorizationHeader_ValidAsset_Guid_ValidScope_ThenItShouldReturn_200OK()
    {
        // Arrange
        AddAuthorisationHeader();
        _httpContext.Request.Headers["X-Request-ID"] = XRequestId;

        // Act
        var result = await _controller.GetAsync(ValidAssetGuid, Scope, XRequestId);
        OkObjectResult okResult = (OkObjectResult)result;
        var data = (ViewDataResponseModel)okResult.Value!;

        //Assert
        Assert.True(result.GetType() == typeof(OkObjectResult));
        Assert.True(data.GetType() == typeof(ViewDataResponseModel));
        Assert.NotNull(result);
        Assert.True(okResult.StatusCode == (int)HttpStatusCode.OK);
        var okResultValues = result as OkObjectResult;
        Assert.NotNull(okResultValues);
        var value = okResultValues.Value;
        Assert.NotNull(value);
    }

    [Fact]
    public async void WhenControllerIsCalled_WithNoAuthorizationHeader_ThenItShouldReturn_Unauthorised401ResponseAnd_WwwAuthenticateResponseHeader()
    {
        // Act            
        var result = await _controller.GetAsync(InValidAssetGuid, Scope, XRequestId);
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
        var result = await _controller.GetAsync(EmptyAssetGuid, Scope, XRequestId);
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
        var result = await _controller.GetAsync(ValidAssetGuid, Scope, XRequestId);
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
        var result = await _controller.GetAsync("invalid", Scope, XRequestId);
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
        var result = await _controller.GetAsync(EmptyAssetGuid, Scope, XRequestId);
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
        
        ViewDataPayload? testModel = null;
        
        _mockViewDataPayloadRepository
            .Setup(c => c.GetByIdAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(testModel); // Mock the ReadItemAsync method

        // Act           
        var result = await _controller.GetAsync(InValidAssetGuid, Scope, XRequestId);
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
        var result = await _controller.GetAsync(ValidAssetGuid, InValidScope, XRequestId);
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
        var result = await _controller.GetAsync(ValidAssetGuid, EmptyScope, XRequestId);
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

        ViewDataPayload? testModel = null;
        
        _mockViewDataPayloadRepository
            .Setup(c => c.GetByIdAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(testModel);
        
        // Act           
        var result = await _controller.GetAsync(InValidAssetGuid,Scope, XRequestId);
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
               $"as_uri=\"{_httpConfiguration.CdaTokenEndpoint}\", " +
               $"ticket=\"{SecurityConstants.Jwe.AuthorizationRequiredPermissionTicket}";
    }

}