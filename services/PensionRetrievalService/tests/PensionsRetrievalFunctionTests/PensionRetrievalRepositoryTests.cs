using MhpdCommon.Models.Configuration;
using MhpdCommon.Models.MessageBodyModels;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using Moq;
using PensionsRetrievalFunction.Models;
using PensionsRetrievalFunction.Repository;

namespace PensionsRetrievalFunctionTests;

public class PensionRetrievalRepositoryTests
{
    private readonly Mock<Container> _container;
    private readonly PensionRetrievalRepository _repository;
    private readonly Mock<FeedResponse<PensionsRetrievalRecord>> _readResponse;
    private readonly Mock<ItemResponse<PensionsRetrievalRecord>> _writeResponse;

    public PensionRetrievalRepositoryTests()
    {
        var configuration = new CommonCosmosConfiguration
        {
            DatabaseId = "PensionDatabase",
            ContainerId = "PensionContainer",
            ContainerPartitionKey = "PensionPartition"
        };

        var options = Options.Create(configuration);
        var client = new Mock<CosmosClient>();
        var iterator = new Mock<FeedIterator<PensionsRetrievalRecord>>();
        _container = new Mock<Container>();
        _readResponse = new Mock<FeedResponse<PensionsRetrievalRecord>>();
        _writeResponse = new Mock<ItemResponse<PensionsRetrievalRecord>>();

        client.Setup(mock => mock.GetContainer(configuration.DatabaseId, configuration.ContainerId))
            .Returns(_container.Object);

        _container.Setup(mock => mock.GetItemQueryIterator<PensionsRetrievalRecord>(It.IsAny<QueryDefinition>(), 
            It.IsAny<string>(), It.IsAny<QueryRequestOptions>())).Returns(iterator.Object);
        _container.Setup(mock => mock.CreateItemAsync(It.IsAny<PensionsRetrievalRecord>(), It.IsAny<PartitionKey>(),
            It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>())).ReturnsAsync(_writeResponse.Object).Verifiable();

        iterator.Setup(mock => mock.ReadNextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(_readResponse.Object);

        _repository = new PensionRetrievalRepository(options, client.Object);
    }

    [Theory]
    [InlineData(0 , 1, true)]
    [InlineData(1 , 0, true)]
    public async Task WhenIsSaved_ReturnsTrue(int recordsFound, int expectedCalls, bool expectedResult)
    {
        //Arrange
        _container.Invocations.Clear();
        var message = new PensionRetrievalPayload
        {
            UserSessionId = "Id",
            Iss = "iss",
            PeisId = "PeisId"
        };

        _writeResponse.Setup(mock => mock.Resource).Returns(new PensionsRetrievalRecord());
        _readResponse.Setup(mock => mock.Count).Returns(recordsFound);

        //Act
        var result = await _repository.CreateRecordIfNotExistsAsync(message);

        //Assert
        Assert.Equal(expectedResult, result);
        _container.Verify(mock => mock.CreateItemAsync(It.IsAny<PensionsRetrievalRecord>(), It.IsAny<PartitionKey>(),
            It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()), Times.Exactly(expectedCalls));
    }

    [Fact]
    public async Task WhenIsNotSaved_ReturnsFalse()
    {
        //Arrange
        _container.Invocations.Clear();
        var message = new PensionRetrievalPayload
        {
            UserSessionId = "Id",
            Iss = "iss",
            PeisId = "PeisId"
        };

        _readResponse.Setup(mock => mock.Count).Returns(0);

        //Act
        var result = await _repository.CreateRecordIfNotExistsAsync(message);

        //Assert
        Assert.False(result);
    }
}
