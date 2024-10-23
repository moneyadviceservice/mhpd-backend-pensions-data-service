using Azure.Messaging.ServiceBus;
using MhpdCommon.Constants.HttpClient;
using MhpdCommon.Models.Configuration;
using MhpdCommon.Repository;
using MhpdCommon.Utils;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

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

            return services;
        }

        public static IServiceCollection AddMhpdCosmosDb(
            this IServiceCollection services, IConfiguration? configuration = null)
        {
            configuration = GetConfiguration(services, configuration);

            services.AddSingleton(provider =>
            {
                var connString = configuration[CommonCosmosConfiguration.ConnectionStringVariable];

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

            services.AddOptions<CommonCosmosConfiguration>().Configure(option =>
            {
                option.DatabaseId = configuration[CommonCosmosConfiguration.DatabaseVariable];
                option.ContainerId = configuration[CommonCosmosConfiguration.ContainerVariable];
                option.ContainerPartitionKey = configuration[CommonCosmosConfiguration.PartitionVariable];
            }).ValidateOnStart();

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
            AddMhpdHttpClient(services, configuration, HttpClientNames.PensionRetrievalService, HttpClientUrlVariables.PensionRetrievalServiceUrl);
            AddMhpdHttpClient(services, configuration, HttpClientNames.RetrievedPensionsService, HttpClientUrlVariables.RetrievedPensionsServiceUrl);
            AddMhpdHttpClient(services, configuration, HttpClientNames.PeiIntegrationService, HttpClientUrlVariables.PeiIntegrationServiceUrl);

            services.AddOptions<CommonHttpConfiguration>().Configure(option =>
            {
                option.MapsCdaServiceUrl = configuration[HttpClientUrlVariables.MapsCdaServiceUrl];
                option.TokenIntegrationServiceUrl = configuration[HttpClientUrlVariables.TokenIntegrationServiceUrl];
                option.CdaServiceUrl = configuration[HttpClientUrlVariables.CdaServiceUrl];
                option.PensionRetrievalServiceUrl = configuration[HttpClientUrlVariables.PensionRetrievalServiceUrl];
                option.RetrievedPensionsServiceUrl = configuration[HttpClientUrlVariables.RetrievedPensionsServiceUrl];
                option.PeiIntegrationServiceUrl = configuration[HttpClientUrlVariables.PeiIntegrationServiceUrl];
            });

            return services;
        }

        public static IServiceCollection AddMhpdHttpClient(IServiceCollection services, IConfiguration configuration, string serviceName, string serviceUrlVariable)
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
    }
}
