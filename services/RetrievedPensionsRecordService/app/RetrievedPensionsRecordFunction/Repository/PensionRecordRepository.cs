using MhpdCommon.Models.MessageBodyModels;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RetrievedPensionsRecordFunction.Models;
using RetrievedPensionsRecordFunction.Models.Configuration;
using System.Net;

namespace RetrievedPensionsRecordFunction.Repository;

public class PensionRecordRepository(CosmosClient cosmosClient, IOptions<MhpdCosmosConfiguration> config, ILogger<PensionRecordRepository> logger) 
    : IPensionRecordRepository
{
    private readonly CosmosClient _cosmosClient = cosmosClient;
    private readonly ILogger<PensionRecordRepository> _logger = logger;
    private readonly MhpdCosmosConfiguration _mhpdConfiguration = config.Value;

    public async Task<bool> SaveRetrievedPensionRecordAsync(string? correlationId, RetrievedPensionDetailsPayload payload)
    {
        LogDatabaseInfo();
        if(string.IsNullOrWhiteSpace(correlationId)) return false;

        var record = new RetrievedPensionRecord
        {
            Id = Guid.NewGuid().ToString(),
            CorrelationId = correlationId,
            Pei = payload.Pei,
            PensionsRetrievalRecordId = payload.PensionRetrievalRecordId,
            RetrievalResult = new RetrievalResult
            {
                PensionArrangements = payload.PensionArrangements
            }
        };

        Database database = _cosmosClient.GetDatabase(_mhpdConfiguration.DatabaseId);

        Container container = database.GetContainer(_mhpdConfiguration.ContainerId);

        var response = await container.UpsertItemAsync(record, new PartitionKey(record.PensionsRetrievalRecordId), null, default);

        string? logMessage;

        if (response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.Created)
        {
            logMessage = $"Retrieved pension record for PEI: {payload.Pei} " +
                $"{(response.StatusCode == HttpStatusCode.Created ? "created" : "updated")}.";

            _logger.LogInformation(logMessage);
            return true;
        }

        logMessage = $"Unable to save a record for pension with PEI: {payload.Pei}";
        _logger.LogCritical(logMessage);
        return false;
    }

    private void LogDatabaseInfo()
    {
        var connDetails = $"Accessing Cosmos DB partition: [{_mhpdConfiguration.ContainerPartitionKey}] on " +
            $"container: [{_mhpdConfiguration.ContainerId}] in the database [{_mhpdConfiguration.DatabaseId}]";

        _logger.LogCritical(connDetails);
    }
}
