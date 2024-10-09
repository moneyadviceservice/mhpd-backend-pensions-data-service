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

public class TokenControllerUnitTests
{
    private readonly TokenController _controller;
    private readonly Mock<ICdaServiceClient> _iCdaToken = new();
    private readonly Mock<ITokenUtility> _mockTokenUtility = new();

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
    private const string PeisId = "22c47c98-ade8-4c3d-8e78-39259cdcd57b";
    private const string IdToken =
    "eyJhbGciOiJodHRwOi8vd3d3LnczLm9yZy8yMDAxLzA0L3htbGRzaWctbW9yZSNyc2Etc2hhMjU2IiwidHlwIjoiSldUIn0.eyJwZWlzX2lkIjoiMDAwMjZjZTItOWM3ZS00YjJmLWE3YTYtMDk1ZDU1ZGUwODExIiwic3ViIjoiY2Y2NjhkNDctZWU1OC00ZTMzLWJjMDUtZmViNzA1OGRlNThkIiwiaXNzIjoiaHR0cHM6Ly9lbXVsYXRvcnMubWFwcy5vcmcudWsvYW0vb2F1dGgyIiwiYXVkIjpbImh0dHBzOi8vcGRwL2lnL3Rva2VuIiwiaHR0cHM6Ly9wZHAvaWcvdG9rZW4iXSwiaWF0IjoxNzI4NDU4ODQ3LCJleHAiOjE3Mjg0NTk0NDcsImp0aSI6IjAwOWYxMzFkLTk1NmUtNDgxMS1hNjM0LTgzZDdhYTRjZDZkMyJ9.ENiVIWmCmRZSt0KO71irwxtO4Z2yJGN3Z3luPEDVIyVI_8oFReVgLRZbpnigSxgJnu_8lh3tE1L3c5uv29ScB5a1e6AqoGcJ8R6zc8J1DrE6oBQA7ClU7guw1m9Rx1D2z4X0PiRk0AVU3rF5UkqDpNcJ2dlyLBkUYTYmuJZ25AWq2NYlQaHL_f2kErC1-Q6gsFWbGmKLIEJbjiGB4FN4kI_jIe9Ey04QOHvc1Z59vgGLdU9k9wNt9mooK2daacqhUnAjNb1JAGOuEs5rt5v_5S-7V6MVw0Oubx3UEE378Q_UKPUDQTdcsD20G7knOjUpu_ZTYlITFyOpOs2lUM4xrw";

    public TokenControllerUnitTests()
    {
        // Arrange: Create the mocks
        Mock<ILogger<TokenController>> mockLogger = new();
        Mock<IIdValidator> mockIdValidator = new();
        mockIdValidator.Setup(v => v.IsValidGuid(It.IsAny<string>())).Returns(true);
            
        // Get ordered validators
        var validators = Helper.GetOrderedValidatorsForTokenIntegrationRequest();

        // Create the TokenRequestValidatorPipeline with the mock validators
        Mock<TokenIntegrationRequestValidatorPipeline> mockValidatorPipeline = new(validators);
        
        // Get ordered validators
        var cdaValidators = Helper.GetOrderedValidators();

        // Create the TokenRequestValidatorPipeline with the mock validators
        Mock<TokenRequestValidatorPipeline> mockCdaValidatorPipeline = new(cdaValidators);
        
        _iCdaToken
            .Setup(x => x.PostAsync(It.IsAny<CdaTokenRequestModel>(), _requestHeaderModel))
            .Returns(Task.FromResult(new CdaTokenResponseModel { IdToken =  IdToken}));
        
        var httpContext = new DefaultHttpContext();
        _controller = new TokenController(_iCdaToken.Object, mockLogger.Object,
            mockIdValidator.Object, mockValidatorPipeline.Object,
            mockCdaValidatorPipeline.Object, _mockTokenUtility.Object)
        {
            ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            }
        };
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
    public async void WhenController_RPTS_Endpoint_IsCalled_WithValidRequestBody_ThenItShouldReturn_OKRequest200Response()
    {
        // Arrange           
        var request = new TokenIntegrationRequestModel
        {
            Rqp = RqpValue,
            Ticket = TicketValue,
            AsUri = AsUriValue
        };
        
        _iCdaToken
            .Setup(x => x.PostAsync(It.IsAny<CdaTokenRequestModel>(), _requestHeaderModel))
            .Returns(Task.FromResult(new CdaTokenResponseModel { AccessToken = RqpValue }));

        // Act
        var result = await _controller.PostAsync(request, _requestHeaderModel); 
        var okResult = (OkObjectResult)result;
        var data = (TokenIntegrationResponseModel)okResult.Value!;

        // Assert
        Assert.NotNull(result);
        Assert.True(result.GetType() == typeof(OkObjectResult));
        Assert.True(data.GetType() == typeof(TokenIntegrationResponseModel));
        Assert.True(okResult.StatusCode == (int)HttpStatusCode.OK);
        Assert.Equal(RqpValue, data.Rpt);
    }
    
