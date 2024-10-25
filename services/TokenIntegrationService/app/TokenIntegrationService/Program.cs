using System.Diagnostics.CodeAnalysis;
using MhpdCommon.Extensions;
using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.TokenValidation;
using Microsoft.AspNetCore.HttpLogging;
using TokenIntegrationService.HttpClients;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddMhpdUtilities();
builder.Services.AddMhpdHttpClients();
builder.Services.AddScoped<ICdaServiceClient, CdaServiceClient>();
builder.Services.AddScoped<ITokenRequestValidator<TokenIntegrationRequestModel>, RqpNotPresentValidator>();
builder.Services.AddScoped<ITokenRequestValidator<TokenIntegrationRequestModel>, RqpNotAJwtValidator>();
builder.Services.AddScoped<ITokenRequestValidator<TokenIntegrationRequestModel>, TicketNotPresentTokenIntegrationValidator>();
builder.Services.AddScoped<ITokenRequestValidator<TokenIntegrationRequestModel>, TicketNotAJwtTokenIntegrationValidator>();
builder.Services.AddScoped<ITokenRequestValidator<TokenIntegrationRequestModel>, AsUriNotPresentValidator>();
builder.Services.AddScoped<ITokenRequestValidator<TokenIntegrationRequestModel>, AsUriNotAUrlValidator>();
builder.Services.AddScoped<TokenIntegrationRequestValidatorPipeline>();

builder.Services.AddScoped<ITokenRequestValidator<CdaTokenRequestModel>, GrantTypeNotPresentValidator>();
builder.Services.AddScoped<ITokenRequestValidator<CdaTokenRequestModel>, UnsupportedGrantTypeValidation>();
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