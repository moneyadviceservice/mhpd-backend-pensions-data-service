using MhpdCommon.Models.Configuration;
using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using PensionsRetrievalFunction.HttpClients;
using PensionsRetrievalFunction.Models;
using PensionsRetrievalFunction.Orchestration;
using PensionsRetrievalFunction.Repository;

namespace PensionsRetrievalFunctionTests;

public class PeiIntegrationOrchestratorTests
{
    private readonly Mock<ILogger<PeiIntegrationOrchestrator>> _logger;
    private readonly Mock<IMessagingService> _messagingService;
    private readonly Mock<IPensionRetrievalRepository> _repository;
    private const string InboundQueue = "data-in";
    private const string OutboundQueue = "data-out";

    public PeiIntegrationOrchestratorTests()
    {
        _logger = new Mock<ILogger<PeiIntegrationOrchestrator>>();
        _messagingService = new Mock<IMessagingService>();
        _messagingService.Setup(mock => mock.SendMessageAsync(It.IsAny<PensionRequestPayload>(), OutboundQueue, It.IsAny<string>())).Verifiable();

        _repository = new Mock<IPensionRetrievalRepository>();
    }

    [Theory]
    [InlineData(4, 5, 3, 1, 2)]
    [InlineData(7, 10, 6, 3, 4)]
    [InlineData(9, 15, 8, 4, 5)]
    [InlineData(12, 20, 11, 5, 6)]
    public async Task WhenHttpClientIsExecutedWithRetryConfiguration_EndpointIsCalledAsExpected(
        int callsToSimulate, int timeout, int expectedClientCallCount, int expectedMessagingCallCount, int expectedSaveCount)
    {
        //Arrange
        var apiConfiguration = new PeiOrchestrationSettings
        {
            PeiRetryInterval = 2,
            PeiRetryTimeout = timeout
        };

        var sbConfiguration = new CommonServiceBusConfiguration
        {
            InboundQueue = InboundQueue,
            OutboundQueue = OutboundQueue
        };

        var client = CreateHttpClientWithRetry(callsToSimulate);

        var apiOptions = Options.Create(apiConfiguration);
        var sbOptions = Options.Create(sbConfiguration);

        var payload = new PensionRetrievalPayload
        {
            Iss = "Test ISS",
            PeisId = Guid.NewGuid().ToString(),
            UserSessionId = Guid.NewGuid().ToString()
        };

        var record = new PensionsRetrievalRecord
        {
            Id = Guid.NewGuid().ToString(),
            Iss = payload.Iss,
            PeisId = payload.PeisId,
            UserSessionId = payload.UserSessionId
        };

        _repository.Setup(mock => mock.CreateRecordIfNotExistsAsync(It.IsAny<PensionRetrievalPayload>())).ReturnsAsync(record);
        _repository.Setup(mock => mock.UpdatePensionsRetrievalRecordAsync(It.IsAny<PensionsRetrievalRecord>())).Verifiable();

        var orchestrator = new PeiIntegrationOrchestrator(sbOptions, apiOptions, _messagingService.Object,
            client.Object, _repository.Object, _logger.Object);

        var correlationId = Guid.NewGuid().ToString();

        //Act
        await orchestrator.RunAsync(payload, correlationId);

        //Assert
        client.Verify(mock => mock.GetPeiDataAsync(It.IsAny<string>(), payload.Iss,
            payload.PeisId, payload.UserSessionId), Times.Exactly(expectedClientCallCount));

        _messagingService.Verify(mock => mock.SendMessageAsync(It.IsAny<PensionRequestPayload>(), OutboundQueue, correlationId),
            Times.Exactly(expectedMessagingCallCount));

        _repository.Verify(mock => mock.UpdatePensionsRetrievalRecordAsync(It.IsAny<PensionsRetrievalRecord>()), Times.Exactly(expectedSaveCount));
    }


    private static Mock<IPeiServiceClient> CreateHttpClientWithRetry(int simulationAttempts)
    {
        var httpClientMock = new Mock<IPeiServiceClient>();

        var attempts = 1;

        var sequence = httpClientMock
            .SetupSequence(mock => mock.GetPeiDataAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));

        while (simulationAttempts > attempts)
        {
            sequence = sequence.ReturnsAsync(CreateResponse(attempts % 2 == 0));
            attempts++;
        }

        return httpClientMock;
    }

    private static PeiDataResponse CreateResponse(bool withData)
    {
        var response = new List<PeiData>
        {
            new() {
                Description = "Test",
                Pei = Guid.NewGuid().ToString(),
                RetrievalRequestedTimestamp = DateTime.UtcNow,
                RetrievalStatus = "Started"
            }
        };
        return new PeiDataResponse("rpt", withData ? response : []);
    }
}
