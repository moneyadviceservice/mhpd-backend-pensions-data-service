using MhpdCommon.Models.Configuration;
using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PensionsRetrievalFunction.HttpClients;
using PensionsRetrievalFunction.Models;
using PensionsRetrievalFunction.Repository;
using Polly;

namespace PensionsRetrievalFunction.Orchestration;

public class PeiIntegrationOrchestrator(IOptions<CommonServiceBusConfiguration> sbOptions, 
    IOptions<PeiOrchestrationSettings> peiOptions,
    IMessagingService messagingService, 
    IPeiServiceClient client, 
    IPensionRetrievalRepository repository,
    ILogger<PeiIntegrationOrchestrator> logger) : IPeiIntegrationOrchestrator
{
    private readonly CommonServiceBusConfiguration _serviceBusConfiguration = sbOptions.Value;
    private readonly PeiOrchestrationSettings _settings = peiOptions.Value;
    private readonly IMessagingService _messagingService = messagingService;
    private readonly IPensionRetrievalRepository _repository = repository;
    private readonly IPeiServiceClient _client = client;
    private readonly ILogger<PeiIntegrationOrchestrator> _logger = logger;

    public async Task RunAsync(PensionRetrievalPayload payload, string correlationId)
    {
        var record = await _repository.CreateRecordIfNotExistsAsync(payload);
        if (record == null)
        {
            _logger.LogInformation("Pension retrieval record already exists for session: {session}. Skipping further processing...", payload.UserSessionId);
            return;
        }

        var peiResponse = new PeiDataResponse(null, []);

        var retryCondition = new Func<PeiDataResponse, bool>(response =>
        {
            //we want to keep trying until we reach the specified timeout
            return true;
        });

        var retryPolicy = Policy
            .HandleResult(retryCondition)
            .WaitAndRetryAsync(
                retryCount: _settings.RetryLimit,
                sleepDurationProvider: retryAttempt =>
                {
                    return TimeSpan.FromSeconds(_settings.PeiRetryInterval);
                },
                onRetry: (outcome, lapse, attemptCount, context) =>
                {
                    _logger.LogInformation("Attempt #{attemptCount} to fetch PEI data for user session {sessionId}", attemptCount, payload.UserSessionId);
                }
            );

        try
        {
            await retryPolicy.ExecuteAsync(async () =>
            {
                var response = await _client.GetPeiDataAsync(peiResponse.Rpt, payload.Iss, payload.PeisId, payload.UserSessionId);
                peiResponse.SetRpt(response.Rpt);

                foreach (var pei in response.PeiData)
                {
                    pei.RetrievalStatus = Constants.RetrievalStatus.Requested;
                    pei.RetrievalRequestedTimestamp = DateTime.UtcNow;

                    if (peiResponse.TryAdd(pei))
                    {
                        var message = CreateRequestPayload(pei, record);
                        _logger.LogInformation("Pension details request sent for PEI {pei} with retrieval Id {id}"
                            , message.Pei, message.PensionRetrievalRecordId);
                        await _messagingService.SendMessageAsync(message, _serviceBusConfiguration.OutboundQueue!, correlationId);

                        record.PeiData.Add(pei);
                        await _repository.UpdatePensionsRetrievalRecordAsync(record);
                    }
                }
                return response;
            });

            record.PeisRpt = peiResponse.Rpt;
            record.PeiRetrievalComplete = true;
            await _repository.UpdatePensionsRetrievalRecordAsync(record);
        }
        catch (Exception error)
        {
            _logger.LogError(error, "Error retrieving PEI data for Id {PeisId}", payload.PeisId);
        }
    }

    private static PensionRequestPayload CreateRequestPayload(PeiData pei, PensionsRetrievalRecord record)
    {
        return new PensionRequestPayload
        {
            Iss = record.Iss,
            Pei = pei.Pei,
            PensionRetrievalRecordId = record.Id,
            UserSessionId = record.UserSessionId
        };
    }
}
