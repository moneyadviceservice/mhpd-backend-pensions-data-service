using System.Net;
using CDAServiceEmulator.CosmosRepository;
using CDAServiceEmulator.Models.Peis;
using Microsoft.Azure.Cosmos;
using Moq;
using Newtonsoft.Json.Linq;

namespace CDAServiceEmulatorUnitTests.CosmosRepositoryTests;

public class CdaPeisEmulatorScenarioModelRepositoryTests
{
    private readonly Mock<Container> _mockContainer;
    private readonly CdaPeisEmulatorScenarioModelRepository _repository;

    public CdaPeisEmulatorScenarioModelRepositoryTests()
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
        _repository = new CdaPeisEmulatorScenarioModelRepository(mockCosmosClient.Object, "TestDatabase", "TestContainer");
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsModel_WhenModelExists()
    {
        // Arrange
        var testModel = new CdaPeisEmulatorScenarioModel
        {
            Id = "1",
            PeisIdStartCode = "PEIS123",
            DataPoints = new List<DataPoint>
            {
                new() { AvailableAt = 0, ResponsePayload = new ResponsePayload() },
                new() { AvailableAt = 3, ResponsePayload = new ResponsePayload() }
            }
            
        };

        var response = new Mock<ItemResponse<CdaPeisEmulatorScenarioModel>>();
        response.Setup(r => r.Resource).Returns(testModel);

        _mockContainer
            .Setup(c => c.ReadItemAsync<CdaPeisEmulatorScenarioModel>(It.IsAny<string>(), It.IsAny<PartitionKey>(), null, default))
            .ReturnsAsync(response.Object); // Mock the ReadItemAsync method

        // Act
        var result = await _repository.GetByIdAsync("1", "partition1");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("1", result?.Id);
        Assert.Equal("PEIS123", result?.PeisIdStartCode);
        Assert.Equal(2, result?.DataPoints?.Count);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenModelDoesNotExist()
    {
        // Arrange
        _mockContainer
            .Setup(c => c.ReadItemAsync<CdaPeisEmulatorScenarioModel>(It.IsAny<string>(), It.IsAny<PartitionKey>(), null, default))
            .ThrowsAsync(new CosmosException("Not Found", HttpStatusCode.NotFound, 0, "", 0)); // Mock a not found exception

        // Act
        var result = await _repository.GetByIdAsync("1", "partition1");

        // Assert
        Assert.Null(result);
    }
}