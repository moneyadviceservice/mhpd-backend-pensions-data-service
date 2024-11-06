using MhpdCommon.Constants;
using MhpdCommon.Constants.HttpClient;
using MhpdCommon.CustomExceptions;
using MhpdCommon.Extensions;
using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.Utils;
using TokenIntegrationService.Models;

namespace TokenIntegrationService.HttpClients;

public class CdaServiceClient(IHttpClientFactory httpClientFactory, ILogger<CdaServiceClient> logger) : ICdaServiceClient
{
    public async Task<CdaTokenResponseModel> PostAsync<TRequest>(TRequest request)
    {
        try
        {
            var httpClient = httpClientFactory.CreateClient(HttpClientNames.CdaService);
            var requestId = Guid.NewGuid().ToString();
            logger.LogWarning("Sending request to token endpoint with request Id: {requestId}", requestId);
            
            // Add request ID header
            httpClient.DefaultRequestHeaders.Add(HeaderConstants.RequestId, requestId);

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
            logger.LogResponse(result);
            return result ?? throw new InvalidOperationException("Response content was null.");
        }
        catch (HttpRequestException ex)
        {
            // Log the error and throw a more specific exception
            logger.LogError(ex, "An HTTP request error occurred while calling the CDA service");
            throw new ServiceCommunicationException("Error communicating with CDA service", ex);
        }
        catch (InvalidOperationException ex)
        {
            // Log and handle specific invalid operation errors with contextual information
            logger.LogError(ex, "Invalid operation: {Message}", ex.Message);
            throw new InvalidOperationException("An invalid operation occurred during CDA service communication", ex);
        }
        catch (Exception ex)
        {
            // Log any other exceptions with context, but do not throw a generic Exception
            logger.LogError(ex, "An unexpected error occurred in PostRpt");
            throw new ServiceCommunicationException("An unexpected error occurred during CDA service communication.", ex);
        }
    }
}