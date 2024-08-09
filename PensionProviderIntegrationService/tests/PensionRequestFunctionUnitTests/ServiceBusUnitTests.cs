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

    public class ServiceBusUnitTests
    {
        private readonly Mock<ServiceBusClient> _mockClient = new();
        private readonly Mock<ServiceBusSender> _mockSender = new();
        private readonly ServiceBusClient _client;

        private readonly Mock<IPDPViewDataClient> _mockPdpViewDataClient = new();
        private readonly Mock<IMapsRqpServiceClient> _mockMapsRqpService = new();
        private readonly Mock<ITokenIntegrationServiceClient> _mockTokenIntegrationService = new();


        private readonly Mock<ILogger<PensionDetailsRequestFunction>> _mockLogger;
        PensionDetailsRequestFunction? _function;

        public ServiceBusUnitTests()
        {
            Environment.SetEnvironmentVariable("PdpViewDataUrl", "https://pdpviewdataservicedemulator.azurewebsites.net/view-data/");

            var responseHeader = GetResponseHeader();
            var rqp = GetRqp();
            var rpt = GetRpt();

            _mockClient
                .Setup(c => c.CreateSender(It.IsAny<string>()))
                .Returns(_mockSender.Object);

            _mockSender
            .Setup(sender => sender.SendMessageAsync(
                It.IsAny<ServiceBusMessage>(),
                It.IsAny<CancellationToken>()))
             .Returns(Task.CompletedTask);

            _client = _mockClient.Object;

            _mockPdpViewDataClient = new Mock<IPDPViewDataClient>();
            _mockLogger = new Mock<ILogger<PensionDetailsRequestFunction>>();

            _mockPdpViewDataClient.Setup(x => x.GetPDPViewDataAsync(It.IsAny<string>(), It.IsAny<string>(), It.Is<string>(x => string.IsNullOrEmpty(x))))
                .ReturnsAsync(new PDPServiceResponseModel { ViewDataToken = null, 
                                                            ResponseMessage = new ResponseMessage { ResponseStatusCode = "Unauthorized", 
                                                                WWWAuthenticateResponseHeader = responseHeader } } );

            _mockPdpViewDataClient.Setup(x => x.GetPDPViewDataAsync(It.IsAny<string>(), It.IsAny<string>(), It.Is<string>(x => !string.IsNullOrEmpty(x))))
                .ReturnsAsync(new PDPServiceResponseModel
                {
                    ViewDataToken = GetViewDataToken(),
                    ResponseMessage = new ResponseMessage { ResponseStatusCode = "200" }
                });

            _mockMapsRqpService.Setup(x => x.PostRqp(It.IsAny<MapsRqpServiceRequestModel>()))
                .ReturnsAsync(new MapsRqpServiceResponseModel { Rqp = rqp });

            _mockTokenIntegrationService.Setup(x => x.PostRpt(It.IsAny<TokenIntegrationServiceRequestModel>()))
                .ReturnsAsync(new TokenIntegrationResponseModel {  Rpt = rpt });

             _function = new PensionDetailsRequestFunction(_mockLogger.Object, 
                                                           _mockPdpViewDataClient.Object, 
                                                           _client, 
                                                           _mockMapsRqpService.Object, 
                                                           _mockTokenIntegrationService.Object);
        }

        [Fact]
        public void WhenMessageIsReceivedAndNoValidationErrors_ThenMessageIsCompleted()
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
            var result = _function!.Run(message, messageActions!);

            _mockSender
            .Verify(sender => sender.SendMessageAsync(
                It.IsAny<ServiceBusMessage>(),
                It.IsAny<CancellationToken>()));
        }

        private string GetViewDataToken()
        {
            return "{\"view_data_token\": \"eyJhbGciOiJSUzI1NiIsImtpZCI6ImMwMGI0MGVhLTZkYTEtNDA4YS1hNmM5LTE3YjFmZjQ1YmI5YSIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIzMjRicWZ3MzQ4ZjlxNDM5OGgzIiwiaWF0IjoxNzIzMTI5NTM4LCJleHAiOjE3MjMxMjk1OTgsImp0aSI6IjA0OTczYTFkLTQ1ZGMtNGIxYy1iYTE3LTJjM2NiYmNiNGYwMSIsImF1ZCI6Imh0dHBzOi8vcGRwL2lnL3Rva2VuIiwiaXNzIjoiREFUQV9QUk9WSURFUl8xZmQxZGE4OC05ZmIzLTQ2MWMtYTQ4YS0zZGJhMjFiZmJhMTciLCJ2aWV3X2RhdGEiOiJ7XCJhcnJhbmdlbWVudHNcIjpbe1wicGVuc2lvblByb3ZpZGVyU2NoZW1lTmFtZVwiOlwiU3RhdGUgUGVuc2lvblwiLFwicG9zc2libGVNYXRjaFwiOmZhbHNlLFwicGVuc2lvblR5cGVcIjpcIlNQXCIsXCJzdGF0ZVBlbnNpb25EYXRlXCI6XCIyMDQyLTAyLTIzXCIsXCJwZW5zaW9uQWRtaW5pc3RyYXRvclwiOntcIm5hbWVcIjpcIkRXUFwiLFwiY29udGFjdE1ldGhvZHNcIjpbe1wicHJlZmVycmVkXCI6ZmFsc2UsXCJjb250YWN0TWV0aG9kRGV0YWlsc1wiOntcInBvc3RhbE5hbWVcIjpcIkZyZWVwb3N0IERXUFwiLFwibGluZTFcIjpcIlBlbnNpb25zIFNlcnZpY2UgM1wiLFwiY291bnRyeUNvZGVcIjpcIkdCXCJ9fSx7XCJwcmVmZXJyZWRcIjp0cnVlLFwiY29udGFjdE1ldGhvZERldGFpbHNcIjp7XCJ1cmxcIjpcImh0dHBzOi8vd3d3Lmdvdi51ay9mdXR1cmUtcGVuc2lvbi1jZW50cmVcIn19LHtcInByZWZlcnJlZFwiOmZhbHNlLFwiY29udGFjdE1ldGhvZERldGFpbHNcIjp7XCJudW1iZXJcIjpcIis0NCA4MDA3MzEwMTc1XCIsXCJ1c2FnZVwiOltcIk1cIixcIldcIl19fSx7XCJwcmVmZXJyZWRcIjpmYWxzZSxcImNvbnRhY3RNZXRob2REZXRhaWxzXCI6e1wibnVtYmVyXCI6XCIrNDQgODAwNzMxMDE3NlwiLFwidXNhZ2VcIjpbXCJNXCJdfX0se1wicHJlZmVycmVkXCI6ZmFsc2UsXCJjb250YWN0TWV0aG9kRGV0YWlsc1wiOntcIm51bWJlclwiOlwiKzQ0IDgwMDczMTA0NTZcIixcInVzYWdlXCI6W1wiV1wiXX19LHtcInByZWZlcnJlZFwiOmZhbHNlLFwiY29udGFjdE1ldGhvZERldGFpbHNcIjp7XCJudW1iZXJcIjpcIis0NCAxOTEyMTgyMDUxXCIsXCJ1c2FnZVwiOltcIk5cIl19fSx7XCJwcmVmZXJyZWRcIjpmYWxzZSxcImNvbnRhY3RNZXRob2REZXRhaWxzXCI6e1wibnVtYmVyXCI6XCIrNDQgMTkxMjE4MzYwMFwiLFwidXNhZ2VcIjpbXCJOXCJdfX1dfSxcImJlbmVmaXRJbGx1c3RyYXRpb25zXCI6W3tcImlsbHVzdHJhdGlvbkNvbXBvbmVudHNcIjpbe1wiaWxsdXN0cmF0aW9uVHlwZVwiOlwiRVJJXCIsXCJjYWxjdWxhdGlvbk1ldGhvZFwiOlwiQlNcIixcInBheWFibGVEZXRhaWxzXCI6e1wicGF5YWJsZURhdGVcIjpcIjIwNDItMDItMjNcIixcImFubnVhbEFtb3VudFwiOjExNTAyLFwibW9udGhseUFtb3VudFwiOjk1OC41LFwiYW1vdW50VHlwZVwiOlwiSU5DXCIsXCJpbmNyZWFzaW5nXCI6dHJ1ZX19LHtcImlsbHVzdHJhdGlvblR5cGVcIjpcIkFQXCIsXCJjYWxjdWxhdGlvbk1ldGhvZFwiOlwiQlNcIixcInBheWFibGVEZXRhaWxzXCI6e1wicGF5YWJsZURhdGVcIjpcIjIwNDItMDItMjNcIixcImFubnVhbEFtb3VudFwiOjExNTAyLFwibW9udGhseUFtb3VudFwiOjk1OC41LFwiYW1vdW50VHlwZVwiOlwiSU5DXCIsXCJpbmNyZWFzaW5nXCI6dHJ1ZX19XSxcImlsbHVzdHJhdGlvbkRhdGVcIjpcIjIwMjQtMDgtMjRcIn1dLFwiYWRkaXRpb25hbERhdGFTb3VyY2VzXCI6W3tcInVybFwiOlwiaHR0cHM6Ly93d3cuZ292LnVrL2NoZWNrLXN0YXRlLXBlbnNpb25cIixcImluZm9ybWF0aW9uVHlwZVwiOlwiU1BcIn1dLFwic3RhdGVQZW5zaW9uTWVzc2FnZUVuZ1wiOlwiU3RhdGUgcGVuc2lvbiBtZXNzYWdlIGluIEVuZ2xpc2guXCIsXCJzdGF0ZVBlbnNpb25NZXNzYWdlV2Vsc2hcIjpcIk5lZ2VzIHBlbnNpd24gZ3dsYWRvbCB5biBTYWVzbmVnLlwifV19IiwibmJmIjoxNzIzMTI5NTM4fQ.svwxVmzgcRMPEL-gJnv8tFu-FxwiPJuhwcUKgF4MS_9ExKPR_NzkFPsKY_5NFzG2H83Dr6Njfy9WuMYkOoN139SfL8yKwjlfotRQeSfieZzaItX15hYKtJFnKmhPle2AAFGSUcdwjwtBvlssbJQkFswypWXDoUMinKsqBRaU8YbxKSWZQjboZy-2FYk7ORAP2oqaAVS9RgSmGv_hoZmL3kYF7ZjzEYzLq4rJ6gHISOjxL2s_tDX7Q9RlZudrG_rCWCTfuYoc_-IE5ucVvlfr35eKZDRM1pHUo45EZKS1cps-4u7QWA_qrtjc9XNG2N-xiIaZY4epNKTb8o7LWkn9_A\"}";
        }

        private string GetResponseHeader()
        {
            return "realm=\"PensionDashboard\", as_uri=\"https://as.pdp.com\", ticket=\"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.cThIIoDvwdueQB468K5xDc5633seEFoqwxjF_xSJyQQ\"";
        }

        private string GetRqp()
        {
            return "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";
        }

        private string GetRpt ()
        {
            return "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJsb2dnZWRJbkFzIjoiYWRtaW4iLCJpYXQiOjE0MjI3Nzk2Mzh9.gzSraSYS8EXBxLN_oWnFSRgCzcmJmMjLiuyu5CSpyHI";
        }
    }
}