    [Fact]
    public async void WhenController_Pei_Retrieval_Endpoint_IsCalled_WithValidRequestBody_ThenItShouldReturn_OKRequest200Response()
    {
        // Arrange           
        var request = new CdaTokenRequestModel
        {
            GrantType = TokenQueryParams.AuthorizationCodeGrantType, // Only used for triggering the validations
            ClientId = TokenQueryParams.ValidClientId,
            ClientSecret = TokenQueryParams.ValidClientSecret,
            Code = TokenQueryParams.ValidCode,
            CodeVerifier = TokenQueryParams.ValidCodeVerifier,
            RedirectUri = AsUriValue,
        };
        
        _iCdaToken
            .Setup(x => x.PostAsync(It.IsAny<CdaTokenRequestModel>(), It.IsAny<RequestHeaderModel>()))
            .ReturnsAsync(new CdaTokenResponseModel { IdToken = IdToken });
        
        // Simulate decoding of token
        _mockTokenUtility
            .Setup(x => x.DecodeJwt(It.IsAny<string>()))
            .Returns(new Dictionary<string, string> {{ "peis_id", PeisId }});

        // Act
        var result = await _controller.PostPeiRetrievalDetailsAsync(request, _requestHeaderModel); 
        var okResult = (OkObjectResult)result;
        var data = (PeiRetrievalDetailsResponseModel)okResult.Value!;

        // Assert
        Assert.NotNull(result);
        Assert.True(result.GetType() == typeof(OkObjectResult));
        Assert.True(data.GetType() == typeof(PeiRetrievalDetailsResponseModel));
        Assert.True(okResult.StatusCode == (int)HttpStatusCode.OK);
        Assert.Equal(PeisId, data.PeisId);
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
    
    [Fact]
    public async void WhenController_Pei_Retrieval_Endpoint_IsCalled_InvalidClientSecret_ThenItShouldReturn_BadRequest400Response()
    {
        // Arrange           
        var request = new CdaTokenRequestModel
        {
            GrantType = TokenQueryParams.AuthorizationCodeGrantType,
            ClientId = TokenQueryParams.ValidClientId,
            ClientSecret = string.Empty,
            Code = TokenQueryParams.ValidCode,
            CodeVerifier = TokenQueryParams.ValidCodeVerifier,
            RedirectUri = AsUriValue,
        };

        // Act
        var result = await _controller.PostPeiRetrievalDetailsAsync(request, _requestHeaderModel);
        var badResult = (BadRequestObjectResult)result;

        // Assert
        Assert.True(result.GetType() == typeof(BadRequestObjectResult));
        Assert.True(badResult.StatusCode == (int)HttpStatusCode.BadRequest);
        Assert.Equal(TokenValidationMessages.InvalidRequest, badResult.Value);
    }
    
    [Fact]
    public async void WhenController_Pei_Retrieval_Endpoint_IsCalled_InvalidGrantType_ThenItShouldReturn_BadRequest400Response()
    {
        // Arrange           
        var request = new CdaTokenRequestModel
        {
            GrantType = string.Empty,
            ClientId = TokenQueryParams.ValidClientId,
            ClientSecret = TokenQueryParams.ValidClientSecret,
            Code = TokenQueryParams.ValidCode,
            CodeVerifier = TokenQueryParams.ValidCodeVerifier,
            RedirectUri = AsUriValue,
        };

        // Act
        var result = await _controller.PostPeiRetrievalDetailsAsync(request, _requestHeaderModel);
        var badResult = (BadRequestObjectResult)result;

        // Assert
        Assert.True(result.GetType() == typeof(BadRequestObjectResult));
        Assert.True(badResult.StatusCode == (int)HttpStatusCode.BadRequest);
        Assert.Equal(TokenValidationMessages.MissingGrantType, badResult.Value);
    }
    
    [Fact]
    public async void WhenController_Pei_Retrieval_Endpoint_IsCalled_InvalidClientId_ThenItShouldReturn_BadRequest400Response()
    {
        // Arrange           
        var request = new CdaTokenRequestModel
        {
            GrantType = TokenQueryParams.AuthorizationCodeGrantType,
            ClientSecret = TokenQueryParams.ValidClientSecret,
            Code = TokenQueryParams.ValidCode,
            CodeVerifier = TokenQueryParams.ValidCodeVerifier,
            RedirectUri = AsUriValue,
        };

        // Act
        var result = await _controller.PostPeiRetrievalDetailsAsync(request, _requestHeaderModel);
        var badResult = (BadRequestObjectResult)result;

        // Assert
        Assert.True(result.GetType() == typeof(BadRequestObjectResult));
        Assert.True(badResult.StatusCode == (int)HttpStatusCode.BadRequest);
        Assert.Equal(TokenValidationMessages.InvalidRequest, badResult.Value);
    }
    
    [Fact]
    public async void WhenController_Pei_Retrieval_Endpoint_IsCalled_InvalidCode_ThenItShouldReturn_BadRequest400Response()
    {
        // Arrange           
        var request = new CdaTokenRequestModel
        {
            GrantType = TokenQueryParams.AuthorizationCodeGrantType,
            ClientId = TokenQueryParams.ValidClientId,
            ClientSecret = TokenQueryParams.ValidClientSecret,
            CodeVerifier = TokenQueryParams.ValidCodeVerifier,
            RedirectUri = AsUriValue,
        };

        // Act
        var result = await _controller.PostPeiRetrievalDetailsAsync(request, _requestHeaderModel);
        var badResult = (BadRequestObjectResult)result;

        // Assert
        Assert.True(result.GetType() == typeof(BadRequestObjectResult));
        Assert.True(badResult.StatusCode == (int)HttpStatusCode.BadRequest);
        Assert.Equal(TokenValidationMessages.InvalidRequest, badResult.Value);
    }
    
    [Fact]
    public async void WhenController_Pei_Retrieval_Endpoint_IsCalled_InvalidRedirectUri_ThenItShouldReturn_BadRequest400Response()
    {
        // Arrange           
        var request = new CdaTokenRequestModel
        {
            GrantType = TokenQueryParams.AuthorizationCodeGrantType,
            ClientId = TokenQueryParams.ValidClientId,
            Code = TokenQueryParams.ValidCode,
            ClientSecret = TokenQueryParams.ValidClientSecret,
            CodeVerifier = TokenQueryParams.ValidCodeVerifier
        };

        // Act
        var result = await _controller.PostPeiRetrievalDetailsAsync(request, _requestHeaderModel);
        var badResult = (BadRequestObjectResult)result;

        // Assert
        Assert.True(result.GetType() == typeof(BadRequestObjectResult));
        Assert.True(badResult.StatusCode == (int)HttpStatusCode.BadRequest);
        Assert.Equal(TokenValidationMessages.InvalidRequest, badResult.Value);
    }
    
    [Fact]
    public async void WhenController_Pei_Retrieval_Endpoint_IsCalled_InvalidCodeVerifier_ThenItShouldReturn_BadRequest400Response()
    {
        // Arrange
        var request = new CdaTokenRequestModel
        {
            GrantType = TokenQueryParams.AuthorizationCodeGrantType,
            ClientId = TokenQueryParams.ValidClientId,
            Code = TokenQueryParams.ValidCode,
            ClientSecret = TokenQueryParams.ValidClientSecret,
            RedirectUri = AsUriValue
        };

        // Act
        var result = await _controller.PostPeiRetrievalDetailsAsync(request, _requestHeaderModel);
        var badResult = (BadRequestObjectResult)result;

        // Assert
        Assert.True(result.GetType() == typeof(BadRequestObjectResult));
        Assert.True(badResult.StatusCode == (int)HttpStatusCode.BadRequest);
        Assert.Equal(TokenValidationMessages.InvalidRequest, badResult.Value);
    }
    
    [Fact]
    public async Task WhenPeisIdIsMissingInIdToken_ShouldReturnInternalServerError()
    {
        // Arrange
        var request = new CdaTokenRequestModel
        {
            GrantType = TokenQueryParams.AuthorizationCodeGrantType,
            ClientId = TokenQueryParams.ValidClientId,
            Code = TokenQueryParams.ValidCode,
            ClientSecret = TokenQueryParams.ValidClientSecret,
            RedirectUri = AsUriValue,
            CodeVerifier = TokenQueryParams.ValidCodeVerifier
        };

        // Simulate a response with a valid IdToken
        _iCdaToken
            .Setup(x => x.PostAsync(It.IsAny<CdaTokenRequestModel>(), It.IsAny<RequestHeaderModel>()))
            .ReturnsAsync(new CdaTokenResponseModel { IdToken = RqpValue });

        // Simulate decoding of token but missing "peis_id"
        _mockTokenUtility
            .Setup(x => x.DecodeJwt(It.IsAny<string>()))
            .Returns(new Dictionary<string, string>());

        // Act
        var result = await _controller.PostPeiRetrievalDetailsAsync(request, _requestHeaderModel);
        var internalErrorResult = (ObjectResult)result;

        // Assert
        Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, internalErrorResult.StatusCode);
        Assert.Equal("Internal server error", internalErrorResult.Value);
    }

