using MhpdCommon.Models.Configuration;
using MhpdCommon.Models.MessageBodyModels;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using PensionsRetrievalFunction.Models;

namespace PensionsRetrievalFunction.Repository;

public class PensionRetrievalRepository(IOptions<CommonCosmosConfiguration> options, CosmosClient client) : IPensionRetrievalRepository
{
    private readonly CosmosClient _client = client;
    private readonly CommonCosmosConfiguration _configuration = options.Value;
    public async Task<PensionsRetrievalRecord?> CreateRecordIfNotExistsAsync(PensionRetrievalPayload payload)
    {
        var query = new QueryDefinition("SELECT TOP 1 * FROM c WHERE c.userSessionId = @partitionKey")
                .WithParameter("@partitionKey", payload.UserSessionId);

        var container = _client.GetContainer(_configuration.DatabaseId, _configuration.ContainerId);
        var iterator = container.GetItemQueryIterator<PensionsRetrievalRecord>(query);

        var response = await iterator.ReadNextAsync(default);
        if(response.Count == 0)
        {
            var record = CreateRecord(payload);

            var writeResponse = await container.CreateItemAsync(
                item: record,
                partitionKey: new PartitionKey(record.UserSessionId)
            );

            return writeResponse.Resource;
        }

        return null;
    }

    public async Task UpdatePensionsRetrievalRecordAsync(PensionsRetrievalRecord record)
    {
        var container = _client.GetContainer(_configuration.DatabaseId, _configuration.ContainerId);
        await container.ReplaceItemAsync(record, record.Id);
    }

    private static PensionsRetrievalRecord CreateRecord(PensionRetrievalPayload payload)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(payload.UserSessionId);
        ArgumentNullException.ThrowIfNullOrWhiteSpace(payload.Iss);
        ArgumentNullException.ThrowIfNullOrWhiteSpace(payload.PeisId);

        return new PensionsRetrievalRecord
        {
            Id = Guid.NewGuid().ToString(),
            Iss = payload.Iss,
            UserSessionId = payload.UserSessionId,
            PeisId = payload.PeisId,
            JobStartTimestamp = DateTime.UtcNow
        };
    }
}
