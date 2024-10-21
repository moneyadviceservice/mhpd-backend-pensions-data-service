using Azure.Messaging.ServiceBus;
using MhpdCommon.Constants.HttpClient;
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
        _serviceCollectionMock.ContainsScopedService<ITokenUtility, TokenUtility>();
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

    [Fact]
    public void WhenServiceCollection_AddsMhpdHttpClients_ClientIsRegistered()
    {
        //Arrange
        const string cdaEmulatorUrl = "https://cda.emulator.net";
        const string tokenIntegrationUrl = "http://integration.token.com";
        const string retrievalUrl = "https://retrieve.pensions.gov";

        var serviceCollection = new ServiceCollection();
        Environment.SetEnvironmentVariable(HttpClientUrlVariables.CdaServiceUrl, cdaEmulatorUrl);
        Environment.SetEnvironmentVariable(HttpClientUrlVariables.TokenIntegrationServiceUrl, tokenIntegrationUrl);
        Environment.SetEnvironmentVariable(HttpClientUrlVariables.PensionRetrievalServiceUrl, retrievalUrl);

        //Act
        serviceCollection.AddMhpdHttpClients();

        var provider = serviceCollection.BuildServiceProvider();
        var factory = provider.GetRequiredService<IHttpClientFactory>();
        var cdaEmulatorClient = factory.CreateClient(HttpClientNames.CdaService);
        var tokenClient = factory.CreateClient(HttpClientNames.TokenIntegrationService);
        var mapsCdaClient = factory.CreateClient(HttpClientNames.MapsCdaService);
        var retrievalClient = factory.CreateClient(HttpClientNames.PensionRetrievalService);
        var retrievedClient = factory.CreateClient(HttpClientNames.RetrievedPensionsService);
        var config = provider.GetRequiredService(typeof(IOptions<CommonHttpConfiguration>));

        //Assert
        Assert.Equal(cdaEmulatorUrl, cdaEmulatorClient!.BaseAddress!.OriginalString);
        Assert.Equal(tokenIntegrationUrl, tokenClient!.BaseAddress!.OriginalString);
        Assert.Equal(retrievalUrl, retrievalClient!.BaseAddress!.OriginalString);
        Assert.Null(retrievedClient!.BaseAddress);
        Assert.Null(mapsCdaClient!.BaseAddress);
        Assert.NotNull(config);
    }
}
