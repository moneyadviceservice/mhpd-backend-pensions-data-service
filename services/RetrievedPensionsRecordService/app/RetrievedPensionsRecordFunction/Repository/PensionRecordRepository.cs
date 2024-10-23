using MhpdCommon.Models.Configuration;
using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.Models.MHPDModels;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;

namespace RetrievedPensionsRecordFunction.Repository;

public class PensionRecordRepository(CosmosClient cosmosClient, IOptions<CommonCosmosConfiguration> config, ILogger<PensionRecordRepository> logger) 
    : IPensionRecordRepository
{
    private readonly CosmosClient _cosmosClient = cosmosClient;
    private readonly ILogger<PensionRecordRepository> _logger = logger;
    private readonly CommonCosmosConfiguration _configuration = config.Value;

    public async Task<List<RetrievedPensionRecord>> GetRetrievedRecordsAsync(string pensionsRetrievalRecordId)
    {
        var query = new QueryDefinition("SELECT * FROM c WHERE c.pensionsRetrievalRecordId = @partitionKey")
                .WithParameter("@partitionKey", pensionsRetrievalRecordId);

        var container = _cosmosClient.GetContainer(_configuration.DatabaseId, _configuration.ContainerId);
        var iterator = container.GetItemQueryIterator<RetrievedPensionRecord>(query);

        var response = await iterator.ReadNextAsync();

        return [.. response];
    }

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
            RetrievalResult = payload.RetrievalResult
        };

        Container container = _cosmosClient.GetContainer(_configuration.DatabaseId, _configuration.ContainerId);

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
        var connDetails = $"Accessing Cosmos DB partition: [{_configuration.ContainerPartitionKey}] on " +
            $"container: [{_configuration.ContainerId}] in the database [{_configuration.DatabaseId}]";

        _logger.LogCritical(connDetails);
    }
}
