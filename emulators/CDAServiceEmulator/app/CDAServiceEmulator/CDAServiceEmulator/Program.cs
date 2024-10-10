using System.Diagnostics.CodeAnalysis;
using CDAServiceEmulator.Configuration;
using CDAServiceEmulator.CosmosRepository;
using CDAServiceEmulator.Models.HolderConfiguration;
using MhpdCommon.Models.Configuration;
using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.TokenValidation;
using MhpdCommon.Utils;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddTransient<IIdValidator, IdValidator>();
builder.Services.AddTransient<ITokenUtility, TokenUtility>();
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

// Register HolderNameViewDataRepository
builder.Services.AddSingleton<IHolderNameViewDataRepository<HolderNameConfigurationModel>>(provider =>
{
    var cosmosClient = provider.GetRequiredService<CosmosClient>();
    var config = provider.GetRequiredService<IOptions<MhpdCosmosConfiguration>>().Value;

    return new HolderNameViewDataRepository(cosmosClient, config.DatabaseName, config.HolderNameConfigurationModelsContainerName);
});

builder.Services.AddScoped<ITokenRequestValidator<CdaTokenRequestModel>, GrantTypeNotPresentValidator>();
builder.Services.AddScoped<ITokenRequestValidator<CdaTokenRequestModel>, UnsupportedGrantTypeValidation>();
builder.Services.AddScoped<ITokenRequestValidator<CdaTokenRequestModel>, ClaimTokenNotPresentValidation>();
builder.Services.AddScoped<ITokenRequestValidator<CdaTokenRequestModel>, ClaimTokenNotJwtValidator>();
builder.Services.AddScoped<ITokenRequestValidator<CdaTokenRequestModel>, ClaimTokenFormatNotPresentValidator>();
builder.Services.AddScoped<ITokenRequestValidator<CdaTokenRequestModel>, ClaimTokenFormatNotPensionDashboardRqpValidator>();
builder.Services.AddScoped<ITokenRequestValidator<CdaTokenRequestModel>, ScopeNotOwnerValidator>();
builder.Services.AddScoped<ITokenRequestValidator<CdaTokenRequestModel>, ScopeNotPresentValidator>();
builder.Services.AddScoped<ITokenRequestValidator<CdaTokenRequestModel>, TicketNotAJwtValidator>();
builder.Services.AddScoped<ITokenRequestValidator<CdaTokenRequestModel>, TicketQueryNotPresentValidator>();
builder.Services.AddScoped<ITokenRequestValidator<CdaTokenRequestModel>, ClientIdNotPresentValidation>();
builder.Services.AddScoped<ITokenRequestValidator<CdaTokenRequestModel>, ClientIdInvalidFormatValidation>();
builder.Services.AddScoped<ITokenRequestValidator<CdaTokenRequestModel>, ClientSecretNotGuidValidation>();
builder.Services.AddScoped<ITokenRequestValidator<CdaTokenRequestModel>, ClientSecretNotPresentValidation>();
builder.Services.AddScoped<ITokenRequestValidator<CdaTokenRequestModel>, CodeNotPresentValidation>();
builder.Services.AddScoped<ITokenRequestValidator<CdaTokenRequestModel>, CodeInvalidFormatValidation>();
builder.Services.AddScoped<ITokenRequestValidator<CdaTokenRequestModel>, CodeVerifierNotBase64String>();
builder.Services.AddScoped<ITokenRequestValidator<CdaTokenRequestModel>, CodeVerifierNotPresentValidation>();
builder.Services.AddScoped<ITokenRequestValidator<CdaTokenRequestModel>, RedirectUriNotPresentValidation>();
builder.Services.AddScoped<ITokenRequestValidator<CdaTokenRequestModel>, RedirectUriNotValidUrlValidation>();
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
builder.Services.AddScoped<TokenUtility>();

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