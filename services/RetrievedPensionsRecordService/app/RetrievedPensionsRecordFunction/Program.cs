using MhpdCommon.Utils;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RetrievedPensionsRecordFunction.Models.Configuration;
using RetrievedPensionsRecordFunction.Repository;
using RetrievedPensionsRecordFunction.Utils;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true);
    })
    .ConfigureServices((hostContext, services) =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        services.AddSingleton<CosmosClient>(provider =>
        {
            var connString = hostContext.Configuration.GetConnectionString("CosmosDBConnectionString");
            var options = new CosmosClientOptions
            {
                SerializerOptions = new CosmosSerializationOptions { PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase },
            };
            return new CosmosClient(connString, options);
        });
        services.AddTransient<IIdValidator, IdValidator>();
        services.AddTransient<IMessageParser, MessageParser>();
        services.AddTransient<IPensionRecordValidator, PensionRecordValidator>();
        services.AddTransient<IPensionRecordRepository, PensionRecordRepository>();

        services.Configure<MhpdCosmosConfiguration>(hostContext.Configuration.GetSection("MhpdCosmosConfiguration"));
    })
    .Build();

await host.RunAsync();
