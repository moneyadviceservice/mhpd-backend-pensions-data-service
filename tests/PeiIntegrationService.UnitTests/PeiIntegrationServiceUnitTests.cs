using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PeiIntegratioinService.Controllers;
using PeiIntegrationService.HttpClients.Interfaces;
using PeiIntegrationService.Models.CdaPiesService;
using PeiIntegrationService.Models.MapsCdaService;
using PeiIntegrationService.Models.PeiIntegrationService;
using PeiIntegrationService.Models.TokenIntegrationService;

namespace PeiIntegrationService.UnitTests
{
    public class PeiIntegrationServiceUnitTests
    {

        private readonly PeIController _controller;
        private readonly DefaultHttpContext _httpContext;

        private readonly Mock<IMapsRqpServiceClient> _iMapsCdaService = new Mock<IMapsRqpServiceClient>();
        readonly Mock<ICdaPiesServiceClient> _iCDAPiesServiceClient = new Mock<ICdaPiesServiceClient>();
        private readonly Mock<ITokenIntegrationServiceClient> _iTokenIntegrationService = new Mock<ITokenIntegrationServiceClient>();

        public PeiIntegrationServiceUnitTests()
        {
            _httpContext = new DefaultHttpContext();

            _controller = new PeIController(_iCDAPiesServiceClient.Object,
                                            _iMapsCdaService.Object,
                                            _iTokenIntegrationService.Object)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = _httpContext
                }
            };

