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
    private readonly Mock<IIdValidator> validator = new();
    private readonly Mock<ITokenUtility> tokenUtility = new();
    private readonly Mock<IPeiServiceClient> peiClient = new();
    private readonly Mock<ICosmosDbRepository<UserSessionData>> sessionRespository = new();
    private readonly Mock<ITokenIntegrationServiceClient> tokenClient = new();
    private readonly ClaimsRedirectController controller;

    public ClaimsRedirectUnitTests()
    {
        var logger = new Mock<ILogger<ClaimsRedirectController>>();

        validator.Setup(mock => mock.IsValidGuid(It.IsAny<string>())).Returns(true);
        tokenUtility.Setup(mock => mock.GenerateJwt(It.IsAny<CustomClaimDataModel>())).Returns("rqp");
        peiClient.Setup(mock => mock.GetPeiDataAsync(It.IsAny<PeiRequestModel>())).ReturnsAsync(GetPeiResponse());
        tokenClient.Setup(mock => mock.PostRptAsync(It.IsAny<TokenClientRequestModel>())).ReturnsAsync(GetTokenResponse());
        sessionRespository.Setup(mock => mock.GetByIdAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(GetSessionData());

        controller = new ClaimsRedirectController(logger.Object, validator.Object, tokenUtility.Object, peiClient.Object, tokenClient.Object, sessionRespository.Object);
    }

    [Fact]
    public async Task GetRedirect_WithInvalidSessionId_ReturnsBadRequest()
    {
        // Arrange
        var request = GetRequest();

        validator.Setup(mock => mock.IsValidGuid(It.IsAny<string>())).Returns(false);

        // Act
        var response = await controller.GetRedirectAsync(request);

        // Assert
        Assert.IsType<BadRequestObjectResult>(response);
    }

    [Fact]
    public async Task GetRedirect_WithNoSessionData_ReturnsServerError()
    {
        // Arrange
        var request = GetRequest();

        sessionRespository.Setup(mock => mock.GetByIdAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(GetSessionData(false));

        // Act
        var response = await controller.GetRedirectAsync(request);

        // Assert
        var result = Assert.IsType<ObjectResult>(response);
        Assert.Equal(500, result.StatusCode);
    }

    [Fact]
    public async Task GetRedirect_WithNoUnAuthorisedTicket_ReturnsServerError()
    {
        // Arrange
        var request = GetRequest();

        peiClient.Setup(mock => mock.GetPeiDataAsync(It.IsAny<PeiRequestModel>())).ReturnsAsync(GetPeiResponse(true));

        // Act
        var response = await controller.GetRedirectAsync(request);

        // Assert
        var result = Assert.IsType<ObjectResult>(response);
        Assert.Equal(500, result.StatusCode);
    }

    [Fact]
    public async Task GetRedirect_WithNoForbiddenTicket_ReturnsServerError()
    {
        // Arrange
        var request = GetRequest();

        tokenClient.Setup(mock => mock.PostRptAsync(It.IsAny<TokenClientRequestModel>())).ReturnsAsync(GetTokenResponse(true));

        // Act
        var response = await controller.GetRedirectAsync(request);

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
        var response = await controller.GetRedirectAsync(request);

        // Assert
        Assert.IsType<OkObjectResult>(response);
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

    private static CdaTokenResponseModel GetTokenResponse(bool success = false)
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
                UserRedirectDetails = new ClaimsGatheringResponseModel
                {
                    RedirectUser = "http://id.cda.com/claim_gathering",
                    Ticket = SecurityConstants.Jwe.ClaimsRequiredPermissionTicket
                }
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
