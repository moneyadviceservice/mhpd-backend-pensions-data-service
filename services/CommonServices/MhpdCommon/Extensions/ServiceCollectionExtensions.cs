using Azure.Messaging.ServiceBus;
using MhpdCommon.Constants.HttpClient;
using MhpdCommon.Models.Configuration;
using MhpdCommon.Utils;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;

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
            this IServiceCollection services)
        {
            services.AddSingleton<CosmosClient>(provider =>
            {
                var connString = Environment.GetEnvironmentVariable(CommonCosmosConfiguration.ConnectionStringVariable);
                var options = new CosmosClientOptions
                {
                    SerializerOptions = new CosmosSerializationOptions { PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase },
                };
                return new CosmosClient(connString, options);
            });

            services.AddOptions<CommonCosmosConfiguration>().Configure(option =>
            {
                option.DatabaseId = Environment.GetEnvironmentVariable(CommonCosmosConfiguration.DatabaseVariable);
                option.ContainerId = Environment.GetEnvironmentVariable(CommonCosmosConfiguration.ContainerVariable);
                option.ContainerPartitionKey = Environment.GetEnvironmentVariable(CommonCosmosConfiguration.PartitionVariable);
            }).ValidateOnStart();

            return services;
        }

        public static IServiceCollection AddMhpdServiceBusTools(this IServiceCollection services)
        {
            services.AddSingleton(sp =>
            {
                var connectionString = Environment.GetEnvironmentVariable(CommonServiceBusConfiguration.ConnectionStringVariable);
                return new ServiceBusClient(connectionString);
            });

            services.AddOptions<CommonServiceBusConfiguration>().Configure(option =>
            {
                option.InboundQueue = Environment.GetEnvironmentVariable(CommonServiceBusConfiguration.InboundQueueVariable);
                option.OutboundQueue = Environment.GetEnvironmentVariable(CommonServiceBusConfiguration.OutboundQueueVariable);
            }).ValidateOnStart();

            services.AddScoped<IMessagingService, MessagingService>();

            return services;
        }

        public static IServiceCollection AddMhpdHttpClients(this IServiceCollection services)
        {
            AddMhpdHttpClient(services, HttpClientNames.MapsCdaService, HttpClientUrlVariables.MapsCdaServiceUrl);
            AddMhpdHttpClient(services, HttpClientNames.TokenIntegrationService, HttpClientUrlVariables.TokenIntegrationServiceUrl);
            AddMhpdHttpClient(services, HttpClientNames.CdaService, HttpClientUrlVariables.CdaServiceUrl);
            AddMhpdHttpClient(services, HttpClientNames.PensionRetrievalService, HttpClientUrlVariables.PensionRetrievalServiceUrl);
            AddMhpdHttpClient(services, HttpClientNames.RetrievedPensionsService, HttpClientUrlVariables.RetrievedPensionsServiceUrl);
            AddMhpdHttpClient(services, HttpClientNames.PeiIntegrationService, HttpClientUrlVariables.PeiIntegrationServiceUrl);

            services.AddOptions<CommonHttpConfiguration>().Configure(option =>
            {
                option.MapsCdaServiceUrl = Environment.GetEnvironmentVariable(HttpClientUrlVariables.MapsCdaServiceUrl);
                option.TokenIntegrationServiceUrl = Environment.GetEnvironmentVariable(HttpClientUrlVariables.TokenIntegrationServiceUrl);
                option.CdaServiceUrl = Environment.GetEnvironmentVariable(HttpClientUrlVariables.CdaServiceUrl);
                option.PensionRetrievalServiceUrl = Environment.GetEnvironmentVariable(HttpClientUrlVariables.PensionRetrievalServiceUrl);
                option.RetrievedPensionsServiceUrl = Environment.GetEnvironmentVariable(HttpClientUrlVariables.RetrievedPensionsServiceUrl);
                option.PeiIntegrationServiceUrl = Environment.GetEnvironmentVariable(HttpClientUrlVariables.PeiIntegrationServiceUrl);
            });

            return services;
        }

        public static IServiceCollection AddMhpdHttpClient(IServiceCollection services, string serviceName, string serviceUrlVariable)
        {
            var serviceUrl = Environment.GetEnvironmentVariable(serviceUrlVariable);

            if (serviceUrl == null) return services;

            services.AddHttpClient(serviceName, client =>
            {
                client.BaseAddress = new Uri(serviceUrl);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            });

            return services;
        }
    }
}
