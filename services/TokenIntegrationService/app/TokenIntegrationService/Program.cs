using System.Diagnostics.CodeAnalysis;
using MhpdCommon.Extensions;
using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.TokenValidation;
using Microsoft.AspNetCore.HttpLogging;
using TokenIntegrationService.HttpClients;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddMhpdUtilities();
builder.Services.AddMhpdHttpClients();
builder.Services.AddMhpdCosmosDb();
builder.Services.AddIntegrationServices();
builder.Services.AddScoped<ICdaServiceClient, CdaServiceClient>();
builder.Services.AddScoped<ITokenRequestValidator<TokenClientRequestModel>, RqpNotPresentValidator>();
builder.Services.AddScoped<ITokenRequestValidator<TokenClientRequestModel>, RqpNotAJwtValidator>();
builder.Services.AddScoped<ITokenRequestValidator<TokenClientRequestModel>, TicketNotPresentTokenIntegrationValidator>();
builder.Services.AddScoped<ITokenRequestValidator<TokenClientRequestModel>, TicketNotAJweTokenIntegrationValidator>();
builder.Services.AddScoped<ITokenRequestValidator<TokenClientRequestModel>, AsUriNotPresentValidator>();
builder.Services.AddScoped<ITokenRequestValidator<TokenClientRequestModel>, AsUriNotAUrlValidator>();
builder.Services.AddScoped<TokenIntegrationRequestValidatorPipeline>();

builder.Services.AddScoped<ITokenRequestValidator<PensionsDataRequestModel>, AuthorisationCodeInvalidFormatValidationPensionsData>();
builder.Services.AddScoped<ITokenRequestValidator<PensionsDataRequestModel>, AuthorisationCodeNotPresentValidationPensionsData>();
builder.Services.AddScoped<ITokenRequestValidator<PensionsDataRequestModel>, RedirectUrlNotValidUrlValidationPensionsData>();
builder.Services.AddScoped<ITokenRequestValidator<PensionsDataRequestModel>, RedirectUrlNotPresentValidationPensionsData>();
builder.Services.AddScoped<ITokenRequestValidator<PensionsDataRequestModel>, CodeVerifierNotPresentValidationPensionsData>();
builder.Services.AddScoped<ITokenRequestValidator<PensionsDataRequestModel>, CodeVerifierNotBase64StringPensionsData>();
builder.Services.AddScoped<ITokenRequestValidator<PensionsDataRequestModel>, ClientIdInvalidFormatValidationPensionData>();
builder.Services.AddScoped<ITokenRequestValidator<PensionsDataRequestModel>, ClientIdNotPresentValidationPensionData>();
builder.Services.AddScoped<ITokenRequestValidator<PensionsDataRequestModel>, ClientSecretNotGuidValidationPensionData>();
builder.Services.AddScoped<ITokenRequestValidator<PensionsDataRequestModel>, ClientSecretNotPresentValidationPensionData>();
builder.Services.AddScoped<PensionsDataRequestValidatorPipeline>();

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddHttpLogging(logging =>
{
    logging.LoggingFields = HttpLoggingFields.All;    
    logging.RequestBodyLogLimit = 4096;
    logging.ResponseBodyLogLimit = 4096;
    logging.CombineLogs = true;
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Configure the HTTP request pipeline.
app.UseAuthorization();
app.MapControllers();
app.UseHttpLogging();
app.Run();

[ExcludeFromCodeCoverage]
public partial class Program { }