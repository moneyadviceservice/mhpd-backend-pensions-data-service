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
using MhpdCommon.Models.MHPDModels.JwkUri;
using MhpdCommon.SharedHttpClient;

namespace TokenIntegrationServiceUnitTests;

public class TokenControllerUnitTests
{
    private TokenController _controller;
    private readonly Mock<ICdaServiceClient> _iCdaToken = new();
    private readonly Mock<ITokenUtility> _mockTokenUtility = new();
    private readonly Mock<IJwtUtility> _mockJwtUtility = new();
    private readonly Mock<IIdValidator> _mockIdValidator;
    private readonly Mock<ILogger<TokenController>> _mockLogger = new();
    private readonly Mock<TokenIntegrationRequestValidatorPipeline> _mockValidatorPipeline;
    private readonly Mock<PensionsDataRequestValidatorPipeline> _mockCdaValidatorPipeline;

    private readonly RequestHeaderModel _requestHeaderModel = new() { CorrelationId = ValidXRequestId };

    private const string RqpValue = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";
    private const string TicketValue = "eyJ0eXAiOiJKV1QiLCJraWQiOiJ1c2VyLWFjY291bnQtc2lnbmF0dXJlLWtleSIsImVuYyI6IkExMjhDQkMtSFMyNTYiLCJjdHkiOiJKV1QiLCJhbGciOiJSU0EtT0FFUC0yNTYifQ."
                                       + "e8fhuMztCdiet-Q6uW34w-RmlHXbVxLkDwmoDCvO8Y-Fqh_DIvUltm7lRgzugZevFa8VPPOlKuFRH6iBXwFNvtXlyHdYLg1Z2nT-3YV-ylvXiTRn71SesgV_clxHehQv7083zKavWJGxkV4L02maxJC-h3QYuH3KRouf3qfCHxpaLVNRTWxQZXdSArap9Sd5DGAfXWYEy-UmHvdXZ5XLsY_1VQhx4cqwwJKyJXrLCne76tU92ZT_bcreQp2u1gccjtLWOWVMp05ESM-dFp0i5pp3D1YG4FZjUr8K92fRhl683rf9ugxSIl-WqTZ2LdK6XHVFnUJxSiNJoPHedhKr2A."
                                       + "Y4adof4jIngLUFuQ5CeKhw."
                                       + "kplzS7OrJcbzVRI__xEhZZxzXmKRRAH4QhkXlScKmHVa1nq0S8G1yyXRE0XWGclIQma0jcqxOHKve23MdExps5K9dKoUdCbGqOR8SVjJed9UvRysudwyTLThQ9xBRrBMwnx8gGSiNyvEij4jxWR_v2vEEtn_JmnuYTQh8eTZwJUpHj3hk_yH3eRYD3LvDrO5wtMIWVDodFI3ZEFlxk9Ja5TddIru8b_eBgaDdRsEnhW-6Zx51quYkTTf28So_8sRZS2rHxQHJlwDuNPjGbYyiLsJBTKe0e0WCoWLSyKbaNvTjx7Mxl8SHw8Fo3oYls-IqVGftl6EdBvGB-D1ImChlU5f-Ht6n9tt-lqsAJz5Vqli9ARcrTfFOXwSuEWOM1CDqjKnN4AEXd5N0X4ZR4iYt5RwLS4_o6d2m-aA3gQW2gPqj1yywcE6hvX6DBjo9Xm9n4k9pYmfJ7NQsdhQmFpluq6K3OX9BfO5nBpmeocHEcP0QfdLaeAg4cXgBh1OkGEjLXgMOzx-ElWfVnRYFyoRRDAYqBDtZNK3OuMK4rL54enKlc928tIt-Fv7IjfmhR7s."
                                       + "yAKO3wvIMrrlNE1_ln_Zng";
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
        _mockIdValidator = new();
        _mockIdValidator.Setup(v => v.IsValidGuid(It.IsAny<string>())).Returns(true);
            
        // Get ordered validators
        var validators = Helper.GetOrderedValidatorsForTokenIntegrationRequest();

        // Create the TokenRequestValidatorPipeline with the mock validators
        _mockValidatorPipeline = new(validators);
        
        // Get ordered validators
        var cdaValidators = Helper.GetOrderedValidators();

        // Create the TokenRequestValidatorPipeline with the mock validators
        _mockCdaValidatorPipeline = new(cdaValidators);
        
        _iCdaToken
            .Setup(x => x.PostAsync(It.IsAny<CdaTokenRequestModel>()))
            .Returns(Task.FromResult(new CdaTokenResponseModel { IdToken =  IdToken}));
        
