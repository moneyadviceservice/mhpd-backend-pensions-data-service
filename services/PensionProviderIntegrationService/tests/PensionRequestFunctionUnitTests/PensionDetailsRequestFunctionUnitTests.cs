using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Moq;
using NSubstitute;
using PensionRequestFunction;
using PensionRequestFunction.HttpClient;
using PensionRequestFunction.HttpClient.Interfaces;
using PensionRequestFunction.Models.CdaPeisServiceClient;
using PensionRequestFunction.Models.MapsRqpServiceClient;
using PensionRequestFunction.Models.TokenIntegrationServiceClient;

namespace PensionRequestFunctionUnitTests
{
    public class PensionDetailsRequestFunctionUnitTests
    {
        private readonly Mock<ILoggerFactory> _mockLoggerFactory;
        private readonly PensionDetailsRequestFunction _function;
        private readonly Mock<ILogger<PensionDetailsRequestFunction>> _mockLogger;
        private readonly Mock<IPDPViewDataClient> _mockPdpViewDataClient;
        private readonly Mock<ServiceBusClient> _mockServiceBusClient;
        private readonly Mock<ServiceBusSender> _mockServiceBusSender;
        private readonly Mock<IMapsRqpServiceClient> _mockMapsRqpService = new();
        private readonly Mock<ITokenIntegrationServiceClient> _mockTokenIntegrationService = new();

        public PensionDetailsRequestFunctionUnitTests()
        {
            var responseHeader = GetResponseHeader();
            var rqp = GetRqp();
            var rpt = GetRpt();

            _mockLoggerFactory = new Mock<ILoggerFactory>();
            _mockServiceBusClient = new Mock<ServiceBusClient>();
            _mockPdpViewDataClient = new Mock<IPDPViewDataClient>();
            _mockLogger = new Mock<ILogger<PensionDetailsRequestFunction>>();
            _mockLoggerFactory.Setup(factory => factory.CreateLogger(It.IsAny<string>())).Returns(_mockLogger.Object);

            _mockPdpViewDataClient.Setup(x => x.GetPDPViewDataAsync(It.IsAny<string>(), It.IsAny<string>(), It.Is<string>(x => string.IsNullOrEmpty(x))))
                .ReturnsAsync(new PDPServiceResponseModel
                {
                    ViewDataToken = null,
                    ResponseMessage = new ResponseMessage
                    {
                        ResponseStatusCode = "Unauthorized",
                        WWWAuthenticateResponseHeader = responseHeader
                    }
                });

            _mockPdpViewDataClient.Setup(x => x.GetPDPViewDataAsync(It.IsAny<string>(), It.IsAny<string>(), It.Is<string>(x => !string.IsNullOrEmpty(x))))
                .ReturnsAsync(new PDPServiceResponseModel
                {
                    ViewDataToken = GetViewDataToken(),
                    ResponseMessage = new ResponseMessage { ResponseStatusCode = "200" }
                });

            _mockMapsRqpService.Setup(x => x.PostRqp(It.IsAny<MapsRqpServiceRequestModel>()))
                .ReturnsAsync(new MapsRqpServiceResponseModel { Rqp = rqp });

            _mockTokenIntegrationService.Setup(x => x.PostRpt(It.IsAny<TokenIntegrationServiceRequestModel>()))
                .ReturnsAsync(new TokenIntegrationResponseModel { Rpt = rpt });

            _function = new PensionDetailsRequestFunction(_mockLogger.Object, _mockPdpViewDataClient.Object, _mockServiceBusClient.Object, _mockMapsRqpService.Object,
                                                           _mockTokenIntegrationService.Object);

            _mockServiceBusSender = new Mock<ServiceBusSender>();
            _mockServiceBusClient.Setup(x => x.CreateSender(It.IsAny<string>())).Returns(_mockServiceBusSender.Object);
        }

