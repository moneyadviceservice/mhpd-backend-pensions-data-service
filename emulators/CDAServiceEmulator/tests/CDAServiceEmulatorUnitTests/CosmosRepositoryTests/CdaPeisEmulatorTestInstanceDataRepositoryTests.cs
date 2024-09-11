using System.Net;
using CDAServiceEmulator.CosmosRepository;
using CDAServiceEmulator.Models.Peis;
using Microsoft.Azure.Cosmos;
using Moq;
using Newtonsoft.Json.Linq;

namespace CDAServiceEmulatorUnitTests.CosmosRepositoryTests;

public class CdaPeisEmulatorTestInstanceDataRepositoryTests
{
    private readonly Mock<Container> _mockContainer;
    private readonly CdaPeisEmulatorTestInstanceDataRepository _repository;

    public CdaPeisEmulatorTestInstanceDataRepositoryTests()
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
        _repository = new CdaPeisEmulatorTestInstanceDataRepository(mockCosmosClient.Object, "TestDatabase", "TestContainer");
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsModel_WhenModelExists()
    {
        // Arrange
        var currentTimeInUtc = DateTimeOffset.UtcNow;
        
        var testModel = new CdaPeisEmulatorTestInstanceDataModel
        {
            Id = "1",
            PeisId = "0001",
            InitialCallTimestamp = currentTimeInUtc
        };

        var response = new Mock<ItemResponse<CdaPeisEmulatorTestInstanceDataModel>>();
        response.Setup(r => r.Resource).Returns(testModel);

        _mockContainer
            .Setup(c => c.ReadItemAsync<CdaPeisEmulatorTestInstanceDataModel>(It.IsAny<string>(), It.IsAny<PartitionKey>(), null, default))
            .ReturnsAsync(response.Object); // Mock the ReadItemAsync method

        // Act
        var result = await _repository.GetByIdAsync("1", "partition1");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("1", result.Id);
        Assert.Equal("0001", result?.PeisId);
        Assert.Equal(currentTimeInUtc, result?.InitialCallTimestamp);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenModelDoesNotExist()
    {
        // Arrange
        _mockContainer
            .Setup(c => c.ReadItemAsync<CdaPeisEmulatorTestInstanceDataModel>(It.IsAny<string>(), It.IsAny<PartitionKey>(), null, default))
            .ThrowsAsync(new CosmosException("Not Found", HttpStatusCode.NotFound, 0, "", 0)); // Mock a not found exception

        // Act
        var result = await _repository.GetByIdAsync("1", "partition1");

        // Assert
        Assert.Null(result);
    }
}