using System.Diagnostics.CodeAnalysis;
using MhpdCommon.Constants.HttpClient;
using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.TokenValidation;
using MhpdCommon.Utils;
using Microsoft.AspNetCore.HttpLogging;
using TokenIntegrationService.HttpClients;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient<CdaServiceClient>(HttpClientNames.CdaService, c => {} );

builder.Services.AddScoped<ICdaServiceClient, CdaServiceClient>();
builder.Services.AddScoped<IIdValidator, IdValidator>();
builder.Services.AddScoped<ITokenUtility, TokenUtility>();
builder.Services.AddScoped<ITokenRequestValidator<TokenIntegrationRequestModel>, RqpNotPresentValidator>();
builder.Services.AddScoped<ITokenRequestValidator<TokenIntegrationRequestModel>, RqpNotAJwtValidator>();
builder.Services.AddScoped<ITokenRequestValidator<TokenIntegrationRequestModel>, TicketNotPresentTokenIntegrationValidator>();
builder.Services.AddScoped<ITokenRequestValidator<TokenIntegrationRequestModel>, TicketNotAJwtTokenIntegrationValidator>();
builder.Services.AddScoped<ITokenRequestValidator<TokenIntegrationRequestModel>, AsUriNotPresentValidator>();
builder.Services.AddScoped<ITokenRequestValidator<TokenIntegrationRequestModel>, AsUriNotAUrlValidator>();
builder.Services.AddScoped<TokenIntegrationRequestValidatorPipeline>();

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddHttpLogging(logging =>
{
    logging.LoggingFields = HttpLoggingFields.All;    
    logging.RequestBodyLogLimit = 4096;
    logging.ResponseBodyLogLimit = 4096;
    logging.CombineLogs = true;
});
var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseAuthorization();
app.MapControllers();
app.UseHttpLogging();
app.Run();

[ExcludeFromCodeCoverage]
public partial class Program { }