        var httpContext = new DefaultHttpContext();
        _controller = new TokenController(_iCdaToken.Object, _mockLogger.Object,
            _mockIdValidator.Object, _mockValidatorPipeline.Object,
            _mockCdaValidatorPipeline.Object, _mockTokenUtility.Object, _mockJwtUtility.Object)
        {
            ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            }
        };
    }
    
    [Fact]
    public async void WhenControllerIsCalled_WithInvalidCorrelationId_ThenItShouldReturn_BadRequest400Response()
    {
        // Arrange           
        var request = new TokenIntegrationRequestModel
        {
            Rqp = RqpValue,
            Ticket = TicketValue,
            AsUri = AsUriValue
        };

        var correlationId = "string.Empty";
        _mockIdValidator.Setup(v => v.IsValidGuid(correlationId)).Returns(false);

        // Act
        var result = await _controller.PostAsync(request, new RequestHeaderModel { CorrelationId = correlationId }); 
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
            .Setup(x => x.PostAsync(It.IsAny<CdaTokenRequestModel>()))
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
        var request = new PensionsDataRequestModel()
        {
            ClientId = TokenQueryParams.ValidClientId,
            ClientSecret = TokenQueryParams.ValidClientSecret,
            AuthorisationCode = TokenQueryParams.ValidCode,
            CodeVerifier = TokenQueryParams.ValidCodeVerifier,
            RedirectUrl = AsUriValue,
        };
        
        _iCdaToken
            .Setup(x => x.PostAsync(It.IsAny<CdaTokenRequestModel>()))
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
        Assert.Equal(TokenValidationMessages.InvalidJweTicketQueryFormat, (string)badResult.Value!);
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
        var request = new PensionsDataRequestModel
        {
            ClientId = TokenQueryParams.ValidClientId,
            ClientSecret = string.Empty,
            AuthorisationCode = TokenQueryParams.ValidCode,
            CodeVerifier = TokenQueryParams.ValidCodeVerifier,
            RedirectUrl = AsUriValue,
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
    public async void WhenController_Pei_Retrieval_Endpoint_IsCalled_InvalidClientId_ThenItShouldReturn_BadRequest400Response()
    {
        // Arrange           
        var request = new PensionsDataRequestModel
        {
            ClientSecret = TokenQueryParams.ValidClientSecret,
            AuthorisationCode = TokenQueryParams.ValidCode,
            CodeVerifier = TokenQueryParams.ValidCodeVerifier,
            RedirectUrl = AsUriValue,
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
        var request = new PensionsDataRequestModel
        {
            ClientId = TokenQueryParams.ValidClientId,
            ClientSecret = TokenQueryParams.ValidClientSecret,
            CodeVerifier = TokenQueryParams.ValidCodeVerifier,
            RedirectUrl = AsUriValue,
        };

        // Act
        var result = await _controller.PostPeiRetrievalDetailsAsync(request, _requestHeaderModel);
        var badResult = (BadRequestObjectResult)result;

        // Assert
        Assert.True(result.GetType() == typeof(BadRequestObjectResult));
        Assert.True(badResult.StatusCode == (int)HttpStatusCode.BadRequest);
        Assert.Equal(TokenValidationMessages.MissingAuthorisationCode, badResult.Value);
    }
    
    [Fact]
    public async void WhenController_Pei_Retrieval_Endpoint_IsCalled_InvalidRedirectUri_ThenItShouldReturn_BadRequest400Response()
    {
        // Arrange           
        var request = new PensionsDataRequestModel
        {
            ClientId = TokenQueryParams.ValidClientId,
            AuthorisationCode = TokenQueryParams.ValidCode,
            ClientSecret = TokenQueryParams.ValidClientSecret,
            CodeVerifier = TokenQueryParams.ValidCodeVerifier
        };

        // Act
        var result = await _controller.PostPeiRetrievalDetailsAsync(request, _requestHeaderModel);
        var badResult = (BadRequestObjectResult)result;

        // Assert
        Assert.True(result.GetType() == typeof(BadRequestObjectResult));
        Assert.True(badResult.StatusCode == (int)HttpStatusCode.BadRequest);
        Assert.Equal(TokenValidationMessages.RedirectUriNotPresent, badResult.Value);
    }
    
    [Fact]
    public async void WhenController_Pei_Retrieval_Endpoint_IsCalled_InvalidCodeVerifier_ThenItShouldReturn_BadRequest400Response()
    {
        // Arrange
        var request = new PensionsDataRequestModel
        {
            ClientId = TokenQueryParams.ValidClientId,
            AuthorisationCode = TokenQueryParams.ValidCode,
            ClientSecret = TokenQueryParams.ValidClientSecret,
            RedirectUrl = AsUriValue
        };

        // Act
        var result = await _controller.PostPeiRetrievalDetailsAsync(request, _requestHeaderModel);
        var badResult = (BadRequestObjectResult)result;

        // Assert
        Assert.True(result.GetType() == typeof(BadRequestObjectResult));
        Assert.True(badResult.StatusCode == (int)HttpStatusCode.BadRequest);
        Assert.Equal(TokenValidationMessages.CodeVerifierNotPresent, badResult.Value);
    }
    
    [Fact]
    public async Task WhenPeisIdIsMissingInIdToken_ShouldReturnInternalServerError()
    {
        // Arrange
        var request = new PensionsDataRequestModel
        {
            ClientId = TokenQueryParams.ValidClientId,
            AuthorisationCode = TokenQueryParams.ValidCode,
            ClientSecret = TokenQueryParams.ValidClientSecret,
            RedirectUrl = AsUriValue,
            CodeVerifier = TokenQueryParams.ValidCodeVerifier
        };

        // Simulate a response with a valid IdToken
        _iCdaToken
            .Setup(x => x.PostAsync(It.IsAny<CdaTokenRequestModel>()))
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
        var request = new PensionsDataRequestModel
        {
            ClientId = TokenQueryParams.ValidClientId,
            AuthorisationCode = TokenQueryParams.ValidCode,
            ClientSecret = TokenQueryParams.ValidClientSecret,
            RedirectUrl = AsUriValue,
            CodeVerifier = TokenQueryParams.ValidCodeVerifier
        };

        // Simulate a response with a valid IdToken
        _iCdaToken
            .Setup(x => x.PostAsync(It.IsAny<CdaTokenRequestModel>()))
            .ReturnsAsync(new CdaTokenResponseModel { IdToken = IdToken });

        _mockJwtUtility
            .Setup(x => x.ValidateJwtTokenWithKidAsync(It.IsAny<string>(), _mockLogger.Object))
            .Throws(new Exception("id_token signature invalid"));

        // Act
        var result = await _controller.PostPeiRetrievalDetailsAsync(request, _requestHeaderModel);
        var internalErrorResult = (ObjectResult)result;

        // Assert
        Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, internalErrorResult.StatusCode);
        Assert.Equal("Internal server error", internalErrorResult.Value);
    }
    
    [Fact]
    public async Task When_AccessToken_Is_Empty_ShouldReturnInternalServerError()
    {
        // Arrange
        var request = new TokenIntegrationRequestModel
        {
            Rqp = RqpValue,
            Ticket = TicketValue,
            AsUri = AsUriValue
        };

        // Simulate a response with an empty IdToken
        _iCdaToken
            .Setup(x => x.PostAsync(It.IsAny<CdaTokenRequestModel>()))
            .ReturnsAsync(new CdaTokenResponseModel { AccessToken = string.Empty });

        // Act
        var result = await _controller.PostAsync(request, _requestHeaderModel);
        var internalErrorResult = (ObjectResult)result;

        // Assert
        Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, internalErrorResult.StatusCode);
        Assert.Equal("Internal server error", internalErrorResult.Value);
    }
    
    [Fact]
    public async Task When_AccessToken_Is_Invalid_ShouldReturnInternalServerError()
    {
        // Arrange
        var request = new TokenIntegrationRequestModel
        {
            Rqp = RqpValue,
            Ticket = TicketValue,
            AsUri = AsUriValue
        };

        // Simulate a response with an invalid IdToken
        _iCdaToken
            .Setup(x => x.PostAsync(It.IsAny<CdaTokenRequestModel>()))
            .ReturnsAsync(new CdaTokenResponseModel { AccessToken = InvalidTicketValue });

        _mockJwtUtility
            .Setup(x => x.ValidateJwtTokenWithKidAsync(It.IsAny<string>(), _mockLogger.Object))
            .Throws(new Exception("AccessToken signature invalid"));

        // Act
        var result = await _controller.PostAsync(request, _requestHeaderModel);
        var internalErrorResult = (ObjectResult)result;

        // Assert
        Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, internalErrorResult.StatusCode);
        Assert.Equal("Internal server error", internalErrorResult.Value);
    }

    [Theory]
    [InlineData(JwkConstants.TestTokenNoKid, true, 500)]
    [InlineData(JwkConstants.TestTokenUnknownKid, true, 500)]
    [InlineData(JwkConstants.TestTokenExpired, true, 500)]
    [InlineData(JwkConstants.TestTokenUnknownKey, true, 500)]
    [InlineData(JwkConstants.TestTokenValidKey, false, 200)]
    public async Task When_AccessToken_Is_Not_Valid_ShouldReturnInternalServerError(string token, bool shouldfailValidation, int code)
    {
        // Arrange
        var request = new TokenIntegrationRequestModel
        {
            Rqp = RqpValue,
            Ticket = TicketValue,
            AsUri = AsUriValue
        };

        // Simulate a response with an invalid IdToken
        _iCdaToken
            .Setup(x => x.PostAsync(It.IsAny<CdaTokenRequestModel>()))
            .ReturnsAsync(new CdaTokenResponseModel { AccessToken = token });
        var client = new Mock<ISharedHttpClient>();
        client.Setup(mock => mock.GetAsync()).ReturnsAsync(JwkUriResponseModel.Default);

        var httpContext = new DefaultHttpContext();
        _controller = new TokenController(_iCdaToken.Object, _mockLogger.Object,
            _mockIdValidator.Object, _mockValidatorPipeline.Object,
            _mockCdaValidatorPipeline.Object, _mockTokenUtility.Object, new JwtUtility(client.Object))
        {
            ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            }
        };

        // Act
        var result = await _controller.PostAsync(request, _requestHeaderModel);
        var internalErrorResult = (ObjectResult)result;

        // Assert
        Assert.Equal(code, internalErrorResult.StatusCode);

        if (shouldfailValidation)
        {
            Assert.IsType<ObjectResult>(result);
            Assert.Equal("Internal server error", internalErrorResult.Value);
        }
    }
}