    [Fact]
    public async Task When_IdToken_Is_Invalid_ShouldReturnInternalServerError()
    {
        // Arrange
        var request = new CdaTokenRequestModel
        {
            GrantType = TokenQueryParams.AuthorizationCodeGrantType,
            ClientId = TokenQueryParams.ValidClientId,
            Code = TokenQueryParams.ValidCode,
            ClientSecret = TokenQueryParams.ValidClientSecret,
            RedirectUri = AsUriValue,
            CodeVerifier = TokenQueryParams.ValidCodeVerifier
        };

        // Simulate a response with a valid IdToken
        _iCdaToken
            .Setup(x => x.PostAsync(It.IsAny<CdaTokenRequestModel>(), It.IsAny<RequestHeaderModel>()))
            .ReturnsAsync(new CdaTokenResponseModel { IdToken = IdToken });

        // Simulate decoding of token but missing "peis_id"
        _mockTokenUtility
            .Setup(x => x.DecodeJwt(It.IsAny<string>()))
            .Throws(new Exception("id_token signature invalid"));

        // Act
        var result = await _controller.PostPeiRetrievalDetailsAsync(request, _requestHeaderModel);
        var internalErrorResult = (ObjectResult)result;

        // Assert
        Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, internalErrorResult.StatusCode);
        Assert.Equal("Internal server error", internalErrorResult.Value);
    }
}