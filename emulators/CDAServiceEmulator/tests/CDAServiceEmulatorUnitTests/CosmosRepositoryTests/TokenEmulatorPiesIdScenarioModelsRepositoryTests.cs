using System.Net;
using CDAServiceEmulator.CosmosRepository;
using CDAServiceEmulator.Models.Token;
using Microsoft.Azure.Cosmos;
using Moq;

namespace CDAServiceEmulatorUnitTests.CosmosRepositoryTests;

public class TokenEmulatorPiesIdScenarioModelsRepositoryTests
{
    private readonly Mock<Container> _mockContainer;
    private readonly TokenEmulatorPiesIdScenarioModelsRepository _repository;

    public TokenEmulatorPiesIdScenarioModelsRepositoryTests()
    {
        Mock<CosmosClient> mockCosmosClient = new();
        _mockContainer = new Mock<Container>();
        Mock<Database> mockDatabase = new();

        // Set up the mocks
        mockCosmosClient
            .Setup(c => c.GetDatabase(It.IsAny<string>()))
            .Returns(mockDatabase.Object); // Mock the Database

        mockDatabase
            .Setup(d => d.GetContainer(It.IsAny<string>()))
            .Returns(_mockContainer.Object); // Mock the Container

        // Instantiate the repository with the mocked CosmosClient, Database, and Container
        _repository = new TokenEmulatorPiesIdScenarioModelsRepository(mockCosmosClient.Object, "TestDatabase", "TestContainer");
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsModel_WhenModelExists()
    {
        // Arrange
        var testModel = new TokenEmulatorPiesIdScenarioModel
        {
            Id = "1",
            Code = "1",
            PeisIdStartCode = "PEIS123"
        };

        var response = new Mock<ItemResponse<TokenEmulatorPiesIdScenarioModel>>();
        response.Setup(r => r.Resource).Returns(testModel);

        _mockContainer
            .Setup(c => c.ReadItemAsync<TokenEmulatorPiesIdScenarioModel>(It.IsAny<string>(), It.IsAny<PartitionKey>(), null, default))
            .ReturnsAsync(response.Object); // Mock the ReadItemAsync method

        // Act
        var result = await _repository.GetByIdAsync("1", "partition1");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("1", result?.Id);
        Assert.Equal("1", result?.Code);
        Assert.Equal("PEIS123", result?.PeisIdStartCode);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenModelDoesNotExist()
    {
        // Arrange
        _mockContainer
            .Setup(c => c.ReadItemAsync<TokenEmulatorPiesIdScenarioModel>(It.IsAny<string>(), It.IsAny<PartitionKey>(), null, default))
            .ThrowsAsync(new CosmosException("Not Found", HttpStatusCode.NotFound, 0, "", 0)); // Mock a not found exception

        // Act
        var result = await _repository.GetByIdAsync("1", "partition1");

        // Assert
        Assert.Null(result);
    }
}