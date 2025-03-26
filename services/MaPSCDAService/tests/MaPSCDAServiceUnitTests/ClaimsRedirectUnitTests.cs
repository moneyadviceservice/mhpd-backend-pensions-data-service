using MaPSCDAService.Controllers;
using MhpdCommon.Constants;
using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.Models.MHPDModels;
using MhpdCommon.Models.RequestHeaderModel;
using MhpdCommon.Repository;
using MhpdCommon.SharedHttpClient;
using MhpdCommon.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;

namespace MaPSCDAServiceUnitTests;

public class ClaimsRedirectUnitTests
{
    private readonly Mock<IIdValidator> _validator = new();
    private readonly Mock<ITokenUtility> _tokenUtility = new();
    private readonly Mock<IPeiServiceClient> _peiClient = new();
    private readonly Mock<ICosmosDbRepository<UserSessionData>> _sessionRepository = new();
    private readonly Mock<ITokenIntegrationServiceClient> _tokenClient = new();
    private readonly ClaimsRedirectController _controller;

    public ClaimsRedirectUnitTests()
    {
        var logger = new Mock<ILogger<ClaimsRedirectController>>();

        _validator.Setup(mock => mock.IsValidGuid(It.IsAny<string>())).Returns(true);
        _validator.Setup(mock => mock.IsValidPeisId(It.IsAny<string>())).Returns(true);
        _tokenUtility.Setup(mock => mock.GenerateJwt(It.IsAny<CustomClaimDataModel>())).Returns("rqp");
        _peiClient.Setup(mock => mock.GetPeiDataAsync(It.IsAny<PeiRequestModel>())).ReturnsAsync(GetPeiResponse());
        _tokenClient.Setup(mock => mock.PostRptAsync(It.IsAny<TokenClientRequestModel>())).ReturnsAsync(GetTokenResponse());
        _sessionRepository.Setup(mock => mock.GetByIdAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(GetSessionData());

        _controller = new ClaimsRedirectController(logger.Object, _validator.Object, _tokenUtility.Object, _peiClient.Object, _tokenClient.Object, _sessionRepository.Object);
    }

    [Fact]
    public async Task GetRedirect_WithInvalidSessionId_ReturnsBadRequest()
    {
        // Arrange
        var request = GetRequest();

        _validator.Setup(mock => mock.IsValidGuid(It.IsAny<string>())).Returns(false);

        // Act
        var response = await _controller.GetRedirectAsync(request);

        // Assert
        Assert.IsType<BadRequestObjectResult>(response);
    }
    
    [Fact]
    public async Task GetRedirect_WithInvalidCorrelationId_ReturnsBadRequest()
    {
        // Arrange
        var request = GetRequest();

        _validator.Setup(mock => mock.IsValidGuid("A")).Returns(false);

        // Act
        var response = await _controller.GetRedirectAsync(request);

        // Assert
        Assert.IsType<BadRequestObjectResult>(response);
    }
    
    [Fact]
    public async Task GetRedirect_WithEmptyCorrelationId_ReturnsBadRequest()
    {
        // Arrange
        var request = GetRequest();
        request.CorrelationId = string.Empty;

        _validator.Setup(mock => mock.IsValidGuid(It.IsAny<string>())).Returns(true);

        // Act
        var response = await _controller.GetRedirectAsync(request);

        // Assert
        Assert.IsType<OkObjectResult>(response);
    }
    
    [Fact]
    public async Task GetRedirect_WithInvalidPeisId_ReturnsBadRequest()
      {
        // Arrange
        var request = GetRequest();

        _validator.Setup(mock => mock.IsValidPeisId(It.IsAny<string>())).Returns(false);

        // Act
        var response = await _controller.GetRedirectAsync(request);

        // Assert
        var result = Assert.IsType<ObjectResult>(response);
        Assert.Equal(500, result.StatusCode);
    }

    [Fact]
    public async Task GetRedirect_WithNoSessionData_ReturnsServerError()
    {
        // Arrange
        var request = GetRequest();

        _sessionRepository.Setup(mock => mock.GetByIdAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(GetSessionData(false));

        // Act
        var response = await _controller.GetRedirectAsync(request);

        // Assert
        var result = Assert.IsType<ObjectResult>(response);
        Assert.Equal(500, result.StatusCode);
    }

    [Fact]
    public async Task GetRedirect_WithNoUnAuthorisedTicket_ReturnsServerError()
    {
        // Arrange
        var request = GetRequest();

        _peiClient.Setup(mock => mock.GetPeiDataAsync(It.IsAny<PeiRequestModel>())).ReturnsAsync(GetPeiResponse(true));

        // Act
        var response = await _controller.GetRedirectAsync(request);

        // Assert
        var result = Assert.IsType<ObjectResult>(response);
        Assert.Equal(500, result.StatusCode);
    }

    [Fact]
    public async Task GetRedirect_WithNoForbiddenTicket_ReturnsServerError()
    {
        // Arrange
        var request = GetRequest();

        _tokenClient.Setup(mock => mock.PostRptAsync(It.IsAny<TokenClientRequestModel>())).ReturnsAsync(GetTokenResponse(true));

        // Act
        var response = await _controller.GetRedirectAsync(request);

        // Assert
        var result = Assert.IsType<ObjectResult>(response);
        Assert.Equal(500, result.StatusCode);
    }

    [Fact]
    public async Task GetRedirect_WithForbiddenTicket_ReturnsClaimsGatheringDetails()
    {
        // Arrange
        var request = GetRequest();

        // Act
        var response = await _controller.GetRedirectAsync(request);

        // Assert
        Assert.IsType<OkObjectResult>(response);
    }
    
    [Fact]
    public async Task GetRedirect_WithForbiddenTicket_Returns_Null_ClaimsGatheringDetails()
    {
        // Arrange
        var request = GetRequest();
        
        _tokenClient.Setup(mock => mock
            .PostRptAsync(It.IsAny<TokenClientRequestModel>()))
            .ReturnsAsync(GetTokenResponse(false, false));

        // Act
        var response = await _controller.GetRedirectAsync(request);

        // Assert
        var result = Assert.IsType<ObjectResult>(response);
        Assert.Equal(500, result.StatusCode);
    }
    
    [Fact]
    public async Task GetRedirect_WithForbiddenTicket_Returns_ClaimsGatheringDetails_With_Invalid_Ticket()
    {
        // Arrange
        var request = GetRequest();
        
        _tokenClient.Setup(mock => mock
                .PostRptAsync(It.IsAny<TokenClientRequestModel>()))
            .ReturnsAsync(GetTokenResponse(false, true, false));

        // Act
        var response = await _controller.GetRedirectAsync(request);

        // Assert
        var result = Assert.IsType<ObjectResult>(response);
        Assert.Equal(500, result.StatusCode);
    }

    private static RequestHeaderModel GetRequest()
    {
        return new RequestHeaderModel
        {
            CorrelationId = "A",
            Iss = "B",
            UserSessionId = "C"
        };
    }

    private static UserSessionData? GetSessionData(bool success = true)
    {
        return success ? new UserSessionData { PeisId = "PeisId", UserSessionId = "Session Id" } : null;
    }

    private static CdaTokenResponseModel GetTokenResponse(bool success = false,
        bool withRedirect = true, bool validClaimsTicket = true)
    {
        if (success)
        {
            return new CdaTokenResponseModel
            {
                AccessToken = "AccessToken",
                StatusCode = HttpStatusCode.OK
            };
        }
        else
        {
            return new CdaTokenResponseModel
            {
                StatusCode = HttpStatusCode.Forbidden,
                UserRedirectDetails = withRedirect ? new ClaimsGatheringResponseModel
                {
                    RedirectUser = "http://id.cda.com/claim_gathering",
                    Ticket = validClaimsTicket ? SecurityConstants.Jwe.ClaimsRequiredPermissionTicket : string.Empty
                } : null
            };
        }
    }

    private static CdaPeisServiceResponseModel GetPeiResponse(bool success = false)
    {
        if (success)
        {
            return new CdaPeisServiceResponseModel
            {
                Peis = [],
                ResponseMessage = new ResponseMessage
                {
                    ResponseStatusCode = HttpStatusCode.OK
                }
            };
        }
        else
        {
            return new CdaPeisServiceResponseModel
            {
                ResponseMessage = new ResponseMessage
                {
                    ResponseStatusCode = HttpStatusCode.Unauthorized,
                    WwwAuthenticateResponseHeader = GetWwwResopnseHeader()
                }
            };
        }
    }

    private static string GetWwwResopnseHeader()
    {
        return "realm=\"PensionDashboard\", " +
            "as_uri=\"https://as.pdp.com\", " +
            $"ticket=\"{SecurityConstants.Jwe.AuthorizationRequiredPermissionTicket}\"";
    }
}
