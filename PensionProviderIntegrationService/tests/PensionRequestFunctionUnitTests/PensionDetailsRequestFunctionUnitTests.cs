using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Moq;
using NSubstitute;
using PensionRequestFunction;
using PensionRequestFunction.HttpClient;

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

        public PensionDetailsRequestFunctionUnitTests()
        {
            _mockLoggerFactory = new Mock<ILoggerFactory>();
            _mockServiceBusClient = new Mock<ServiceBusClient>();
            _mockPdpViewDataClient = new Mock<IPDPViewDataClient>();
            _mockLogger = new Mock<ILogger<PensionDetailsRequestFunction>>();
            _mockLoggerFactory.Setup(factory => factory.CreateLogger(It.IsAny<string>())).Returns(_mockLogger.Object);

            _mockPdpViewDataClient.Setup(x => x.GetPDPViewDataAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(GetViewDataToken());

            _function = new PensionDetailsRequestFunction(_mockLogger.Object, _mockPdpViewDataClient.Object, _mockServiceBusClient.Object);

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
                .ReturnsAsync(GetInvalidViewDataToken());

            // Act
            var result = _function.Run(message, messageActions);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void WhenMessagesReceivedAndViewDataClaimIsInvalid_ThenMessageIsDLQed()
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
                .ReturnsAsync(GetInvalidViewDataClaim());

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
            return "{\"view_data_token\": \"eyJhbGciOiJSUzI1NiIsImtpZCI6ImMwMGI0MGVhLTZkYTEtNDA4YS1hNmM5LTE3YjFmZjQ1YmI5YSIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIzMjRicWZ3MzQ4ZjlxNDM5OGgzIiwiaWF0IjoxNzIxMDIzNjk1LCJleHAiOjE3MjEwMjcyOTUsImp0aSI6IjkxZTBhNjRlLWE2NjctNDNjYi05YjA4LWRlYzdiYjBlYTgyYyIsImF1ZCI6Imh0dHBzOi8vcGRwL2lnL3Rva2VuIiwiaXNzIjoiREFUQV9QUk9WSURFUl8xZmQxZGE4OC05ZmIzLTQ2MWMtYTQ4YS0zZGJhMjFiZmJhMTciLCJWaWV3RGF0YSI6IntcclxuXHRcImFycmFuZ2VtZW50c1wiOiBbXHJcblx0XHR7XHJcblx0XHRcdFwicGVuc2lvblByb3ZpZGVyU2NoZW1lTmFtZVwiOiBcIk15IENvbXBhbnkgRGlyZWN0IENvbnRyaWJ1dGlvbiBTY2hlbWVcIixcclxuXHRcdFx0XCJhbHRlcm5hdGVTY2hlbWVOYW1lXCI6IHtcclxuXHRcdFx0XHRcIm5hbWVcIjogXCJDb252ZXJ0ZWQgZnJvbSBNeSBPbGQgRGlyZWN0IENvbnRyaWJ1dGlvbiBTY2hlbWVcIixcclxuXHRcdFx0XHRcImFsdGVybmF0ZU5hbWVUeXBlXCI6IFwiRk9SXCJcclxuXHRcdFx0fSxcclxuXHRcdFx0XCJwb3NzaWJsZU1hdGNoXCI6IHRydWUsXHJcblx0XHRcdFwicG9zc2libGVNYXRjaFJlZmVyZW5jZVwiOiBcIlExMjM0NVwiLFxyXG5cdFx0XHRcInBlbnNpb25BZG1pbmlzdHJhdG9yXCI6IHtcclxuXHRcdFx0XHRcIm5hbWVcIjogXCJQZW5zaW9uIENvbXBhbnkgMVwiLFxyXG5cdFx0XHRcdFwiY29udGFjdE1ldGhvZHNcIjogW1xyXG5cdFx0XHRcdFx0e1xyXG5cdFx0XHRcdFx0XHRcInByZWZlcnJlZFwiOiBmYWxzZSxcclxuXHRcdFx0XHRcdFx0XCJjb250YWN0TWV0aG9kRGV0YWlsc1wiOiB7XHJcblx0XHRcdFx0XHRcdFx0XCJlbWFpbFwiOiBcImV4YW1wbGVAZXhhbXBsZW15bGluZS5jb21cIlxyXG5cdFx0XHRcdFx0XHR9XHJcblx0XHRcdFx0XHR9LFxyXG5cdFx0XHRcdFx0e1xyXG5cdFx0XHRcdFx0XHRcInByZWZlcnJlZFwiOiB0cnVlLFxyXG5cdFx0XHRcdFx0XHRcImNvbnRhY3RNZXRob2REZXRhaWxzXCI6IHtcclxuXHRcdFx0XHRcdFx0XHRcIm51bWJlclwiOiBcIisxMjMgMTExMTExMTExMVwiLFxyXG5cdFx0XHRcdFx0XHRcdFwidXNhZ2VcIjogW1xyXG5cdFx0XHRcdFx0XHRcdFx0XCJBXCIsXHJcblx0XHRcdFx0XHRcdFx0XHRcIk1cIlxyXG5cdFx0XHRcdFx0XHRcdF1cclxuXHRcdFx0XHRcdFx0fVxyXG5cdFx0XHRcdFx0fVxyXG5cdFx0XHRcdF1cclxuXHRcdFx0fVxyXG5cdFx0fVxyXG5cdF1cclxufSIsIm5iZiI6MTcyMTAyMzY5NX0.KPLTCR3Kud1Le_FnSaDFbLOKnznX6VBNwwkd4QDiloXiy7ijZgtXNQU_6CGcWQBMSw-FlJWtMLkS2wtmRBeJa_Zlsy81aJquEQQWQkwChunSEedvoabZh-tA9P9PW7MDSm_jV9YvbP_f5Yr_PCV6VmXE8hXDrraNABbyp1O83XYQDOjAqLMlfncBpu0AfFQ-AnwoEcCR8wCzVIFF_j5x4idiWFUf-S5nI3SnSAPNmtF5SinBMVEgX5YuKQB_ZYAisViVoKGwYw2Ke4kka9WNVBHFGQTuxCvVTdpAdHOaxH3mnV3AXP-0-VeyH3FYnDBzhWJ33vuMirP9rx397UnSnQ\"}";
        }

        private string GetInvalidViewDataToken()
        {
            return "{\"XXXview_data_token\": \"eyJhbGciOiJSUzI1NiIsImtpZCI6ImMwMGI0MGVhLTZkYTEtNDA4YS1hNmM5LTE3YjFmZjQ1YmI5YSIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIzMjRicWZ3MzQ4ZjlxNDM5OGgzIiwiaWF0IjoxNzIxMDIzNjk1LCJleHAiOjE3MjEwMjcyOTUsImp0aSI6IjkxZTBhNjRlLWE2NjctNDNjYi05YjA4LWRlYzdiYjBlYTgyYyIsImF1ZCI6Imh0dHBzOi8vcGRwL2lnL3Rva2VuIiwiaXNzIjoiREFUQV9QUk9WSURFUl8xZmQxZGE4OC05ZmIzLTQ2MWMtYTQ4YS0zZGJhMjFiZmJhMTciLCJWaWV3RGF0YSI6IntcclxuXHRcImFycmFuZ2VtZW50c1wiOiBbXHJcblx0XHR7XHJcblx0XHRcdFwicGVuc2lvblByb3ZpZGVyU2NoZW1lTmFtZVwiOiBcIk15IENvbXBhbnkgRGlyZWN0IENvbnRyaWJ1dGlvbiBTY2hlbWVcIixcclxuXHRcdFx0XCJhbHRlcm5hdGVTY2hlbWVOYW1lXCI6IHtcclxuXHRcdFx0XHRcIm5hbWVcIjogXCJDb252ZXJ0ZWQgZnJvbSBNeSBPbGQgRGlyZWN0IENvbnRyaWJ1dGlvbiBTY2hlbWVcIixcclxuXHRcdFx0XHRcImFsdGVybmF0ZU5hbWVUeXBlXCI6IFwiRk9SXCJcclxuXHRcdFx0fSxcclxuXHRcdFx0XCJwb3NzaWJsZU1hdGNoXCI6IHRydWUsXHJcblx0XHRcdFwicG9zc2libGVNYXRjaFJlZmVyZW5jZVwiOiBcIlExMjM0NVwiLFxyXG5cdFx0XHRcInBlbnNpb25BZG1pbmlzdHJhdG9yXCI6IHtcclxuXHRcdFx0XHRcIm5hbWVcIjogXCJQZW5zaW9uIENvbXBhbnkgMVwiLFxyXG5cdFx0XHRcdFwiY29udGFjdE1ldGhvZHNcIjogW1xyXG5cdFx0XHRcdFx0e1xyXG5cdFx0XHRcdFx0XHRcInByZWZlcnJlZFwiOiBmYWxzZSxcclxuXHRcdFx0XHRcdFx0XCJjb250YWN0TWV0aG9kRGV0YWlsc1wiOiB7XHJcblx0XHRcdFx0XHRcdFx0XCJlbWFpbFwiOiBcImV4YW1wbGVAZXhhbXBsZW15bGluZS5jb21cIlxyXG5cdFx0XHRcdFx0XHR9XHJcblx0XHRcdFx0XHR9LFxyXG5cdFx0XHRcdFx0e1xyXG5cdFx0XHRcdFx0XHRcInByZWZlcnJlZFwiOiB0cnVlLFxyXG5cdFx0XHRcdFx0XHRcImNvbnRhY3RNZXRob2REZXRhaWxzXCI6IHtcclxuXHRcdFx0XHRcdFx0XHRcIm51bWJlclwiOiBcIisxMjMgMTExMTExMTExMVwiLFxyXG5cdFx0XHRcdFx0XHRcdFwidXNhZ2VcIjogW1xyXG5cdFx0XHRcdFx0XHRcdFx0XCJBXCIsXHJcblx0XHRcdFx0XHRcdFx0XHRcIk1cIlxyXG5cdFx0XHRcdFx0XHRcdF1cclxuXHRcdFx0XHRcdFx0fVxyXG5cdFx0XHRcdFx0fVxyXG5cdFx0XHRcdF1cclxuXHRcdFx0fVxyXG5cdFx0fVxyXG5cdF1cclxufSIsIm5iZiI6MTcyMTAyMzY5NX0.KPLTCR3Kud1Le_FnSaDFbLOKnznX6VBNwwkd4QDiloXiy7ijZgtXNQU_6CGcWQBMSw-FlJWtMLkS2wtmRBeJa_Zlsy81aJquEQQWQkwChunSEedvoabZh-tA9P9PW7MDSm_jV9YvbP_f5Yr_PCV6VmXE8hXDrraNABbyp1O83XYQDOjAqLMlfncBpu0AfFQ-AnwoEcCR8wCzVIFF_j5x4idiWFUf-S5nI3SnSAPNmtF5SinBMVEgX5YuKQB_ZYAisViVoKGwYw2Ke4kka9WNVBHFGQTuxCvVTdpAdHOaxH3mnV3AXP-0-VeyH3FYnDBzhWJ33vuMirP9rx397UnSnQ\"}";
        }

        private string GetInvalidViewDataClaim()
        {
            return "{\"view_data_token\": \"XXXeyJhbGciOiJSUzI1NiIsImtpZCI6ImMwMGI0MGVhLTZkYTEtNDA4YS1hNmM5LTE3YjFmZjQ1YmI5YSIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIzMjRicWZ3MzQ4ZjlxNDM5OGgzIiwiaWF0IjoxNzIxMDIzNjk1LCJleHAiOjE3MjEwMjcyOTUsImp0aSI6IjkxZTBhNjRlLWE2NjctNDNjYi05YjA4LWRlYzdiYjBlYTgyYyIsImF1ZCI6Imh0dHBzOi8vcGRwL2lnL3Rva2VuIiwiaXNzIjoiREFUQV9QUk9WSURFUl8xZmQxZGE4OC05ZmIzLTQ2MWMtYTQ4YS0zZGJhMjFiZmJhMTciLCJWaWV3RGF0YSI6IntcclxuXHRcImFycmFuZ2VtZW50c1wiOiBbXHJcblx0XHR7XHJcblx0XHRcdFwicGVuc2lvblByb3ZpZGVyU2NoZW1lTmFtZVwiOiBcIk15IENvbXBhbnkgRGlyZWN0IENvbnRyaWJ1dGlvbiBTY2hlbWVcIixcclxuXHRcdFx0XCJhbHRlcm5hdGVTY2hlbWVOYW1lXCI6IHtcclxuXHRcdFx0XHRcIm5hbWVcIjogXCJDb252ZXJ0ZWQgZnJvbSBNeSBPbGQgRGlyZWN0IENvbnRyaWJ1dGlvbiBTY2hlbWVcIixcclxuXHRcdFx0XHRcImFsdGVybmF0ZU5hbWVUeXBlXCI6IFwiRk9SXCJcclxuXHRcdFx0fSxcclxuXHRcdFx0XCJwb3NzaWJsZU1hdGNoXCI6IHRydWUsXHJcblx0XHRcdFwicG9zc2libGVNYXRjaFJlZmVyZW5jZVwiOiBcIlExMjM0NVwiLFxyXG5cdFx0XHRcInBlbnNpb25BZG1pbmlzdHJhdG9yXCI6IHtcclxuXHRcdFx0XHRcIm5hbWVcIjogXCJQZW5zaW9uIENvbXBhbnkgMVwiLFxyXG5cdFx0XHRcdFwiY29udGFjdE1ldGhvZHNcIjogW1xyXG5cdFx0XHRcdFx0e1xyXG5cdFx0XHRcdFx0XHRcInByZWZlcnJlZFwiOiBmYWxzZSxcclxuXHRcdFx0XHRcdFx0XCJjb250YWN0TWV0aG9kRGV0YWlsc1wiOiB7XHJcblx0XHRcdFx0XHRcdFx0XCJlbWFpbFwiOiBcImV4YW1wbGVAZXhhbXBsZW15bGluZS5jb21cIlxyXG5cdFx0XHRcdFx0XHR9XHJcblx0XHRcdFx0XHR9LFxyXG5cdFx0XHRcdFx0e1xyXG5cdFx0XHRcdFx0XHRcInByZWZlcnJlZFwiOiB0cnVlLFxyXG5cdFx0XHRcdFx0XHRcImNvbnRhY3RNZXRob2REZXRhaWxzXCI6IHtcclxuXHRcdFx0XHRcdFx0XHRcIm51bWJlclwiOiBcIisxMjMgMTExMTExMTExMVwiLFxyXG5cdFx0XHRcdFx0XHRcdFwidXNhZ2VcIjogW1xyXG5cdFx0XHRcdFx0XHRcdFx0XCJBXCIsXHJcblx0XHRcdFx0XHRcdFx0XHRcIk1cIlxyXG5cdFx0XHRcdFx0XHRcdF1cclxuXHRcdFx0XHRcdFx0fVxyXG5cdFx0XHRcdFx0fVxyXG5cdFx0XHRcdF1cclxuXHRcdFx0fVxyXG5cdFx0fVxyXG5cdF1cclxufSIsIm5iZiI6MTcyMTAyMzY5NX0.KPLTCR3Kud1Le_FnSaDFbLOKnznX6VBNwwkd4QDiloXiy7ijZgtXNQU_6CGcWQBMSw-FlJWtMLkS2wtmRBeJa_Zlsy81aJquEQQWQkwChunSEedvoabZh-tA9P9PW7MDSm_jV9YvbP_f5Yr_PCV6VmXE8hXDrraNABbyp1O83XYQDOjAqLMlfncBpu0AfFQ-AnwoEcCR8wCzVIFF_j5x4idiWFUf-S5nI3SnSAPNmtF5SinBMVEgX5YuKQB_ZYAisViVoKGwYw2Ke4kka9WNVBHFGQTuxCvVTdpAdHOaxH3mnV3AXP-0-VeyH3FYnDBzhWJ33vuMirP9rx397UnSnQ\"}";
        }
    }
}