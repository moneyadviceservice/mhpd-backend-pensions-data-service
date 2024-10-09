using System.Security.Policy;
using MhpdCommon.Constants;
using MhpdCommon.Constants.HttpClient;
using MhpdCommon.CustomExceptions;
using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.Models.RequestHeaderModel;
using TokenIntegrationService.Models;
using UrlHelper = MhpdCommon.Utils.UrlHelper;

namespace TokenIntegrationService.HttpClients;

public class CdaServiceClient : ICdaServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CdaServiceClient> _logger;
    
    public CdaServiceClient(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<CdaServiceClient> logger)
    {
        _logger = logger;
        
        _httpClient = httpClientFactory.CreateClient(HttpClientNames.CdaService);
        
        // Set the base address for the client
        var endpoint = configuration[HttpClientUrlVariables.CdaServiceEndpoint];
        if (string.IsNullOrEmpty(endpoint))
        {
            throw new InvalidOperationException("CDA Service endpoint is not configured.");
        }
        _httpClient.BaseAddress = new Uri(endpoint);
    }
    
    public async Task<CdaTokenResponseModel> PostAsync<TRequest>(TRequest request, RequestHeaderModel requestHeader)
    {
        try
        {
            // Add request ID header
            _httpClient.DefaultRequestHeaders.Add(HeaderConstants.RequestId, requestHeader.XRequestId);

            // Construct the endpoint based on the request type
            var endpoint = request switch
            {
                TokenIntegrationRequestModel tokenRequest => UrlHelper.ConstructEndPoint(tokenRequest,
                    HttpEndpoints.CdaTokenServiceEndpoint),
                CdaTokenRequestModel cdaTokenRequest => UrlHelper.ConstructEndPoint(cdaTokenRequest,
                    HttpEndpoints.CdaTokenServiceEndpoint),
                _ => throw new InvalidOperationException("Unsupported request type.")
            };

            // Send the request to the constructed endpoint
            var response = await _httpClient.PostAsync(endpoint, null);

            // Check if the response is successful
            response.EnsureSuccessStatusCode();

            // Attempt to read the response content
            var result = await response.Content.ReadFromJsonAsync<CdaTokenResponseModel>();
            return result ?? throw new InvalidOperationException("Response content was null.");
        }
        catch (HttpRequestException ex)
        {
            // Log the error and throw a more specific exception
            _logger.LogError(ex, "An HTTP request error occurred while calling the CDA service");
            throw new ServiceCommunicationException("Error communicating with CDA service", ex);
        }
        catch (InvalidOperationException ex)
        {
            // Log and handle specific invalid operation errors with contextual information
            _logger.LogError(ex, "Invalid operation: {Message}", ex.Message);
            throw new InvalidOperationException("An invalid operation occurred during CDA service communication", ex);
        }
        catch (Exception ex)
        {
            // Log any other exceptions with context, but do not throw a generic Exception
            _logger.LogError(ex, "An unexpected error occurred in PostRpt");
            throw new ServiceCommunicationException("An unexpected error occurred during CDA service communication.", ex);
        }
    }
}