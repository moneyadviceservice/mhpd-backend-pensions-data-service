using MhpdCommon.Extensions;
using MhpdCommon.Models.Configuration;
using MhpdCommon.Utils;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace MhpdCommonTests.Extensions;
public class ServiceCollectionExtensionTests
{
    private readonly ServiceCollectionMock _serviceCollectionMock;

    public ServiceCollectionExtensionTests()
    {
        _serviceCollectionMock = new ServiceCollectionMock();
    }

    [Fact]
    public void WhenServiceCollection_AddsMhpdUtilities_UtilsAreRegistered()
    {
        //Act
        _serviceCollectionMock.ServiceCollection.AddMhpdUtilities();

        //Assert
        _serviceCollectionMock.ContainsScopedService<IIdValidator, IdValidator>();
        _serviceCollectionMock.ContainsScopedService<IMessageParser, MessageParser>();
    }

    [Fact]
    public void WhenServiceCollection_AddsMhpdCosmosDb_ClientIsRegistered()
    {
        //Arrange
        var serviceCollection = new ServiceCollection();
        Environment.SetEnvironmentVariable(CommonCosmosConfiguration.ConnectionStringVariable, "AccountEndpoint=https://localhost/;AccountKey=test");

        //Act
        serviceCollection.AddMhpdCosmosDb();

        var provider = serviceCollection.BuildServiceProvider();
        var cosmosClient = provider.GetRequiredService(typeof(CosmosClient));
        Assert.NotNull(cosmosClient);

        var config = provider.GetRequiredService(typeof(IOptions<CommonCosmosConfiguration>));
        Assert.NotNull(config);
    }
}
