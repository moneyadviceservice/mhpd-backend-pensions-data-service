using Azure.Messaging.ServiceBus;
using MhpdCommon.Constants.HttpClient;
using MhpdCommon.Models.Configuration;
using MhpdCommon.Repository;
using MhpdCommon.Utils;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using MhpdCommon.SharedHttpClient;
using MhpdCommon.Models.MHPDModels.JwkUri;
using MhpdCommon.Constants;
using MhpdCommon.Models.MHPDModels;
using MhpdCommon.Caching;
using PensionRequestFunction.Transformer;

namespace MhpdCommon.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMhpdUtilities(
            this IServiceCollection services)
        {
            services.AddScoped<IIdValidator, IdValidator>();
            services.AddScoped<IMessageParser, MessageParser>();
            services.AddScoped<ITokenUtility, TokenUtility>();
            services.AddScoped<IViewDataTransformer, ViewDataTransformer>();

            return services;
        }

        public static IServiceCollection AddIntegrationServices(
            this IServiceCollection services)
        {
            services.AddScoped<IHolderNameConfigurationCache<HolderNameConfigurationModel>, HolderNameConfigurationCache>();
            services.AddScoped<IJwkKeyCache<JwkUriResponseModel>, JwkKeyCache>();
            services.AddScoped<IJwtUtility, JwtUtility>();
            services.AddScoped<ISharedHttpClient, SharedHttpClient.SharedHttpClient>();
            services.AddScoped<IPeiServiceClient, PeiServiceClient>();
            services.AddScoped<ITokenIntegrationServiceClient, TokenIntegrationServiceClient>();

            return services;
        }

        public static IServiceCollection AddMhpdCosmosDb(
            this IServiceCollection services, IConfiguration? configuration = null)
        {
            configuration = GetConfiguration(services, configuration);

            services.AddSingleton(provider =>
            {
                var connString = configuration[DatabaseConstants.ConnectionStringVariable];

                JsonSerializerOptions jsonSerializerOptions = new()
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true
                };

                var options = new CosmosClientOptions
                {
                    Serializer = new MhpdCosmosSerializer(jsonSerializerOptions)
                };
                return new CosmosClient(connString, options);
            });

            AddCosmosConfiguration<CosmosBusinessConfiguration>(services, configuration, DatabaseConstants.ConfigurationSections.BusinessLayer);
            AddCosmosConfiguration<CosmosIntegrationConfiguration>(services, configuration, DatabaseConstants.ConfigurationSections.IntegrationLayer);
            AddCosmosConfiguration<CosmosTestHarnessConfiguration>(services, configuration, DatabaseConstants.ConfigurationSections.TestHarness);

            return services;
        }

        public static IServiceCollection AddMhpdServiceBusTools(this IServiceCollection services, IConfiguration? configuration = null)
        {
            configuration = GetConfiguration(services, configuration);

            services.AddSingleton(sp =>
            {
                var connectionString = configuration[CommonServiceBusConfiguration.ConnectionStringVariable];
                return new ServiceBusClient(connectionString);
            });

            services.AddOptions<CommonServiceBusConfiguration>().Configure(option =>
            {
                option.InboundQueue = configuration[CommonServiceBusConfiguration.InboundQueueVariable];
                option.OutboundQueue = configuration[CommonServiceBusConfiguration.OutboundQueueVariable];
            }).ValidateOnStart();

            services.AddScoped<IMessagingService, MessagingService>();

            return services;
        }

        public static IServiceCollection AddMhpdHttpClients(this IServiceCollection services, IConfiguration? configuration = null)
        {
            configuration = GetConfiguration(services, configuration);

            AddMhpdHttpClient(services, configuration, HttpClientNames.MapsCdaService, HttpClientUrlVariables.MapsCdaServiceUrl);
            AddMhpdHttpClient(services, configuration, HttpClientNames.TokenIntegrationService, HttpClientUrlVariables.TokenIntegrationServiceUrl);
            AddMhpdHttpClient(services, configuration, HttpClientNames.CdaService, HttpClientUrlVariables.CdaServiceUrl);
            AddMhpdHttpClient(services, configuration, HttpClientNames.PdpService, HttpClientUrlVariables.PdpServiceUrl);
            AddMhpdHttpClient(services, configuration, HttpClientNames.PensionRetrievalService, HttpClientUrlVariables.PensionRetrievalServiceUrl);
            AddMhpdHttpClient(services, configuration, HttpClientNames.RetrievedPensionsService, HttpClientUrlVariables.RetrievedPensionsServiceUrl);
            AddMhpdHttpClient(services, configuration, HttpClientNames.PeiIntegrationService, HttpClientUrlVariables.PeiIntegrationServiceUrl);
            AddMhpdHttpClient(services, configuration, HttpClientNames.ViewDataIntegrationService, HttpClientUrlVariables.ViewDataIntegrationServiceUrl);

            services.AddOptions<CommonHttpConfiguration>().Configure(option =>
            {
                option.MapsCdaServiceUrl = configuration[HttpClientUrlVariables.MapsCdaServiceUrl];
                option.TokenIntegrationServiceUrl = configuration[HttpClientUrlVariables.TokenIntegrationServiceUrl];
                option.CdaServiceUrl = configuration[HttpClientUrlVariables.CdaServiceUrl];
                option.PdpServiceUrl = configuration[HttpClientUrlVariables.PdpServiceUrl];
                option.PensionRetrievalServiceUrl = configuration[HttpClientUrlVariables.PensionRetrievalServiceUrl];
                option.RetrievedPensionsServiceUrl = configuration[HttpClientUrlVariables.RetrievedPensionsServiceUrl];
                option.PeiIntegrationServiceUrl = configuration[HttpClientUrlVariables.PeiIntegrationServiceUrl];
                option.ViewDataIntegrationServiceUrl = configuration[HttpClientUrlVariables.ViewDataIntegrationServiceUrl];
            });

            return services;
        }

        public static IServiceCollection AddCommonConfigurations(this IServiceCollection services, IConfiguration? configuration = null)
        {
            configuration = GetConfiguration(services, configuration);

            services.AddOptions<PeiOrchestrationSettings>().Configure(option =>
            {
                option.PeiRetrievalDuration = TryParseNumeric(configuration[PeiOrchestrationSettings.PeiRetrievalDurationVariable], 
                    PeiOrchestrationSettings.DefaultPeiRetrievalDuration);
                option.PeiPollingInterval = TryParseNumeric(configuration[PeiOrchestrationSettings.PeiPollingIntervalVariable], 
                    PeiOrchestrationSettings.DefaultPeiPollingInterval);
                option.ViewDataRetrievalDuration = TryParseNumeric(configuration[PeiOrchestrationSettings.ViewDataDurationVariable],
                    PeiOrchestrationSettings.DefaultViewDataDuration);
            });

            return services;
        }

        private static IServiceCollection AddCosmosConfiguration<T>(IServiceCollection services, IConfiguration configuration, string configurationSection) where T : class
        {
            var section = configuration.GetSection(configurationSection);

            if (section != null)
            {
                services.Configure<T>(section);
            }

            return services;
        }

        private static IServiceCollection AddMhpdHttpClient(IServiceCollection services, IConfiguration configuration, string serviceName, string serviceUrlVariable)
        {
            var serviceUrl = configuration[serviceUrlVariable];

            if (serviceUrl == null) return services;

            services.AddHttpClient(serviceName, client =>
            {
                client.BaseAddress = new Uri(serviceUrl);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            });

            return services;
        }

        private static IConfiguration GetConfiguration(IServiceCollection services, IConfiguration? configuration)
        {
            if (configuration != null) return configuration;

            var serviceProvider = services.BuildServiceProvider();
            return serviceProvider.GetRequiredService<IConfiguration>();
        }

        private static int TryParseNumeric(string? value, int defaultValue)
        {
            if (int.TryParse(value, out var result)) return result;
            return defaultValue;
        }
    }
}
