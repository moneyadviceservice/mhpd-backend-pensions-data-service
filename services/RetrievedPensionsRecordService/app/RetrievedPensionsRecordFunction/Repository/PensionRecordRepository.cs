using MhpdCommon.Models.MessageBodyModels;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using RetrievedPensionsRecordFunction.Models;
using RetrievedPensionsRecordFunction.Models.Configuration;
using System.Net;

namespace RetrievedPensionsRecordFunction.Repository;

public class PensionRecordRepository(CosmosClient cosmosClient, IOptions<MhpdCosmosConfiguration> config) : IPensionRecordRepository
{
    private readonly CosmosClient _cosmosClient = cosmosClient;
    private readonly MhpdCosmosConfiguration _mhpdConfiguration = config.Value;

    public async Task<bool> SaveRetrievedPensionRecordAsync(string? correlationId, RetrievedPensionDetailsPayload payload)
    {
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

        var response = await container.UpsertItemAsync(record, 
            new PartitionKey(_mhpdConfiguration.ContainerPartitionKey), null, default);

        return response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.Created;
    }
}
