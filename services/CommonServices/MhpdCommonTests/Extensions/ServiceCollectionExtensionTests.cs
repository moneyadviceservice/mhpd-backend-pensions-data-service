using Azure.Messaging.ServiceBus;
using MhpdCommon.Caching;
using MhpdCommon.Constants;
using MhpdCommon.Constants.HttpClient;
using MhpdCommon.Extensions;
using MhpdCommon.Models.Configuration;
using MhpdCommon.Models.MHPDModels;
using MhpdCommon.Models.MHPDModels.JwkUri;
using MhpdCommon.SharedHttpClient;
using MhpdCommon.Utils;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;

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
    public void WhenServiceCollection_AddsIntegrationServices_ServicesAreRegistered()
    {
        //Arrange
        var inMemorySettings = new Dictionary<string, string>
        {
            {$"{DatabaseConstants.ConfigurationSections.IntegrationLayer}:DatabaseId", "integration-db"},
            {$"{DatabaseConstants.ConfigurationSections.IntegrationLayer}:HolderNameCacheContainer", "holderNameCache"}
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings!)
            .Build();

        var configurationSectionMock = new Mock<IConfigurationSection>();
        configurationSectionMock
            .Setup(x => x[DatabaseConstants.ConfigurationSections.IntegrationLayer])
            .Returns((string key) => configuration[$"{DatabaseConstants.ConfigurationSections.IntegrationLayer}:{key}"]);

        var mockConfiguration = new Mock<IConfiguration>();
        mockConfiguration
           .Setup(x => x.GetSection(DatabaseConstants.ConfigurationSections.IntegrationLayer))
           .Returns(configurationSectionMock.Object);

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(mockConfiguration.Object);

        //Act
        serviceCollection.AddIntegrationServices();
        _serviceCollectionMock.ServiceCollection.AddIntegrationServices(mockConfiguration.Object);

        //Assert
        var provider = serviceCollection.BuildServiceProvider();

        var integrationConfig = provider.GetRequiredService(typeof(IOptions<CosmosIntegrationConfiguration>));
        Assert.NotNull(integrationConfig);

        _serviceCollectionMock.ContainsScopedService<IJwtUtility, JwtUtility>();
        _serviceCollectionMock.ContainsScopedService<ISharedHttpClient, SharedHttpClient>();
        _serviceCollectionMock.ContainsScopedService<IHolderNameConfigurationCache<HolderNameConfigurationModel>, HolderNameConfigurationCache>();
        _serviceCollectionMock.ContainsScopedService<IJwkKeyCache<JwkUriResponseModel>, JwkKeyCache>();
    }

    [Fact]
    public void WhenServiceCollection_AddsMhpdCosmosDb_ClientIsRegistered()
    {
        //Arrange
        var mockConfiguration = new Mock<IConfiguration>();
        mockConfiguration.Setup(x => x[CommonCosmosConfiguration.ConnectionStringVariable]).Returns("AccountEndpoint=https://localhost/;AccountKey=test");

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(mockConfiguration.Object);

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
        var mockConfiguration = new Mock<IConfiguration>();
        mockConfiguration.Setup(x => x[CommonServiceBusConfiguration.ConnectionStringVariable])
            .Returns("Endpoint=sb://mhpd.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=K@y");

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(mockConfiguration.Object);

        //Act
        _serviceCollectionMock.ServiceCollection.AddMhpdServiceBusTools(mockConfiguration.Object);
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

        var mockConfiguration = new Mock<IConfiguration>();
        mockConfiguration.Setup(x => x[HttpClientUrlVariables.CdaServiceUrl]).Returns(cdaEmulatorUrl);
        mockConfiguration.Setup(x => x[HttpClientUrlVariables.TokenIntegrationServiceUrl]).Returns(tokenIntegrationUrl);
        mockConfiguration.Setup(x => x[HttpClientUrlVariables.PensionRetrievalServiceUrl]).Returns(retrievalUrl);

        var serviceCollection = new ServiceCollection();

        //Act
        serviceCollection.AddMhpdHttpClients(mockConfiguration.Object);

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

    [Fact]
    public void WhenServiceCollection_AddsCommonConfiguration_OptionsAreLoaded()
    {
        //Arrange
        var mockConfiguration = new Mock<IConfiguration>();
        var peiRetrievalDuration = 100;
        var viewdataRetrievalDuration = 30;
        mockConfiguration.Setup(x => x[PeiOrchestrationSettings.PeiRetrievalDurationVariable]).Returns($"{peiRetrievalDuration}");
        mockConfiguration.Setup(x => x[PeiOrchestrationSettings.ViewDataDurationVariable]).Returns($"{viewdataRetrievalDuration}");

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(mockConfiguration.Object);

        //Act
        serviceCollection.AddCommonConfigurations();
        var provider = serviceCollection.BuildServiceProvider();

        //Assert
        var config = provider.GetRequiredService(typeof(IOptions<PeiOrchestrationSettings>)) as IOptions<PeiOrchestrationSettings>;
        Assert.NotNull(config);
        Assert.Equal(peiRetrievalDuration, config.Value.PeiRetrievalDuration);
        Assert.Equal(PeiOrchestrationSettings.DefaultPeiPollingInterval, config.Value.PeiPollingInterval);
        Assert.Equal(peiRetrievalDuration + viewdataRetrievalDuration, config.Value.TotalPensionRetrievalDuration);
    }
}