            SetupExternalServiceMocks();
        }

        [Fact]
        public async void WhenControllerIsCalled_WithRptProvided_AndOtherHeadersProvided_AndCorrectBody_ThenItShouldReturnPiesAnd_Ok200Response()
        {
            // Arrange
            AddAuthorisationHeader();
            var request = new PeiIntegrationServiceRequestModel
            {
                RequestId = "b7301d11-f166-499a-9bf1-0598c2f1af52",
                PeisId = "cd0e4fdc-8586-4483-9899-17dd85af9074",
            };

            AddOtherHeaders("https://maps.com", "5a608b97-d738-4da7-b07d-f81861b5d60e");

            _iCDAPiesServiceClient.Setup(x => x.GetPiesAsync(It.IsAny<CdaPiesServiceRequestModel>()))
                .Returns(Task.FromResult<CdaPiesServiceResponseModel?>(new CdaPiesServiceResponseModel
                {
                    Peis = [
                            new PeiModel
                            {
                                Pei = "7d138640-8651-4b66-8c33-70c26059487e:b27e3471-49bf-4cc5-9e2e-9991bf89e1bc",
                                Description = "My Chicken and Mushroom Pies",
                                RetrievalRequestedTimestamp = DateTime.UtcNow,
                                RetrievalStatus = "New"
                            }
                        ],
                    ResponseMessage = new ResponseMessage
                    {
                        ResponseStatusCode = "200"
                    }
                }));

            // Act
            var result = await _controller.GetAsync(request);
            OkObjectResult okResult = (OkObjectResult)result;
            var data = (PeiModel[])okResult!.Value!;

            // Assert
            Assert.NotNull(result);
            Assert.True(result.GetType() == typeof(OkObjectResult));
            Assert.True(data.GetType() == typeof(PeiModel[]));
            Assert.True(okResult.StatusCode == (int)HttpStatusCode.OK);
        }

        [Fact]
        public async void WhenControllerIsCalled_WithoutRpt_ButWithOtherHeaders_CorrectBody_ThenItShouldPerformAuthenticationDance_AndReturnPiesAnd_Ok200Response()
        {
            //Arrange
            var wwwAuthenticateHeader = GetWwwAuthenticateResopnseHeader();
            AddOtherHeaders("https://maps.com", "5a608b97-d738-4da7-b07d-f81861b5d60e");

            var request = new PeiIntegrationServiceRequestModel
            {
                RequestId = "b7301d11-f166-499a-9bf1-0598c2f1af52",
                PeisId = "cd0e4fdc-8586-4483-9899-17dd85af9074",
            };

            _iCDAPiesServiceClient.Setup(x => x.GetPiesAsync(It.Is<CdaPiesServiceRequestModel>(x => !string.IsNullOrEmpty(x.Rpt))))
                .Returns(
                            Task.FromResult<CdaPiesServiceResponseModel?>(new CdaPiesServiceResponseModel
                            {
                                Peis =
                                [
                                    new PeiModel
                                    {
                                        Pei = "7d138640-8651-4b66-8c33-70c26059487e:b27e3471-49bf-4cc5-9e2e-9991bf89e1bc",
                                        Description = "My Steak Pies",
                                        RetrievalRequestedTimestamp = default,
                                        RetrievalStatus = "New"
                                    }
                                ],
                                ResponseMessage = new ResponseMessage
                                {
                                    ResponseStatusCode = "200"
                                }
                            }
                        ));

            _iCDAPiesServiceClient.Setup(x => x.GetPiesAsync(It.Is<CdaPiesServiceRequestModel>(x => string.IsNullOrEmpty(x.Rpt))))
                .Returns(
                            Task.FromResult<CdaPiesServiceResponseModel?>(new CdaPiesServiceResponseModel
                            {
                                Peis = null,
                                ResponseMessage = new ResponseMessage
                                {
                                    ResponseStatusCode = "401",
                                    WWWAuthenticateResponseHeader = wwwAuthenticateHeader
                                }
                            }
                        ));

            // Act
            var result = await _controller.GetAsync(request);
            OkObjectResult okResult = (OkObjectResult)result;
            var data = (PeiModel[])okResult!.Value!;

            // Assert
            Assert.NotNull(result);
            Assert.True(data.GetType() == typeof(PeiModel[]));
            Assert.True(result.GetType() == typeof(OkObjectResult));
            Assert.True(okResult.StatusCode == (int)HttpStatusCode.OK);
        }

        [Fact]
        public async void WhenControllerIsCalled_WithCorrectHeaders_InCorrectBody_ThenItShouldReturn_BadRequest400Response()
        {
            //Arrange
            AddAuthorisationHeader();
            var request = new PeiIntegrationServiceRequestModel
            {
                RequestId = "qwertyuoip",
                PeisId = string.Empty,
            };
            AddOtherHeaders("https://maps.com", "5a608b97-d738-4da7-b07d-f81861b5d60e");

            // Act
            var result = await _controller.GetAsync(request);

            // Assert
            Assert.True(result.GetType() == typeof(BadRequestObjectResult));
        }

        [Fact]
        public async void WhenControllerIsCalled_WithCorrectHeaders_InCorrectBody_ThenItShouldReturn_Bad_Request400Response()
        {
            //Arrange
            AddOtherHeaders("https://maps.com", "5a608b97-d738-4da7-b07d-f81861b5d60e");
            var request = new PeiIntegrationServiceRequestModel
            {
                RequestId = string.Empty,
                PeisId = "cd0e4fdc-8586-4483-9899-17dd85af9074",
            };

            // Act
            var result = await _controller.GetAsync(request);

            // Assert
            Assert.True(result.GetType() == typeof(BadRequestObjectResult));

        }

        [Fact]
        public async void WhenControllerIsCalled_WithAuthHeaders_InCorrect_Iss_CorrectBody_ThenItShouldReturn_Bad_Request400Response()
        {
            //Arrange
            AddOtherHeaders(string.Empty, "5a608b97-d738-4da7-b07d-f81861b5d60e");
            var request = new PeiIntegrationServiceRequestModel
            {
                RequestId = "qwertyuoip",
                PeisId = "cd0e4fdc-8586-4483-9899-17dd85af9074",
            };

            // Act
            var result = await _controller.GetAsync(request);

            // Assert
            Assert.True(result.GetType() == typeof(BadRequestObjectResult));

        }

        private void AddAuthorisationHeader()
        {
            _httpContext.Request.Headers["rpt"] = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";
        }

        private void AddOtherHeaders(string iss, string userSessionId)
        {
            _httpContext.Request.Headers["iss"] = iss;
            _httpContext.Request.Headers["userSessionId"] = userSessionId;
        }

        private string GetWwwAuthenticateResopnseHeader()
        {
            return "realm=\"PensionDashboard\", " +
                "as_uri=\"https://as.pdp.com\", " +
                "ticket=\"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.cThIIoDvwdueQB468K5xDc5633seEFoqwxjF_xSJyQQ\"";
        }

        private string GetRqp()
        {
            return "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.cThIIoDvwdueQB468K5xDc5633seEFoqwxjF_xSJyQQ";

        }

        private string GetRpt()
        {
            return "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJsb2dnZWRJbkFzIjoiYWRtaW4iLCJpYXQiOjE0MjI3Nzk2Mzh9.gzSraSYS8EXBxLN_oWnFSRgCzcmJmMjLiuyu5CSpyHI";
        }

        private void SetupExternalServiceMocks()
        {
            _iMapsCdaService.Setup(x => x.PostRqp(It.IsAny<MapsRqpServiceRequestModel>()))
                .Returns(Task.FromResult(new MapsRqpServiceResponseModel
                {
                    Rqp = GetRqp()

                }
            )); ;

            _iTokenIntegrationService.Setup(x => x.PostRpt(It.IsAny<TokenIntegrationServiceRequestModel>()))
                .Returns(Task.FromResult(new TokenIntegrationResponseModel
                {
                    Rpt = GetRpt()
                }
            ));
        }
    }
}
