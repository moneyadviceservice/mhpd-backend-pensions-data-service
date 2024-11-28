using System.Diagnostics.CodeAnalysis;
using MhpdCommon.Extensions;
using MhpdCommon.Models.Configuration;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using PDPViewDataServiceEmulator.Configuration;
using PDPViewDataServiceEmulator.CosmosRepository;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();
builder.Services.AddMhpdUtilities();
builder.Services.AddControllers();

builder.Services.AddSingleton<CosmosClient>(_ =>
{
    var connString = builder.Configuration.GetConnectionString("cosmosDBConnectionString");
    if (string.IsNullOrEmpty(connString))
    {
        throw new InvalidOperationException("The CosmosDB connection string ('cosmosDBConnectionString') is missing from the configuration.");
    }
    
    var options = new CosmosClientOptions
    {
        SerializerOptions = new CosmosSerializationOptions { PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase },
    };
    return new CosmosClient(connString, options);
});

builder.Services.Configure<MhpdCosmosConfiguration>(builder.Configuration.GetSection("MhpdCosmosConfiguration"));

// Register ViewdatapayloadsContainerName
builder.Services.AddSingleton<ViewDataRepository>(provider =>
{
    var cosmosClient = provider.GetRequiredService<CosmosClient>();
    var config = provider.GetRequiredService<IOptions<MhpdCosmosConfiguration>>().Value;
    
    return new ViewDataRepository(cosmosClient, config.DatabaseName, config.ViewdatapayloadsContainerName);
});

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpLogging(logging =>
{
    logging.LoggingFields = HttpLoggingFields.All;
    logging.RequestHeaders.Add("X-Request-ID");    
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
