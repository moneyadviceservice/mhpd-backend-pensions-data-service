using MhpdCommon.Extensions;
using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.Models.MHPDModels;
using MhpdCommon.Repository;
using MhpdCommon.TokenValidation;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.OpenApi.Models;
using PensionsDataService.HttpClients;
using System.Diagnostics.CodeAnalysis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<PensionServiceClients>();
builder.Services.AddScoped<IRetrievalRecordServiceClient, RetrievalRecordServiceClient>();
builder.Services.AddScoped<IRetrievedPensionsRecordClient, RetrievedPensionsRecordClient>();

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
builder.Services.AddMhpdServiceBusTools();
builder.Services.AddMhpdHttpClients();
builder.Services.AddCommonConfigurations();
builder.Services.AddIntegrationServices();
builder.Services.AddMhpdUtilities();
builder.Services.AddMhpdCosmosDb(builder.Configuration);
builder.Services.AddAntiForgeryValidation();

builder.Services.AddApplicationInsightsTelemetry();

builder.Services.AddScoped<ICosmosDbRepository<UserSessionData>, UserSessionDataRepository>();

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
builder.Services.AddSwaggerGen(c =>
{
    c.AddServer(new OpenApiServer
    {
        Url = builder.Configuration.GetValue<string>("OpenApiServerUrl") ?? "https:\\localhost:3000"
    });
});
builder.Services.AddHttpClient(); 

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(c => c.OpenApiVersion = Microsoft.OpenApi.OpenApiSpecVersion.OpenApi2_0);
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.UseHttpLogging();
app.UseCsrfTokenEndpoint();

await app.RunAsync();

[ExcludeFromCodeCoverage]
public static partial class Program { }