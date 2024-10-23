using MhpdCommon.Models.Configuration;
using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.Models.MHPDModels;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using RetrievedPensionsRecordFunction.Repository;
using System.Net;

namespace RetrievedPensionsRecordFunctionTests;

public  class PensionRecordRepositoryTests
{
    private readonly Mock<ItemResponse<RetrievedPensionRecord>> _writeResponse;
    private readonly Mock<FeedResponse<RetrievedPensionRecord>> _readResponse;
    private readonly PensionRecordRepository _repository;

    public PensionRecordRepositoryTests()
    {
        var configuration = new CommonCosmosConfiguration
        {
            DatabaseId = "PensionDatabase",
            ContainerId = "PensionContainer",
            ContainerPartitionKey = "PensionPartition"
        };

        var container = new Mock<Container>();
        var client = new Mock<CosmosClient>();
        var iterator = new Mock<FeedIterator<RetrievedPensionRecord>>();
        var loggerMock = new Mock<ILogger<PensionRecordRepository>>();

        _writeResponse = new Mock<ItemResponse<RetrievedPensionRecord>>();
        _readResponse = new Mock<FeedResponse<RetrievedPensionRecord>>();

        iterator.Setup(mock => mock.ReadNextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(_readResponse.Object);

        client.Setup(mock => mock.GetContainer(configuration.DatabaseId, configuration.ContainerId))
            .Returns(container.Object);

        container.Setup(mock => mock.UpsertItemAsync(
            It.IsAny<RetrievedPensionRecord>(), It.IsAny<PartitionKey>(), null, default))
            .Returns(Task.FromResult(_writeResponse.Object));
        container.Setup(mock => mock.GetItemQueryIterator<RetrievedPensionRecord>(It.IsAny<QueryDefinition>(),
            It.IsAny<string>(), It.IsAny<QueryRequestOptions>())).Returns(iterator.Object);

        var options = Options.Create(configuration);
        _repository = new PensionRecordRepository(client.Object, options, loggerMock.Object);
    }

    [Fact]
    private async Task WhenNewPayloadIsProvided_NewRecordIsSaved()
    {
        //Arrange
        var payload = GetPayload();
        _writeResponse.Setup(r => r.StatusCode).Returns(HttpStatusCode.Created);

        //Act
        var result = await _repository.SaveRetrievedPensionRecordAsync("CorrelationId", payload);

        //Assert
        Assert.True(result);
    }

    [Fact]
    private async Task WhenNoCorrelationIdIsProvided_NewRecordIsNotSaved()
    {
        //Arrange
        var payload = GetPayload();

        //Act
        var result = await _repository.SaveRetrievedPensionRecordAsync("            ", payload);

        //Assert
        Assert.False(result);
    }

    [Fact]
    private async Task WhenExistingPayloadIsProvided_RecordIsUpdated()
    {
        //Arrange
        var payload = GetPayload();
        _writeResponse.Setup(r => r.StatusCode).Returns(HttpStatusCode.OK);

        //Act
        var result = await _repository.SaveRetrievedPensionRecordAsync("CorrelationId", payload);

        //Assert
        Assert.True(result);
    }

    [Fact]
    private async Task WhenClientDoesNotSave_ResponseReturnsFalse()
    {
        //Arrange
        var payload = GetPayload();
        _writeResponse.Setup(r => r.StatusCode).Returns(HttpStatusCode.BadRequest);

        //Act
        var result = await _repository.SaveRetrievedPensionRecordAsync("CorrelationId", payload);

        //Assert
        Assert.False(result);
    }

    [Fact]
    public async Task WhenRecordIsRequested_DatabaseResultIsCorrect()
    {
        //Arrange
        List<RetrievedPensionRecord> records = [
            new RetrievedPensionRecord(),
            new RetrievedPensionRecord()
        ];

        _readResponse.Setup(mock => mock.GetEnumerator()).Returns(records.GetEnumerator);

        //Act
        var result = await _repository.GetRetrievedRecordsAsync(Guid.NewGuid().ToString());

        //Assert
        Assert.Equal(2, result.Count);
    }

    private static RetrievedPensionDetailsPayload GetPayload()
    {
        return new RetrievedPensionDetailsPayload
        {
            Pei = "pei",
            PensionRetrievalRecordId = "recordId",
            RetrievalResult = Array.Empty<List<PensionArrangement>>()
        };
    }
}
