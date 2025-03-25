using MhpdCommon.Models.MHPDModels;
using MhpdCommon.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PeiIntegrationService.Controllers;
using PeiIntegrationService.HttpClients.Interfaces;
using PeiIntegrationService.Models.CdaPiesService;
using PeiIntegrationService.Models.PeiIntegrationService;
using System.Net;
using PeiIntegrationService.Models.CdaPeisServiceClient;
using PeiIntegrationService.Models.PeiIntegrationServiceClient;

namespace PeiIntegrationService.UnitTests;

public class PeiIntegrationServiceUnitTests
{
    private readonly PeIController _controller;
    private readonly DefaultHttpContext _httpContext;
    private readonly Mock<IIdValidator> _idValidator = new();
    private readonly Mock<ICdaPiesServiceClient> _iCDAPiesServiceClient = new();

    public PeiIntegrationServiceUnitTests()
    {
        _httpContext = new DefaultHttpContext();
        var logger = new Mock<ILogger<PeIController>>();

        _controller = new PeIController(_iCDAPiesServiceClient.Object,
                                        _idValidator.Object,
                                        logger.Object)
        {
            ControllerContext = new ControllerContext()
            {
                HttpContext = _httpContext
            }
        };
    }

    [Fact]
    public async void WhenControllerIsCalled_WithRptProvided_AndOtherHeadersProvided_AndCorrectBody_ThenItShouldReturnPiesAnd_Ok200Response()
    {
        // Arrange
        var request = AddHeadersModel("https://maps.com", "5a608b97-d738-4da7-b07d-f81861b5d60e", "cd0e4fdc-8586-4483-9899-17dd85af9074", true);

        _iCDAPiesServiceClient.Setup(x => x.GetPiesAsync(It.IsAny<CdaPiesServiceRequestModel>()))
            .Returns(Task.FromResult<CdaPeisServiceResponseModel?>(new CdaPeisServiceResponseModel
            {
                Peis = [
                        new PeiDataModel
                        {
                            Pei = "7d138640-8651-4b66-8c33-70c26059487e:b27e3471-49bf-4cc5-9e2e-9991bf89e1bc",
                            Description = "My Chicken and Mushroom Pies",
                            RetrievalRequestedTimestamp = DateTime.UtcNow,
                            RetrievalStatus = "New"
                        }
                    ],
                ResponseMessage = new ResponseMessage
                {
                    ResponseStatusCode = HttpStatusCode.OK,
                }
            }));

        _idValidator.Setup(mock => mock.IsValidGuid(It.IsAny<string>())).Returns(true);
        _idValidator.Setup(mock => mock.IsValidPeisId(It.IsAny<string>())).Returns(true);

        // Act
        var result = await _controller.GetAsync(request);
        OkObjectResult okResult = (OkObjectResult)result;
        var data = (CdaPeisServiceResponseModel)okResult!.Value!;

        // Assert
        Assert.NotNull(data);
        Assert.True(okResult.StatusCode == (int)HttpStatusCode.OK);
    }

