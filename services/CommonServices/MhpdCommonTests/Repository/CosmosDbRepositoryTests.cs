using MhpdCommon.Repository;
using Microsoft.Azure.Cosmos;
using Moq;
using System.Net;

namespace MhpdCommon.Tests.Repository;

public class CosmosDbRepositoryTests
{
    private readonly Mock<Container> _mockContainer;
    private readonly CosmosDbRepository<TestEntity> _repository;

    public CosmosDbRepositoryTests()
    {
        Mock<CosmosClient> mockCosmosClient = new();
        _mockContainer = new Mock<Container>();
        Mock<Database> mockDatabase = new();

        // Setup the mocks
        mockCosmosClient
            .Setup(c => c.GetDatabase(It.IsAny<string>()))
            .Returns(mockDatabase.Object);

        mockDatabase
            .Setup(d => d.GetContainer(It.IsAny<string>()))
            .Returns(_mockContainer.Object);

        // Instantiate the repository with the mocked objects
        _repository = new CosmosDbRepository<TestEntity>(mockCosmosClient.Object, "TestDatabase", "TestContainer");
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsEntity_WhenEntityExists()
    {
        // Arrange
        var testEntity = new TestEntity { Id = "1", PartitionKey = "partition1" };
        var response = new Mock<ItemResponse<TestEntity>>();
        response.Setup(r => r.Resource).Returns(testEntity);

        _mockContainer
            .Setup(c => c.ReadItemAsync<TestEntity>(It.IsAny<string>(), It.IsAny<PartitionKey>(), null, default))
            .ReturnsAsync(response.Object);

        // Act
        var result = await _repository.GetByIdAsync("1", "partition1");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("1", result?.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenEntityDoesNotExist()
    {
        // Arrange
        _mockContainer
            .Setup(c => c.ReadItemAsync<TestEntity>(It.IsAny<string>(), It.IsAny<PartitionKey>(), null, default))
            .ThrowsAsync(new CosmosException("Not Found", HttpStatusCode.NotFound, 0, "", 0));

        // Act
        var result = await _repository.GetByIdAsync("1", "partition1");

        // Assert
        Assert.Null(result);
    }
}

public class TestEntity
{
    public string Id { get; set; }
    public string PartitionKey { get; set; }
}
