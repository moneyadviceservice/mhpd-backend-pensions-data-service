using CDAServiceEmulator.CosmosRepository;
using MhpdCommon.Models.Configuration;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using Moq;
using System.Dynamic;

namespace CDAServiceEmulatorUnitTests.CosmosRepositoryTests;

public class CdaPeisEmulatorScenarioModelRepositoryTests
{
    [Fact]
    public async Task GetMaxPartitionKeyAsync_Returns_MaxPartitionKey()
    {
        // Arrange
        var mockCosmosClient = new Mock<CosmosClient>();
        var mockDatabase = new Mock<Database>();
        var mockContainer = new Mock<Container>();

        mockCosmosClient
            .Setup(c => c.GetDatabase(It.IsAny<string>()))
            .Returns(mockDatabase.Object); // Mock the Database

        mockDatabase
            .Setup(d => d.GetContainer(It.IsAny<string>()))
            .Returns(mockContainer.Object); // Mock the Container

        mockCosmosClient.Setup(mock => mock.GetContainer(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(mockContainer.Object);

        // Setup the mockFeedResponse to return a fake item with MaxStartCode
        const int maxCode = 1234;
        dynamic result = new ExpandoObject();
        result.MaxStartCode = maxCode;
        var results = new List<dynamic> { result };

        var mockFeedIterator = new Mock<FeedIterator<dynamic>>();
        var mockFeedResponse = new Mock<FeedResponse<dynamic>>();

        mockFeedResponse.Setup(r => r.GetEnumerator())
            .Returns(results.GetEnumerator());

        // Setup the feed iterator
        mockFeedIterator.SetupSequence(i => i.HasMoreResults)
            .Returns(true)
            .Returns(false);

        mockFeedIterator.Setup(i => i.ReadNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockFeedResponse.Object);

        // Setup the container to return the feed iterator
        mockContainer.Setup(c => c.GetItemQueryIterator<dynamic>(
                It.IsAny<QueryDefinition>(),
                It.IsAny<string>(),
                It.IsAny<QueryRequestOptions>()))
            .Returns(mockFeedIterator.Object);

        var configuration = Options.Create(new CosmosTestHarnessConfiguration
        {
            DatabaseName = "TestDatabase",
            CdaPeisEmulatorScenarioModelContainerName = "TestContainer"
        });

        var repository = new CdaPeisEmulatorScenarioModelRepository(mockCosmosClient.Object, configuration);

        // Act
        var code = await repository.GetMaxScenarioCodeAsync();

        // Assert
        Assert.Equal(maxCode, code);
    }
}