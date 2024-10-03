using System.Diagnostics.CodeAnalysis;
using CDAServiceEmulator;
using CDAServiceEmulator.Configuration;
using CDAServiceEmulator.CosmosRepository;
using CDAServiceEmulator.TokenValidation;
using MhpdCommon.Utils;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddTransient<IIdValidator, IdValidator>();
builder.Services.AddControllers();

builder.Services.AddApplicationInsightsTelemetry();

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
    
    var options = new CosmosClientOptions
    {
        SerializerOptions = new CosmosSerializationOptions { PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase },
    };
    return new CosmosClient(connString, options);
});

builder.Services.Configure<MhpdCosmosConfiguration>(builder.Configuration.GetSection("MhpdCosmosConfiguration"));

// Register CdaPeisEmulatorScenarioModelRepository
builder.Services.AddSingleton<CdaPeisEmulatorScenarioModelRepository>(provider =>
{
    var cosmosClient = provider.GetRequiredService<CosmosClient>();
    var config = provider.GetRequiredService<IOptions<MhpdCosmosConfiguration>>().Value;
    
    return new CdaPeisEmulatorScenarioModelRepository(cosmosClient, config.DatabaseName, config.CdaPeisEmulatorScenarioModelContainerName);
});

// Register CdaPeisEmulatorTestInstanceDataRepository
builder.Services.AddSingleton<CdaPeisEmulatorTestInstanceDataRepository>(provider =>
{
    var cosmosClient = provider.GetRequiredService<CosmosClient>();
    var config = provider.GetRequiredService<IOptions<MhpdCosmosConfiguration>>().Value;
    
    return new CdaPeisEmulatorTestInstanceDataRepository(cosmosClient, config.DatabaseName, config.CdaPeisEmulatorTestInstanceDataContainerName);
});

// Register TokenEmulatorPiesIdScenarioModelsRepository
builder.Services.AddSingleton<TokenEmulatorPiesIdScenarioModelsRepository>(provider =>
{
    var cosmosClient = provider.GetRequiredService<CosmosClient>();
    var config = provider.GetRequiredService<IOptions<MhpdCosmosConfiguration>>().Value;
    
    return new TokenEmulatorPiesIdScenarioModelsRepository(cosmosClient, config.DatabaseName, config.TokenEmulatorPiesIdScenarioModelsContainerName);
});

builder.Services.AddScoped<ITokenRequestValidator, GrantTypeNotPresentValidator>();
builder.Services.AddScoped<ITokenRequestValidator, UnsupportedGrantTypeValidation>();
builder.Services.AddScoped<ITokenRequestValidator, ClaimTokenNotPresentValidation>();
builder.Services.AddScoped<ITokenRequestValidator, ClaimTokenNotJwtValidator>();
builder.Services.AddScoped<ITokenRequestValidator, ClaimTokenFormatNotPresentValidator>();
builder.Services.AddScoped<ITokenRequestValidator, ClaimTokenFormatNotPensionDashboardRqpValidator>();
builder.Services.AddScoped<ITokenRequestValidator, ScopeNotOwnerValidator>();
builder.Services.AddScoped<ITokenRequestValidator, ScopeNotPresentValidator>();
builder.Services.AddScoped<ITokenRequestValidator, TicketNotAJwtValidator>();
builder.Services.AddScoped<ITokenRequestValidator, TicketQueryNotPresentValidator>();
builder.Services.AddScoped<ITokenRequestValidator, ClientIdNotPresentValidation>();
builder.Services.AddScoped<ITokenRequestValidator, ClientIdInvalidFormatValidation>();
builder.Services.AddScoped<ITokenRequestValidator, ClientSecretNotGuidValidation>();
builder.Services.AddScoped<ITokenRequestValidator, ClientSecretNotPresentValidation>();
builder.Services.AddScoped<ITokenRequestValidator, CodeNotPresentValidation>();
builder.Services.AddScoped<ITokenRequestValidator, CodeInvalidFormatValidation>();
builder.Services.AddScoped<ITokenRequestValidator, CodeVerifierNotBase64String>();
builder.Services.AddScoped<ITokenRequestValidator, CodeVerifierNotPresentValidation>();
builder.Services.AddScoped<ITokenRequestValidator, RedirectUriNotPresentValidation>();
builder.Services.AddScoped<ITokenRequestValidator, RedirectUriNotValidUrlValidation>();
builder.Services.AddScoped<TokenRequestValidatorPipeline>();

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

// Bind JwtSettings
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.AddScoped<Utils>();

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