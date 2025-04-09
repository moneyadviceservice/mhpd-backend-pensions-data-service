using CDAServiceEmulator.CosmosRepository;
using CDAServiceEmulator.Models.ViewData;
using MhpdCommon.Models.Configuration;
using MhpdCommon.Models.MHPDModels;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CDAServiceEmulatorUnitTests.CosmosRepositoryTests;

public class HolderNameViewDataRepositoryTests
{
    private readonly Mock<Container> _mockContainer;
    private readonly HolderNameViewDataRepository _repository;

    public HolderNameViewDataRepositoryTests()
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

        var configuration = Options.Create(new CosmosTestHarnessConfiguration
        {
            DatabaseName = "TestDatabase",
            ViewDataModelContainerName = "TestContainer"
        });

        // Instantiate the repository with the mocked CosmosClient, Database, and Container
        _repository = new HolderNameViewDataRepository(mockCosmosClient.Object, configuration);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsModel_WhenModelExists()
    {
        // Arrange
        var testModel = new HolderNameViewDataResponse
        {
            Id = "200",
            HolderNameGuid = "200",
            Configuration = new HolderNameConfigurationModel
            {
                ViewDataUrl = "testUrl"
            }
        };

        var response = new Mock<ItemResponse<HolderNameViewDataResponse>>();
        response.Setup(r => r.Resource).Returns(testModel);

        _mockContainer
            .Setup(c => c.ReadItemAsync<HolderNameViewDataResponse>(It.IsAny<string>(), It.IsAny<PartitionKey>(), null, default))
            .ReturnsAsync(response.Object); // Mock the ReadItemAsync method

        // Act
        var result = await _repository.GetByIdAsync("1", "partition1");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("200", result?.Id);
        Assert.Equal("200", result?.HolderNameGuid);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenModelDoesNotExist()
    {
        // Arrange
        _mockContainer
            .Setup(c => c.ReadItemAsync<HolderNameViewDataResponse>(It.IsAny<string>(), It.IsAny<PartitionKey>(), null, default))
            .ThrowsAsync(new CosmosException("Not Found", HttpStatusCode.NotFound, 0, "", 0)); // Mock a not found exception

        // Act
        var result = await _repository.GetByIdAsync("1", "partition1");

        // Assert
        Assert.Null(result);
    }
}
