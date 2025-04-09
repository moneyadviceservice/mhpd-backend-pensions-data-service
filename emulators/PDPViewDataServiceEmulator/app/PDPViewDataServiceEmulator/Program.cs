using MhpdCommon.Constants;
using MhpdCommon.Extensions;
using MhpdCommon.Models.Configuration;
using MhpdCommon.Repository;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Azure.Cosmos;
using PDPViewDataServiceEmulator.CosmosRepository;
using PDPViewDataServiceEmulator.Mocks;
using System.Diagnostics.CodeAnalysis;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

var mtlsCertificate = await builder!.ConfigureMtlsWithClientCertificateAsync();

builder.Services.AddMhpdHttpClients();
builder.Services.AddMhpdUtilities();
builder.Services.AddControllers();
builder.Services.AddMhpdCosmosDb();

builder.Services.AddSingleton<CosmosClient>(_ =>
{
    var connString = builder.Configuration.GetConnectionString(DatabaseConstants.ConnectionStringVariable);
    if (string.IsNullOrEmpty(connString))
    {
        throw new InvalidOperationException($"The CosmosDB connection string ('{DatabaseConstants.ConnectionStringVariable}') is missing from the configuration.");
    }

    var options = new CosmosClientOptions
    {
        SerializerOptions = new CosmosSerializationOptions { PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase },
    };
    return new CosmosClient(connString, options);
});

builder.Services.AddSingleton<ICosmosDbRepository<ViewDataPayload>, ViewDataRepository>();

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

app.UseClientCertificateValidation(mtlsCertificate);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();
app.UseHttpLogging();
app.UseHttpsRedirection();
app.Run();

[ExcludeFromCodeCoverage]
public partial class Program { }