    [Fact]
    public async void WhenControllerIsCalled_WithoutRpt_ButWithOtherHeaders_CorrectBody_ThenItShouldPerformAuthenticationDance_AndReturnPiesAnd_Ok200Response()
    {
        //Arrange
        var wwwAuthenticateHeader = GetWwwAuthenticateResopnseHeader();
        var request = AddHeadersModel("https://maps.com", "6d43ba5d-fd9d-4825-9b0f-f141bf5d6a9f", "0001f518264ba44564b186f42af6659b5822eb6e");

        _iCDAPiesServiceClient.Setup(x => x.GetPiesAsync(It.Is<CdaPiesServiceRequestModel>(x => !string.IsNullOrEmpty(x.Rpt))))
            .Returns(
                        Task.FromResult<CdaPeisServiceResponseModel?>(new CdaPeisServiceResponseModel
                        {
                            Peis =
                            [
                                new PeiDataModel
                                {
                                    Pei = "7d138640-8651-4b66-8c33-70c26059487e:b27e3471-49bf-4cc5-9e2e-9991bf89e1bc",
                                    Description = "My Steak Pies",
                                    RetrievalRequestedTimestamp = default,
                                    RetrievalStatus = "New"
                                }
                            ],
                            ResponseMessage = new ResponseMessage
                            {
                                ResponseStatusCode = HttpStatusCode.OK,
                            }
                        }
                    ));

        _iCDAPiesServiceClient.Setup(x => x.GetPiesAsync(It.Is<CdaPiesServiceRequestModel>(x => string.IsNullOrEmpty(x.Rpt))))
            .Returns(
                        Task.FromResult<CdaPeisServiceResponseModel?>(new CdaPeisServiceResponseModel
                        {
                            Peis = null,
                            ResponseMessage = new ResponseMessage
                            {
                                ResponseStatusCode = HttpStatusCode.Unauthorized,
                                WwwAuthenticateResponseHeader = wwwAuthenticateHeader
                            }
                        }
                    ));

        _idValidator.Setup(mock => mock.IsValidPeisId(It.IsAny<string>())).Returns(true);
        _idValidator.Setup(mock => mock.IsValidGuid(It.IsAny<string>())).Returns(true);

        // Act
        var result = await _controller.GetAsync(request);
        OkObjectResult okResult = (OkObjectResult)result;
        var data = (CdaPeisServiceResponseModel)okResult!.Value!;

        // Assert
        Assert.NotNull(data);
        Assert.True(okResult.StatusCode == (int)HttpStatusCode.OK);
    }

    [Fact]
    public async void WhenControllerIsCalled_WithCorrectHeaders_InCorrectBody_ThenItShouldReturn_BadRequest400Response()
    {
        //Arrange
        var request = AddHeadersModel("https://maps.com", "5a608b97-d738-4da7-b07d-f81861b5d60e", string.Empty, true);

        // Act
        var result = await _controller.GetAsync(request);

        // Assert
        Assert.True(result.GetType() == typeof(BadRequestObjectResult));
    }

    [Fact]
    public async void WhenControllerIsCalled_WithCorrectHeaders_InCorrectBody_ThenItShouldReturn_Bad_Request400Response()
    {
        //Arrange
        var request = AddHeadersModel("https://maps.com", "5a608b97-d738-4da7-b07d-f81861b5d60e", "cd0e4NOT-AAAA-GUID-9899-17dd85af9074");

        // Act
        var result = await _controller.GetAsync(request);

        // Assert
        Assert.True(result.GetType() == typeof(BadRequestObjectResult));

    }

    [Fact]
    public async void WhenControllerIsCalled_WithAuthHeaders_InCorrect_Iss_CorrectBody_ThenItShouldReturn_Bad_Request400Response()
    {
        //Arrange
        var request = AddHeadersModel(string.Empty, "5a608b97-d738-4da7-b07d-f81861b5d60e", "cd0e4fdc-8586-4483-9899-17dd85af9074");

        // Act
        var result = await _controller.GetAsync(request);

        // Assert
        Assert.True(result.GetType() == typeof(BadRequestObjectResult));

    }

    private static PeiIntegrationServiceRequestModel AddHeadersModel(string iss, string userSessionId, string peisId, bool withRpt = false)
    {
        const string rpt = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";
        return new PeiIntegrationServiceRequestModel
        {
            Iss = iss,
            UserSessionId = userSessionId,
            PeisId = peisId,
            Rpt = withRpt ? rpt : null
        };
    }

    private static string GetWwwAuthenticateResopnseHeader()
    {
        return "realm=\"PensionDashboard\", " +
            "as_uri=\"https://as.pdp.com\", " +
            "ticket=\"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.cThIIoDvwdueQB468K5xDc5633seEFoqwxjF_xSJyQQ\"";
    }

    private static string GetRqp()
    {
        return "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.cThIIoDvwdueQB468K5xDc5633seEFoqwxjF_xSJyQQ";

    }

    private static string GetRpt()
    {
        return "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJsb2dnZWRJbkFzIjoiYWRtaW4iLCJpYXQiOjE0MjI3Nzk2Mzh9.gzSraSYS8EXBxLN_oWnFSRgCzcmJmMjLiuyu5CSpyHI";
    }
}
