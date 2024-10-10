using CDAServiceEmulator.CosmosRepository;
using CDAServiceEmulator.Models.HolderConfiguration;
using CDAServiceEmulatorUnitTests.Mock;
using Microsoft.Azure.Cosmos;
using Moq;

namespace CDAServiceEmulatorUnitTests;

public class HolderNameViewDataRepositoryTests
{
    private readonly HolderNameViewDataRepository _repository;

    public HolderNameViewDataRepositoryTests()
    {
        var container = new Mock<Container>();
        var database = new Mock<Database>();
        var client = new Mock<CosmosClient>();
        var iterator = new Mock<FeedIterator<HolderNameConfigurationModel>>();
        var readResponse = new Mock<FeedResponse<HolderNameConfigurationModel>>();

        readResponse.Setup(mock => mock.GetEnumerator()).Returns(HolderConfigurationMock.GetHolderConfiguration().GetEnumerator());

        iterator.Setup(mock => mock.ReadNextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(readResponse.Object);
        database.Setup(mock => mock.GetContainer(It.IsAny<string>())).Returns(container.Object);
        container.Setup(mock => mock.GetItemQueryIterator<HolderNameConfigurationModel>(It.IsAny<QueryDefinition>(),
            It.IsAny<string>(), It.IsAny<QueryRequestOptions>())).Returns(iterator.Object);
        client.Setup(mock => mock.GetContainer(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(container.Object);
        client.Setup(mock => mock.GetDatabase(It.IsAny<string>()))
            .Returns(database.Object);

        _repository = new HolderNameViewDataRepository(client.Object, "database", "container");
    }

    [Fact]
    public async Task WhenHolderNameDataIsRequested_ReturnsAllData()
    {
        //Act
        var result = await _repository.GetHolderNameConfigurationsAsync();

        //Assert
        Assert.Equal(2, result.Count);
    }
}