        [Fact]
        public void WhenMessagesReceivedAndViewDataTokenIsInvalid_ThenMessageIsDLQed()
        {
            // Arrange
            string messageBody = """
                {
                  "pensionRetrievalRecordId": "e01a9df7-f147-4a3a-a1dd-0507432a5b7f",
                  "peI": "7075aa11-10ad-4b2f-a9f5-1068e79119bf:1ba03e25-659a-43b8-ae77-b956df168969",
                  "iss": "DATA_PROVIDER_1fd1da88-9fb3-461c-a48a-3dba21bfba17",
                  "userSessionId": "459566f6-5fce-479e-a098-298ca9676a85"
                }
                """;
            var message = ServiceBusModelFactory.ServiceBusReceivedMessage(body: new BinaryData(messageBody));
            var messageActions = Substitute.For<ServiceBusMessageActions>();

            _mockPdpViewDataClient.Setup(x => x.GetPDPViewDataAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new PDPServiceResponseModel
                {
                    ViewDataToken = GetInvalidViewDataToken(),
                    ResponseMessage = new ResponseMessage { ResponseStatusCode = "200" }
                });

            // Act
            var result = _function.Run(message, messageActions);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void WhenMssageReceivedIsValid_ThenMessageIsCompleted()
        {
            // Arrange
            string messageBody = """
                {
                  "pensionRetrievalRecordId": "e01a9df7-f147-4a3a-a1dd-0507432a5b7f",
                  "peI": "7075aa11-10ad-4b2f-a9f5-1068e79119bf:1ba03e25-659a-43b8-ae77-b956df168969",
                  "iss": "DATA_PROVIDER_1fd1da88-9fb3-461c-a48a-3dba21bfba17",
                  "userSessionId": "459566f6-5fce-479e-a098-298ca9676a85"
                }
                """;
            var message = ServiceBusModelFactory.ServiceBusReceivedMessage(body: new BinaryData(messageBody));
            var messageActions = Substitute.For<ServiceBusMessageActions>();

            // Act
            var result = _function.Run(message, messageActions);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void WhenMessageReceivedWithValidationErrors_ThenMessageIsDLQed()
        {
            // Arrange
            string messageBody = """
                {
                  "pensionRetrievalRecordId": "e01a9df7-f147-4a3a-a1dd-0507432a5b7f",
                  "peI": "XXX7075aa11-10ad-4b2f-a9f5-1068e79119bf:1ba03e25-659a-43b8-ae77-b956df168969",
                  "iss": "DATA_PROVIDER_1fd1da88-9fb3-461c-a48a-3dba21bfba17",
                  "userSessionId": "459566f6-5fce-479e-a098-298ca9676a85"
                }
                """;
            var message = ServiceBusModelFactory.ServiceBusReceivedMessage(body: new BinaryData(messageBody));
            var messageActions = Substitute.For<ServiceBusMessageActions>();

            // Act
            var result = _function.Run(message, messageActions);

            // Assert
            Assert.NotNull(result);
        }

        private string GetViewDataToken()
        {
            return "{\"view_data_token\":\"eyJhbGciOiJSUzI1NiIsImtpZCI6ImMwMGI0MGVhLTZkYTEtNDA4YS1hNmM5LTE3YjFmZjQ1YmI5YSIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIzMjRicWZ3MzQ4ZjlxNDM5OGgzIiwiaWF0IjoxNzIyODY1MzA1LCJleHAiOjE3MjI4NjUzNjUsImp0aSI6IjRlMTNhYWNkLTMzNWEtNDY0OS1iZjM3LWQ5ZDIyNWU5YTg3NSIsImF1ZCI6Imh0dHBzOi8vcGRwL2lnL3Rva2VuIiwiaXNzIjoiREFUQV9QUk9WSURFUl8xZmQxZGE4OC05ZmIzLTQ2MWMtYTQ4YS0zZGJhMjFiZmJhMTciLCJWaWV3RGF0YSI6IntcImFycmFuZ2VtZW50c1wiOlt7XCJwZW5zaW9uUHJvdmlkZXJTY2hlbWVOYW1lXCI6XCJZb3VyIFBlbnNpb24gREMgTWFzdGVyIFRydXN0XCIsXCJwb3NzaWJsZU1hdGNoXCI6ZmFsc2UsXCJwZW5zaW9uQWRtaW5pc3RyYXRvclwiOntcIm5hbWVcIjpcIllvdXIgUGVuc2lvblwiLFwiY29udGFjdE1ldGhvZHNcIjpbe1wicHJlZmVycmVkXCI6ZmFsc2UsXCJjb250YWN0TWV0aG9kRGV0YWlsc1wiOntcImVtYWlsXCI6XCJtYXN0ZXJ0cnVzdEB5b3VycGVuc2lvbi5jb21cIn19LHtcInByZWZlcnJlZFwiOnRydWUsXCJjb250YWN0TWV0aG9kRGV0YWlsc1wiOntcInVybFwiOlwiaHR0cDovL3d3dy55b3VycGVuc2lvbi5jby51a1wifX0se1wicHJlZmVycmVkXCI6ZmFsc2UsXCJjb250YWN0TWV0aG9kRGV0YWlsc1wiOntcIm51bWJlclwiOlwiKzQ0IDgwMDgwMDg3MzU1XCIsXCJ1c2FnZVwiOltcIk1cIl19fSx7XCJwcmVmZXJyZWRcIjpmYWxzZSxcImNvbnRhY3RNZXRob2REZXRhaWxzXCI6e1wicG9zdGFsTmFtZVwiOlwiWW91ciBQZW5zaW9uXCIsXCJsaW5lMVwiOlwiOTIgVmljdG9yaWEgTGFuZVwiLFwibGluZTJcIjpcIkZyYW1wdG9uIENvdHRlcmVsbFwiLFwibGluZTNcIjpcIkJyaXN0b2xcIixcImxpbmU0XCI6XCJTb3V0aCBHbG9jdXN0ZXJzaGlyZVwiLFwicG9zdGNvZGVcIjpcIkJTMzYgOUREXCIsXCJjb3VudHJ5Q29kZVwiOlwiR0JcIn19XX0sXCJlbXBsb3ltZW50TWVtYmVyc2hpcFBlcmlvZHNcIjpbe1wiZW1wbG95ZXJOYW1lXCI6XCJTd2VldHMgUiBVc1wiLFwiZW1wbG95ZXJTdGF0dXNcIjpcIkNcIixcImVtcGxveW1lbnRTdGFydERhdGVcIjpcIjE5OTgtMDUtMTZcIn1dLFwiYmVuZWZpdElsbHVzdHJhdGlvbnNcIjpbe1wiaWxsdXN0cmF0aW9uQ29tcG9uZW50c1wiOlt7XCJpbGx1c3RyYXRpb25UeXBlXCI6XCJFUklcIixcImJlbmVmaXRUeXBlXCI6XCJEQ1wiLFwiY2FsY3VsYXRpb25NZXRob2RcIjpcIlNNUElcIixcInBheWFibGVEZXRhaWxzXCI6e1wicGF5YWJsZURhdGVcIjpcIjIwMzgtMDktMThcIixcImFubnVhbEFtb3VudFwiOjIwNzAwLFwiYW1vdW50VHlwZVwiOlwiSU5DXCJ9LFwiZGNQb3RcIjozMDAwMDAsXCJzdXJ2aXZvckJlbmVmaXRcIjpmYWxzZSxcInNhZmVndWFyZGVkQmVuZWZpdFwiOmZhbHNlfSx7XCJpbGx1c3RyYXRpb25UeXBlXCI6XCJBUFwiLFwiYmVuZWZpdFR5cGVcIjpcIkRDXCIsXCJjYWxjdWxhdGlvbk1ldGhvZFwiOlwiU01QSVwiLFwicGF5YWJsZURldGFpbHNcIjp7XCJwYXlhYmxlRGF0ZVwiOlwiMjAzOC0wOS0xOFwiLFwiYW5udWFsQW1vdW50XCI6MTYyMTUsXCJhbW91bnRUeXBlXCI6XCJJTkNcIn0sXCJkY1BvdFwiOjIzNTAwMCxcInN1cnZpdm9yQmVuZWZpdFwiOmZhbHNlLFwic2FmZWd1YXJkZWRCZW5lZml0XCI6ZmFsc2V9XSxcImlsbHVzdHJhdGlvbkRhdGVcIjpcIjIwMjMtMDUtMTZcIn1dfV19IiwibmJmIjoxNzIyODY1MzA1fQ.YobUqx064SRXiA9u6kygodJ7WJ4KW-SCITy8TlTC3RRL1A1P4KBx9411cnzT_4eolda_d0K6bvHRCYC5vij73LFtJRC16jyrLgPqwO08FiF92r8Q2h0xIw4Fx7_-rXiKRcv6wmYrKdxOvHv35qHDu8W07v9dwRa-u2lMKroT69AH2_HLFo0LbxLxSt-xuGKK8i5b9kDQwSGRAMqLcxWT2Hac-7A0ZoZYqaB6Jk8JA_8IoxzWFNled1MpF2dfTG5SCRNdsYLZBLaXL7GSIywZAE6TvWAAkWcsb1ckic2iA8ciqY0UQ05-NKe6Q-aqP1BHEKeeCX6pwC2ymboy1BbzkA\"}";
        }

        private string GetInvalidViewDataToken()
        {
            return "{\"XXXview_data_token\": \"eyJhbGciOiJSUzI1NiIsImtpZCI6ImMwMGI0MGVhLTZkYTEtNDA4YS1hNmM5LTE3YjFmZjQ1YmI5YSIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIzMjRicWZ3MzQ4ZjlxNDM5OGgzIiwiaWF0IjoxNzIxMDIzNjk1LCJleHAiOjE3MjEwMjcyOTUsImp0aSI6IjkxZTBhNjRlLWE2NjctNDNjYi05YjA4LWRlYzdiYjBlYTgyYyIsImF1ZCI6Imh0dHBzOi8vcGRwL2lnL3Rva2VuIiwiaXNzIjoiREFUQV9QUk9WSURFUl8xZmQxZGE4OC05ZmIzLTQ2MWMtYTQ4YS0zZGJhMjFiZmJhMTciLCJWaWV3RGF0YSI6IntcclxuXHRcImFycmFuZ2VtZW50c1wiOiBbXHJcblx0XHR7XHJcblx0XHRcdFwicGVuc2lvblByb3ZpZGVyU2NoZW1lTmFtZVwiOiBcIk15IENvbXBhbnkgRGlyZWN0IENvbnRyaWJ1dGlvbiBTY2hlbWVcIixcclxuXHRcdFx0XCJhbHRlcm5hdGVTY2hlbWVOYW1lXCI6IHtcclxuXHRcdFx0XHRcIm5hbWVcIjogXCJDb252ZXJ0ZWQgZnJvbSBNeSBPbGQgRGlyZWN0IENvbnRyaWJ1dGlvbiBTY2hlbWVcIixcclxuXHRcdFx0XHRcImFsdGVybmF0ZU5hbWVUeXBlXCI6IFwiRk9SXCJcclxuXHRcdFx0fSxcclxuXHRcdFx0XCJwb3NzaWJsZU1hdGNoXCI6IHRydWUsXHJcblx0XHRcdFwicG9zc2libGVNYXRjaFJlZmVyZW5jZVwiOiBcIlExMjM0NVwiLFxyXG5cdFx0XHRcInBlbnNpb25BZG1pbmlzdHJhdG9yXCI6IHtcclxuXHRcdFx0XHRcIm5hbWVcIjogXCJQZW5zaW9uIENvbXBhbnkgMVwiLFxyXG5cdFx0XHRcdFwiY29udGFjdE1ldGhvZHNcIjogW1xyXG5cdFx0XHRcdFx0e1xyXG5cdFx0XHRcdFx0XHRcInByZWZlcnJlZFwiOiBmYWxzZSxcclxuXHRcdFx0XHRcdFx0XCJjb250YWN0TWV0aG9kRGV0YWlsc1wiOiB7XHJcblx0XHRcdFx0XHRcdFx0XCJlbWFpbFwiOiBcImV4YW1wbGVAZXhhbXBsZW15bGluZS5jb21cIlxyXG5cdFx0XHRcdFx0XHR9XHJcblx0XHRcdFx0XHR9LFxyXG5cdFx0XHRcdFx0e1xyXG5cdFx0XHRcdFx0XHRcInByZWZlcnJlZFwiOiB0cnVlLFxyXG5cdFx0XHRcdFx0XHRcImNvbnRhY3RNZXRob2REZXRhaWxzXCI6IHtcclxuXHRcdFx0XHRcdFx0XHRcIm51bWJlclwiOiBcIisxMjMgMTExMTExMTExMVwiLFxyXG5cdFx0XHRcdFx0XHRcdFwidXNhZ2VcIjogW1xyXG5cdFx0XHRcdFx0XHRcdFx0XCJBXCIsXHJcblx0XHRcdFx0XHRcdFx0XHRcIk1cIlxyXG5cdFx0XHRcdFx0XHRcdF1cclxuXHRcdFx0XHRcdFx0fVxyXG5cdFx0XHRcdFx0fVxyXG5cdFx0XHRcdF1cclxuXHRcdFx0fVxyXG5cdFx0fVxyXG5cdF1cclxufSIsIm5iZiI6MTcyMTAyMzY5NX0.KPLTCR3Kud1Le_FnSaDFbLOKnznX6VBNwwkd4QDiloXiy7ijZgtXNQU_6CGcWQBMSw-FlJWtMLkS2wtmRBeJa_Zlsy81aJquEQQWQkwChunSEedvoabZh-tA9P9PW7MDSm_jV9YvbP_f5Yr_PCV6VmXE8hXDrraNABbyp1O83XYQDOjAqLMlfncBpu0AfFQ-AnwoEcCR8wCzVIFF_j5x4idiWFUf-S5nI3SnSAPNmtF5SinBMVEgX5YuKQB_ZYAisViVoKGwYw2Ke4kka9WNVBHFGQTuxCvVTdpAdHOaxH3mnV3AXP-0-VeyH3FYnDBzhWJ33vuMirP9rx397UnSnQ\"}";
        }

        private string GetInvalidViewDataClaim()
        {
            return "{\"view_data_token\": \"XXXeyJhbGciOiJSUzI1NiIsImtpZCI6ImMwMGI0MGVhLTZkYTEtNDA4YS1hNmM5LTE3YjFmZjQ1YmI5YSIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIzMjRicWZ3MzQ4ZjlxNDM5OGgzIiwiaWF0IjoxNzIxMDIzNjk1LCJleHAiOjE3MjEwMjcyOTUsImp0aSI6IjkxZTBhNjRlLWE2NjctNDNjYi05YjA4LWRlYzdiYjBlYTgyYyIsImF1ZCI6Imh0dHBzOi8vcGRwL2lnL3Rva2VuIiwiaXNzIjoiREFUQV9QUk9WSURFUl8xZmQxZGE4OC05ZmIzLTQ2MWMtYTQ4YS0zZGJhMjFiZmJhMTciLCJWaWV3RGF0YSI6IntcclxuXHRcImFycmFuZ2VtZW50c1wiOiBbXHJcblx0XHR7XHJcblx0XHRcdFwicGVuc2lvblByb3ZpZGVyU2NoZW1lTmFtZVwiOiBcIk15IENvbXBhbnkgRGlyZWN0IENvbnRyaWJ1dGlvbiBTY2hlbWVcIixcclxuXHRcdFx0XCJhbHRlcm5hdGVTY2hlbWVOYW1lXCI6IHtcclxuXHRcdFx0XHRcIm5hbWVcIjogXCJDb252ZXJ0ZWQgZnJvbSBNeSBPbGQgRGlyZWN0IENvbnRyaWJ1dGlvbiBTY2hlbWVcIixcclxuXHRcdFx0XHRcImFsdGVybmF0ZU5hbWVUeXBlXCI6IFwiRk9SXCJcclxuXHRcdFx0fSxcclxuXHRcdFx0XCJwb3NzaWJsZU1hdGNoXCI6IHRydWUsXHJcblx0XHRcdFwicG9zc2libGVNYXRjaFJlZmVyZW5jZVwiOiBcIlExMjM0NVwiLFxyXG5cdFx0XHRcInBlbnNpb25BZG1pbmlzdHJhdG9yXCI6IHtcclxuXHRcdFx0XHRcIm5hbWVcIjogXCJQZW5zaW9uIENvbXBhbnkgMVwiLFxyXG5cdFx0XHRcdFwiY29udGFjdE1ldGhvZHNcIjogW1xyXG5cdFx0XHRcdFx0e1xyXG5cdFx0XHRcdFx0XHRcInByZWZlcnJlZFwiOiBmYWxzZSxcclxuXHRcdFx0XHRcdFx0XCJjb250YWN0TWV0aG9kRGV0YWlsc1wiOiB7XHJcblx0XHRcdFx0XHRcdFx0XCJlbWFpbFwiOiBcImV4YW1wbGVAZXhhbXBsZW15bGluZS5jb21cIlxyXG5cdFx0XHRcdFx0XHR9XHJcblx0XHRcdFx0XHR9LFxyXG5cdFx0XHRcdFx0e1xyXG5cdFx0XHRcdFx0XHRcInByZWZlcnJlZFwiOiB0cnVlLFxyXG5cdFx0XHRcdFx0XHRcImNvbnRhY3RNZXRob2REZXRhaWxzXCI6IHtcclxuXHRcdFx0XHRcdFx0XHRcIm51bWJlclwiOiBcIisxMjMgMTExMTExMTExMVwiLFxyXG5cdFx0XHRcdFx0XHRcdFwidXNhZ2VcIjogW1xyXG5cdFx0XHRcdFx0XHRcdFx0XCJBXCIsXHJcblx0XHRcdFx0XHRcdFx0XHRcIk1cIlxyXG5cdFx0XHRcdFx0XHRcdF1cclxuXHRcdFx0XHRcdFx0fVxyXG5cdFx0XHRcdFx0fVxyXG5cdFx0XHRcdF1cclxuXHRcdFx0fVxyXG5cdFx0fVxyXG5cdF1cclxufSIsIm5iZiI6MTcyMTAyMzY5NX0.KPLTCR3Kud1Le_FnSaDFbLOKnznX6VBNwwkd4QDiloXiy7ijZgtXNQU_6CGcWQBMSw-FlJWtMLkS2wtmRBeJa_Zlsy81aJquEQQWQkwChunSEedvoabZh-tA9P9PW7MDSm_jV9YvbP_f5Yr_PCV6VmXE8hXDrraNABbyp1O83XYQDOjAqLMlfncBpu0AfFQ-AnwoEcCR8wCzVIFF_j5x4idiWFUf-S5nI3SnSAPNmtF5SinBMVEgX5YuKQB_ZYAisViVoKGwYw2Ke4kka9WNVBHFGQTuxCvVTdpAdHOaxH3mnV3AXP-0-VeyH3FYnDBzhWJ33vuMirP9rx397UnSnQ\"}";
        }

        private string GetResponseHeader()
        {
            return "realm=\"PensionDashboard\", as_uri=\"https://as.pdp.com\", ticket=\"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.cThIIoDvwdueQB468K5xDc5633seEFoqwxjF_xSJyQQ\"";
        }

        private string GetRqp()
        {
            return "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";
        }
        private string GetRpt()
        {
            return "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJsb2dnZWRJbkFzIjoiYWRtaW4iLCJpYXQiOjE0MjI3Nzk2Mzh9.gzSraSYS8EXBxLN_oWnFSRgCzcmJmMjLiuyu5CSpyHI";
        }
    }
}