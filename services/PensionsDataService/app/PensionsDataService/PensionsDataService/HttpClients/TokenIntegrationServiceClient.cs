using MhpdCommon.Constants;
using MhpdCommon.Constants.HttpClient;
using MhpdCommon.CustomExceptions;
using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.Models.RequestHeaderModel;
using PensionsDataService.Models;
using UrlHelper = MhpdCommon.Utils.UrlHelper;

namespace PensionsDataService.HttpClients;

public class TokenIntegrationServiceClient : ITokenIntegrationServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<TokenIntegrationServiceClient> _logger;
    
    public TokenIntegrationServiceClient(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<TokenIntegrationServiceClient> logger)
    {
        _logger = logger;
        
        _httpClient = httpClientFactory.CreateClient(HttpClientNames.TokenIntegrationService);
        
        // Set the base address for the client
        var endpoint = configuration[HttpClientUrlVariables.TokenIntegrationServiceUrl];
        if (string.IsNullOrEmpty(endpoint))
        {
            throw new InvalidOperationException("TokenIntegrationService endpoint is not configured.");
        }
        _httpClient.BaseAddress = new Uri(endpoint);
    }
    
    public async Task<PeiRetrievalDetailsResponseModel> PostAsync(CdaTokenRequestModel request, RequestHeaderModel requestHeader)
    {
        try
        {
            // Add request ID header
            _httpClient.DefaultRequestHeaders.Add(HeaderConstants.RequestId, requestHeader.XRequestId);

            // Construct the endpoint based on the request type
            var endpoint = UrlHelper.ConstructEndPoint(request, HttpEndpoints.Internal.PeiRetrievalDetails);

            // Send the request to the constructed endpoint
            var response = await _httpClient.PostAsync(endpoint, null);

            // Check if the response is successful
            response.EnsureSuccessStatusCode();

            // Attempt to read the response content
            var result = await response.Content.ReadFromJsonAsync<PeiRetrievalDetailsResponseModel>();
            return result ?? throw new InvalidOperationException("Response content was null.");
        }
        catch (HttpRequestException ex)
        {
            // Log the error and throw a more specific exception
            _logger.LogError(ex, "An HTTP request error occurred while calling the token integration service");
            throw new ServiceCommunicationException("Error communicating with token integration service", ex);
        }
        catch (InvalidOperationException ex)
        {
            // Log and handle specific invalid operation errors with contextual information
            _logger.LogError(ex, "Invalid operation: {Message}", ex.Message);
            throw new InvalidOperationException("An invalid operation occurred during token integration service communication", ex);
        }
        catch (Exception ex)
        {
            // Log any other exceptions with context, but do not throw a generic Exception
            _logger.LogError(ex, "An unexpected error occurred in PostRpt");
            throw new ServiceCommunicationException("An unexpected error occurred during token integration service communication.", ex);
        }
    }
}