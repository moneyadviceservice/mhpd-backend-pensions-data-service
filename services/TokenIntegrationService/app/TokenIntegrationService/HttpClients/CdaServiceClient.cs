using MhpdCommon.Constants;
using MhpdCommon.Constants.HttpClient;
using MhpdCommon.CustomExceptions;
using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.Models.RequestHeaderModel;
using MhpdCommon.Utils;
using TokenIntegrationService.Models;

namespace TokenIntegrationService.HttpClients;

public class CdaServiceClient(IHttpClientFactory httpClientFactory, ILogger<CdaServiceClient> logger) : ICdaServiceClient
{
    private readonly ILogger<CdaServiceClient> _logger = logger;

    public async Task<CdaTokenResponseModel> PostAsync<TRequest>(TRequest request, RequestHeaderModel requestHeader)
    {
        try
        {
            var httpClient = httpClientFactory.CreateClient(HttpClientNames.CdaService);
            // Add request ID header
            httpClient.DefaultRequestHeaders.Add(HeaderConstants.RequestId, requestHeader.XRequestId);

            // Construct the endpoint based on the request type
            var endpoint = request switch
            {
                TokenIntegrationRequestModel tokenRequest => UrlHelper.ConstructEndPoint(tokenRequest,
                    HttpEndpoints.External.CdaTokenServiceEndpoint),
                CdaTokenRequestModel cdaTokenRequest => UrlHelper.ConstructEndPoint(cdaTokenRequest,
                    HttpEndpoints.External.CdaTokenServiceEndpoint),
                _ => throw new InvalidOperationException("Unsupported request type.")
            };

            // Send the request to the constructed endpoint
            var response = await httpClient.PostAsync(endpoint, null);

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