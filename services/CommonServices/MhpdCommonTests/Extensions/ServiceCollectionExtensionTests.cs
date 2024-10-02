using Azure.Messaging.ServiceBus;
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

        //Assert
        var provider = serviceCollection.BuildServiceProvider();
        var cosmosClient = provider.GetRequiredService(typeof(CosmosClient));
        Assert.NotNull(cosmosClient);

        var config = provider.GetRequiredService(typeof(IOptions<CommonCosmosConfiguration>));
        Assert.NotNull(config);
    }

    [Fact]
    public void WhenServiceCollection_AddsServiceBusTools_ClientIsRegistered()
    {
        //Arrange
        var serviceCollection = new ServiceCollection();
        Environment.SetEnvironmentVariable(CommonServiceBusConfiguration.ConnectionStringVariable, 
            "Endpoint=sb://mhpd.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=K@y");

        //Act
        _serviceCollectionMock.ServiceCollection.AddMhpdServiceBusTools();
        serviceCollection.AddMhpdServiceBusTools();

        //Assert
        var provider = serviceCollection.BuildServiceProvider();
        var serviceBusClient = provider.GetRequiredService(typeof(ServiceBusClient));
        Assert.NotNull(serviceBusClient);

        var config = provider.GetRequiredService(typeof(IOptions<CommonServiceBusConfiguration>));
        Assert.NotNull(config);

        _serviceCollectionMock.ContainsScopedService<IMessagingService, MessagingService>();
    }
}
