using System.Diagnostics.CodeAnalysis;
using MhpdCommon.Extensions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PensionRequestFunction.HttpClient;
using PensionRequestFunction.HttpClient.Implementation;
using PensionRequestFunction.HttpClient.Interfaces;
using PensionRequestFunction.Orchestration;
using PensionRequestFunction.Transformer;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        services.AddHttpClient();
        //services.AddMhpdCosmosDb();
        services.AddMhpdUtilities();
        services.AddMhpdServiceBusTools();
        services.AddMhpdHttpClients();
        services.AddTransient<IPdpViewDataClient, PdpViewDataClient>();
        services.AddTransient<IMapsCdaServiceClient, MapsCdaServiceClient>();
        services.AddTransient<ITokenIntegrationServiceClient, TokenIntegrationServiceClient>();
        services.AddTransient<IHolderNameClient, HolderNameClient>();
        services.AddTransient<IViewDataOrchestrator, ViewDataOrchestrator>();
        services.AddTransient<IVewDataToPensionArrangementTransformer, ViewDataToPensionArrangementTransformer>();
    })
    .Build();

host.Run();

[ExcludeFromCodeCoverage]
public partial class Program { }