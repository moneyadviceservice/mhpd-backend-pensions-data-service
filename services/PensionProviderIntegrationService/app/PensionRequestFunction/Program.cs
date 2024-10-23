using System.Diagnostics.CodeAnalysis;
using MhpdCommon.Extensions;
using MhpdCommon.Models.MHPDModels;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PensionRequestFunction.HttpClient;
using PensionRequestFunction.HttpClient.Implementation;
using PensionRequestFunction.HttpClient.Interfaces;
using PensionRequestFunction.Orchestration;
using PensionRequestFunction.Repository;
using PensionRequestFunction.Transformer;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true);
    })
    .ConfigureServices((context, services) =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        services.AddMhpdCosmosDb(context.Configuration);
        services.AddMhpdUtilities();
        services.AddMhpdServiceBusTools(context.Configuration);
        services.AddMhpdHttpClients(context.Configuration);
        services.AddTransient<IPdpViewDataClient, PdpViewDataClient>();
        services.AddTransient<IMapsCdaServiceClient, MapsCdaServiceClient>();
        services.AddTransient<ITokenIntegrationServiceClient, TokenIntegrationServiceClient>();
        services.AddTransient<IHolderNameClient, HolderNameClient>();
        services.AddTransient<IViewDataOrchestrator, ViewDataOrchestrator>();
        services.AddTransient<IVewDataToPensionArrangementTransformer, ViewDataToPensionArrangementTransformer>();
        services.AddTransient<IHolderNameConfigurationRepository<HolderNameConfigurationModel>, HolderNameConfigurationRepository>();
    })
    .Build();

host.Run();

[ExcludeFromCodeCoverage]
public partial class Program { }