using System.Net;
using MhpdCommon.Repository;
using Microsoft.Azure.Cosmos;
using Moq;

namespace MhpdCommonTests.Repository;

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
    
    [Fact]
    public async Task InsetItemAsync_CallsUpsertItemAsync_WithCorrectParameters()
    {
        // Arrange
        var item = new TestEntity { Id = "test-id" };
        var partitionKey = "test-id";

        // Create a mock ItemResponse
        var mockItemResponse = new Mock<ItemResponse<TestEntity>>();
        _mockContainer.Setup(container =>
                container.UpsertItemAsync(item, new PartitionKey(partitionKey), null, default))
            .ReturnsAsync(mockItemResponse.Object)
            .Verifiable();

        // Act
        await _repository.InsertItemAsync(item, partitionKey);

        // Assert
        _mockContainer.Verify(container =>
                container.UpsertItemAsync(item, new PartitionKey(partitionKey), null, default),
            Times.Once);
    }
    
    [Fact]
    public async Task InsertItemAsync_ThrowsArgumentNullException_WhenItemIsNull()
    {
        // Arrange
        TestEntity? item = null;
        var partitionKey = "test-id";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.InsertItemAsync(item!, partitionKey));
    }

    [Fact]
    public async Task InsertItemAsync_ThrowsArgumentNullException_WhenPartitionKeyIsNull()
    {
        // Arrange
        var item = new TestEntity { Id = "test-id" };
        string? partitionKey = null;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.InsertItemAsync(item, partitionKey!));
    }

    [Fact]
    public async Task InsertItemAsync_ThrowsException_WhenUpsertFails()
    {
        // Arrange
        var item = new TestEntity { Id = "test-id" };
        var partitionKey = "test-id";

        // Simulate a failure in UpsertItemAsync by throwing a CosmosException
        _mockContainer.Setup(container =>
                container.UpsertItemAsync(item, new PartitionKey(partitionKey), null, default))
            .ThrowsAsync(new CosmosException("Some error", System.Net.HttpStatusCode.InternalServerError, 0, "", 0));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _repository.InsertItemAsync(item, partitionKey));
        Assert.Equal("Failed to upsert item.", exception.Message);
    }
}

public class TestEntity
{
    public string Id { get; set; } = string.Empty;
    public string PartitionKey { get; set; } = string.Empty;
}
