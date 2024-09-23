using MhpdCommon.Models.MessageBodyModels;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using RetrievedPensionsRecordFunction.Models;
using RetrievedPensionsRecordFunction.Models.Configuration;
using RetrievedPensionsRecordFunction.Repository;
using System.Net;

namespace RetrievedPensionsRecordFunctionTests;

public  class PensionRecordRepositoryTests
{
    private readonly Mock<ItemResponse<RetrievedPensionRecord>> _response;
    private readonly PensionRecordRepository _repository;

    public PensionRecordRepositoryTests()
    {
        var configuration = new MhpdCosmosConfiguration
        {
            DatabaseId = "PensionDatabase",
            ContainerId = "PensionContainer",
            ContainerPartitionKey = "PensionPartition"
        };

        var container = new Mock<Container>();
        var database = new Mock<Database>();
        var client = new Mock<CosmosClient>();
        _response = new Mock<ItemResponse<RetrievedPensionRecord>>();

        var loggerMock = new Mock<ILogger<PensionRecordRepository>>();

        client.Setup(mock => mock.GetDatabase(configuration.DatabaseId))
            .Returns(database.Object);

        database.Setup(mock => mock.GetContainer(configuration.ContainerId))
            .Returns(container.Object);

        container.Setup(mock => mock.UpsertItemAsync(
            It.IsAny<RetrievedPensionRecord>(), It.IsAny<PartitionKey>(), null, default))
            .Returns(Task.FromResult(_response.Object));

        var options = Options.Create(configuration);
        _repository = new PensionRecordRepository(client.Object, options, loggerMock.Object);
    }

    [Fact]
    private async Task WhenNewPayloadIsProvided_NewRecordIsSaved()
    {
        //Arrange
        var payload = GetPayload();
        _response.Setup(r => r.StatusCode).Returns(HttpStatusCode.Created);

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
        _response.Setup(r => r.StatusCode).Returns(HttpStatusCode.OK);

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
        _response.Setup(r => r.StatusCode).Returns(HttpStatusCode.BadRequest);

        //Act
        var result = await _repository.SaveRetrievedPensionRecordAsync("CorrelationId", payload);

        //Assert
        Assert.False(result);
    }

    private static RetrievedPensionDetailsPayload GetPayload()
    {
        return new RetrievedPensionDetailsPayload
        {
            Pei = "pei",
            PensionRetrievalRecordId = "recordId",
            PensionArrangements =
            [
                new()
            ]
        };
    }
}
