using System.Diagnostics.CodeAnalysis;
using CDAServiceEmulator.Configuration;
using CDAServiceEmulator.CosmosRepository;
using MhpdCommon.Utils;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// // Configure Configuration
// IConfiguration configuration = builder.Configuration;
// configuration = new ConfigurationBuilder()
//     .SetBasePath(Directory.GetCurrentDirectory())
//     .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
//     .Build();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddTransient<IIdValidator, IdValidator>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<CosmosClient>(_ =>
{
    var connString = builder.Configuration.GetConnectionString("cosmosDBConnectionString");
    if (string.IsNullOrEmpty(connString))
    {
        throw new ArgumentNullException(nameof(connString), "CosmosDBConnectionString is missing from the configuration.");
    }
    Console.WriteLine("ConnString {0}", connString);
    
    var options = new CosmosClientOptions
    {
        SerializerOptions = new CosmosSerializationOptions { PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase },
    };
    return new CosmosClient(connString, options);
});

builder.Services.Configure<MhpdCosmosConfiguration>(builder.Configuration.GetSection("MhpdCosmosConfigurationCdaPeisEmulatorScenario"));

// Register CdaPeisEmulatorScenarioModelRepository
builder.Services.AddSingleton<CdaPeisEmulatorScenarioModelRepository>(provider =>
{
    var cosmosClient = provider.GetRequiredService<CosmosClient>();
    var config = provider.GetRequiredService<IOptions<MhpdCosmosConfiguration>>().Value;
    
    Console.WriteLine("CdaPeisEmulatorScenarioModelContainerName {0} {1}", config.CdaPeisEmulatorScenarioModelContainerName, config.DatabaseName);

    return new CdaPeisEmulatorScenarioModelRepository(cosmosClient, config.DatabaseName, config.CdaPeisEmulatorScenarioModelContainerName);
});

// Register CdaPeisEmulatorTestInstanceDataRepository
builder.Services.AddSingleton<CdaPeisEmulatorTestInstanceDataRepository>(provider =>
{
    var cosmosClient = provider.GetRequiredService<CosmosClient>();
    var config = provider.GetRequiredService<IOptions<MhpdCosmosConfiguration>>().Value;
    
    Console.WriteLine("CdaPeisEmulatorTestInstanceDataContainerName {0} {1}", config.CdaPeisEmulatorTestInstanceDataContainerName, config.DatabaseName);
    
    return new CdaPeisEmulatorTestInstanceDataRepository(cosmosClient, config.DatabaseName, config.CdaPeisEmulatorTestInstanceDataContainerName);
});

builder.Services.AddHttpLogging(logging =>
{
    logging.LoggingFields = HttpLoggingFields.All;
    logging.RequestHeaders.Add("X-Request-ID");
    logging.RequestHeaders.Add("Authorisation");
    logging.ResponseHeaders.Add("WWW-Authenticate");
    logging.RequestBodyLogLimit = 4096;
    logging.ResponseBodyLogLimit = 4096;
    logging.CombineLogs = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseAuthorization();

app.MapControllers();

app.UseHttpLogging();

app.Run();

[ExcludeFromCodeCoverage]
public partial class Program { }