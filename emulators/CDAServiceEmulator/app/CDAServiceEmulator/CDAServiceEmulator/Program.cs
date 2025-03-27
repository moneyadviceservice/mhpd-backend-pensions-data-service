using CDAServiceEmulator.CosmosRepository;
using MhpdCommon.Extensions;
using MhpdCommon.Models.Configuration;
using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.Models.MHPDModels;
using MhpdCommon.TokenValidation;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

var mtlsCertificate = await builder!.ConfigureMtlsWithClientCertificateAsync();

// Add services to the container.
builder.Services.AddMhpdHttpClients();
builder.Services.AddMhpdUtilities();
builder.Services.AddMhpdCosmosDb();
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

// Register CdaPeisEmulatorScenarioModelRepository
builder.Services.AddSingleton<CdaPeisEmulatorScenarioModelRepository>(provider =>
{
    var cosmosClient = provider.GetRequiredService<CosmosClient>();
    var config = provider.GetRequiredService<IOptions<CosmosTestHarnessConfiguration>>().Value;
    
    return new CdaPeisEmulatorScenarioModelRepository(cosmosClient, config.DatabaseName, config.CdaPeisEmulatorScenarioModelContainerName);
});

// Register CdaPeisEmulatorTestInstanceDataRepository
builder.Services.AddSingleton<CdaPeisEmulatorTestInstanceDataRepository>(provider =>
{
    var cosmosClient = provider.GetRequiredService<CosmosClient>();
    var config = provider.GetRequiredService<IOptions<CosmosTestHarnessConfiguration>>().Value;
    
    return new CdaPeisEmulatorTestInstanceDataRepository(cosmosClient, config.DatabaseName, config.CdaPeisEmulatorTestInstanceDataContainerName);
});

// Register TokenEmulatorPiesIdScenarioModelsRepository
builder.Services.AddSingleton<TokenEmulatorPiesIdScenarioModelsRepository>(provider =>
{
    var cosmosClient = provider.GetRequiredService<CosmosClient>();
    var config = provider.GetRequiredService<IOptions<CosmosTestHarnessConfiguration>>().Value;
    
    return new TokenEmulatorPiesIdScenarioModelsRepository(cosmosClient, config.DatabaseName, config.TokenEmulatorPiesIdScenarioModelsContainerName);
});

// Register HolderNameViewDataRepository
builder.Services.AddSingleton<IHolderNameViewDataRepository<HolderNameViewDataResponse>>(provider =>
{
    var cosmosClient = provider.GetRequiredService<CosmosClient>();
    var config = provider.GetRequiredService<IOptions<CosmosTestHarnessConfiguration>>().Value;

    return new HolderNameViewDataRepository(cosmosClient, config.DatabaseName, config.HolderNameConfigurationModelsContainerName);
});

builder.Services.AddSingleton(provider =>
{
    var cosmosClient = provider.GetRequiredService<CosmosClient>();
    var config = provider.GetRequiredService<IOptions<CosmosTestHarnessConfiguration>>().Value;

    return new ViewDataRepository(cosmosClient, config.DatabaseName, config.ViewDataModelContainerName);
});

builder.Services.AddScoped<ITokenRequestValidator<CdaTokenRequestModel>, GrantTypeNotPresentValidator>();
builder.Services.AddScoped<ITokenRequestValidator<CdaTokenRequestModel>, UnsupportedGrantTypeValidation>();
builder.Services.AddScoped<ITokenRequestValidator<CdaTokenRequestModel>, ClaimTokenNotPresentValidation>();
builder.Services.AddScoped<ITokenRequestValidator<CdaTokenRequestModel>, ClaimTokenNotJwtValidator>();
builder.Services.AddScoped<ITokenRequestValidator<CdaTokenRequestModel>, ClaimTokenFormatNotPresentValidator>();
builder.Services.AddScoped<ITokenRequestValidator<CdaTokenRequestModel>, ClaimTokenFormatNotPensionDashboardRqpValidator>();
builder.Services.AddScoped<ITokenRequestValidator<CdaTokenRequestModel>, ScopeNotOwnerValidator>();
builder.Services.AddScoped<ITokenRequestValidator<CdaTokenRequestModel>, ScopeNotPresentValidator>();
builder.Services.AddScoped<ITokenRequestValidator<CdaTokenRequestModel>, TicketNotAJweValidator>();
builder.Services.AddScoped<ITokenRequestValidator<CdaTokenRequestModel>, TicketQueryNotPresentValidator>();
builder.Services.AddScoped<ITokenRequestValidator<CdaTokenRequestModel>, ClientIdNotPresentValidation>();
builder.Services.AddScoped<ITokenRequestValidator<CdaTokenRequestModel>, ClientIdInvalidFormatValidation>();
builder.Services.AddScoped<ITokenRequestValidator<CdaTokenRequestModel>, ClientSecretNotGuidValidation>();
builder.Services.AddScoped<ITokenRequestValidator<CdaTokenRequestModel>, ClientSecretNotPresentValidation>();
builder.Services.AddScoped<ITokenRequestValidator<CdaTokenRequestModel>, CodeNotPresentValidation>();
builder.Services.AddScoped<ITokenRequestValidator<CdaTokenRequestModel>, CodeInvalidFormatValidation>();
builder.Services.AddScoped<ITokenRequestValidator<CdaTokenRequestModel>, CodeVerifierNotBase64String>();
builder.Services.AddScoped<ITokenRequestValidator<CdaTokenRequestModel>, CodeVerifierNotPresentValidation>();
builder.Services.AddScoped<ITokenRequestValidator<CdaTokenRequestModel>, RedirectUrlNotPresentValidation>();
builder.Services.AddScoped<ITokenRequestValidator<CdaTokenRequestModel>, RedirectUrlNotValidUrlValidation>();
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