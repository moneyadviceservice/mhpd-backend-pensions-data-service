using Azure.Messaging.ServiceBus;
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
    }
}
