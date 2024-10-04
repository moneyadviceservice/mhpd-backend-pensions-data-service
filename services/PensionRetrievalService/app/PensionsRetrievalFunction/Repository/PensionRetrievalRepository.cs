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
    private readonly Container _container = client.GetContainer(options.Value.DatabaseId, options.Value.ContainerId);

    public async Task<PensionsRetrievalRecord?> CreateRecordIfNotExistsAsync(PensionRetrievalPayload payload)
    {
        var response = await GetMatchingRecordsAsync(payload.UserSessionId!);

        if(response.Count == 0)
        {
            var record = CreateRecord(payload);

            var writeResponse = await _container.CreateItemAsync(
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
        await container.ReplaceItemAsync(record, record.Id, new PartitionKey(record.UserSessionId), null, default);
    }

    public async Task<PensionsRetrievalRecord?> GetRetrievalRecordAsync(string userSessionId)
    {
        var response = await GetMatchingRecordsAsync(userSessionId);
        return response.SingleOrDefault();
    }

    private async Task<FeedResponse<PensionsRetrievalRecord>> GetMatchingRecordsAsync(string userSessionId)
    {
        var query = new QueryDefinition("SELECT TOP 1 * FROM c WHERE c.userSessionId = @partitionKey")
                .WithParameter("@partitionKey", userSessionId);

        var container = _client.GetContainer(_configuration.DatabaseId, _configuration.ContainerId);
        var iterator = container.GetItemQueryIterator<PensionsRetrievalRecord>(query);

        return await iterator.ReadNextAsync();
    }

    private static PensionsRetrievalRecord CreateRecord(PensionRetrievalPayload payload)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(payload.UserSessionId);
        ArgumentException.ThrowIfNullOrWhiteSpace(payload.Iss);
        ArgumentException.ThrowIfNullOrWhiteSpace(payload.PeisId);

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
