using System.Net;
using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.Models.RequestHeaderModel;
using MhpdCommon.TokenValidation;
using MhpdCommon.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TokenIntegrationService.Controllers;
using TokenIntegrationService.HttpClients;
using TokenIntegrationService.Models;
using Moq;

namespace TokenIntegrationServiceUnitTests;

public class TokenIntegrationServiceUnitTests
{
    private readonly TokenController _controller;
    private readonly Mock<ICdaServiceClient> _iCdaToken = new(); 
    private readonly RequestHeaderModel _requestHeaderModel = new() { XRequestId = ValidXRequestId };

    private const string RqpValue = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";
    private const string TicketValue = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";
    private const string AsUriValue = "http://localhost:5044";
    private const string InvalidRqpValue = "ABC123InvalidRqpValue";
    private const string InvalidTicketValue = "ZYZ123InvalidTicketValue";
    private const string ValidXRequestId = "b7301d11-f166-499a-9bf1-0598c2f1af52";
    private const string SpecialCharAsUriValue = "http://localhost:1234@#$%^&&&&.net";
    private const string SpecialCharRqpValue = "SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJVadQssw5c@@##$%_+!!**({{^?,,,@";
    private const string SpecialCharTicketValue = "eyJzdWIiOiIxMjM0NTY  3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.ASDFG@@##$%_+!!**({{^?,,,@";
        
    public TokenIntegrationServiceUnitTests()
    {
        // Arrange: Create the mocks
        Mock<ILogger<TokenController>> mockLogger = new();
        Mock<IIdValidator> mockIdValidator = new();
        mockIdValidator.Setup(v => v.IsValidGuid(It.IsAny<string>())).Returns(true);
            
        // Get ordered validators
        var validators = Helper.GetOrderedValidatorsForTokenIntegrationRequest();

        // Create the TokenRequestValidatorPipeline with the mock validators
        Mock<TokenIntegrationRequestValidatorPipeline> mockValidatorPipeline = new(validators);
            
        var httpContext = new DefaultHttpContext();
        _controller = new TokenController(_iCdaToken.Object, mockLogger.Object, mockIdValidator.Object, mockValidatorPipeline.Object)
        {
            ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            }
        };

        _iCdaToken.Setup(x => x.PostRptAsync(It.IsAny<CdaTokenRequestModel>(), _requestHeaderModel)).Returns(Task.FromResult(new RptsModel { AccessToken = RqpValue }));
    }
    
    [Fact]
    public async void WhenControllerIsCalled_WithInValidXRequestedId_ThenItShouldReturn_BadRequest400Response()
    {
        // Arrange           
        var request = new TokenIntegrationRequestModel
        {
            Rqp = RqpValue,
            Ticket = TicketValue,
            AsUri = AsUriValue
        };

        // Act
        var result = await _controller.PostAsync(request, new RequestHeaderModel { XRequestId = string.Empty}); 
        var badResult = (BadRequestObjectResult)result;

        // Assert
        Assert.True(result.GetType() == typeof(BadRequestObjectResult));
        Assert.True(badResult.StatusCode == (int)HttpStatusCode.BadRequest);
    }
        
    [Fact]
    public async void WhenControllerIsCalled_WithValidRequestBody_ThenItShouldReturn_OKRequest200Response()
    {
        // Arrange           
        var request = new TokenIntegrationRequestModel
        {
            Rqp = RqpValue,
            Ticket = TicketValue,
            AsUri = AsUriValue
        };

        // Act
        var result = await _controller.PostAsync(request, _requestHeaderModel); 
        var okResult = (OkObjectResult)result;
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
            AsUri = AsUriValue
        };

        // Act
        var result = await _controller.PostAsync(request, _requestHeaderModel);
        var badResult = (BadRequestObjectResult)result;

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
            AsUri = AsUriValue
        };

        // Act
        var result = await _controller.PostAsync(request, _requestHeaderModel);
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
            AsUri = string.Empty
        };

        // Act
        var result = await _controller.PostAsync(request, _requestHeaderModel);
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
            AsUri = AsUriValue
        };

        // Act
        var result = await _controller.PostAsync(request, _requestHeaderModel);
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
            AsUri = AsUriValue
        };

        // Act
        var result = await _controller.PostAsync(request, _requestHeaderModel);
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
        var result = await _controller.PostAsync(request, _requestHeaderModel);
        var badResult = (BadRequestObjectResult)result;

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
        var result = await _controller.PostAsync(request, _requestHeaderModel);
        var badResult = (BadRequestObjectResult)result;

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
            AsUri = string.Empty
        };

        // Act
        var result = await _controller.PostAsync(request, _requestHeaderModel);
        var badResult = (BadRequestObjectResult)result;

        // Assert
        Assert.True(result.GetType() == typeof(BadRequestObjectResult));
        Assert.True(badResult.StatusCode == (int)HttpStatusCode.BadRequest);
    }

    [Fact]
    public async void WhenControllerIsCalled_InvalidRqp_ThenItShouldReturn_BadRequest400Response()
    {
        // Arrange
        var request = new TokenIntegrationRequestModel
        {
            Rqp = InvalidRqpValue,
            Ticket = TicketValue,
            AsUri = AsUriValue
        };

        // Act
        var result = await _controller.PostAsync(request, _requestHeaderModel);
        var badResult = (BadRequestObjectResult)result;

        // Assert
        Assert.NotNull(result);
        Assert.True(result.GetType() == typeof(BadRequestObjectResult));
        Assert.Equal(TokenValidationMessages.InvalidRqpFormat, (string)badResult.Value!);
        Assert.True(badResult.StatusCode == (int)HttpStatusCode.BadRequest);
    }

    [Fact]
    public async void WhenControllerIsCalled_InvalidTicket_ThenItShouldReturn_BadRequest400Response()
    {
        // Arrange
        var request = new TokenIntegrationRequestModel
        {
            Rqp = RqpValue,
            Ticket = InvalidTicketValue,
            AsUri = AsUriValue
        };

        // Act
        var result = await _controller.PostAsync(request, _requestHeaderModel);
        var badResult = (BadRequestObjectResult)result;

        // Assert
        Assert.NotNull(result);
        Assert.True(result.GetType() == typeof(BadRequestObjectResult));
        Assert.Equal(TokenValidationMessages.InvalidTicketQueryFormat, (string)badResult.Value!);
        Assert.True(badResult.StatusCode == (int)HttpStatusCode.BadRequest);
    }

    [Fact]
    public async void WhenControllerIsCalled_SpecialCharRqp_ThenItShouldReturn_BadRequest400Response()
    {
        // Arrange
        var request = new TokenIntegrationRequestModel
        {
            Rqp = SpecialCharRqpValue,
            Ticket = TicketValue,
            AsUri = AsUriValue
        };

        // Act
        var result = await _controller.PostAsync(request, _requestHeaderModel);
        var badResult = (BadRequestObjectResult)result;

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
            AsUri = SpecialCharAsUriValue
        };

        // Act
        var result = await _controller.PostAsync(request, _requestHeaderModel);
        var badResult = (BadRequestObjectResult)result;

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
            AsUri = AsUriValue
        };

        // Act
        var result = await _controller.PostAsync(request, _requestHeaderModel);
        var badResult = (BadRequestObjectResult)result;

        // Assert
        Assert.True(result.GetType() == typeof(BadRequestObjectResult));
        Assert.True(badResult.StatusCode == (int)HttpStatusCode.BadRequest);
    }

}