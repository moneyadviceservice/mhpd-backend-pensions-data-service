using MhpdCommon.Constants;
using MhpdCommon.Constants.HttpClient;
using MhpdCommon.CustomExceptions;
using MhpdCommon.Models.MHPDModels;
using MhpdCommon.Models.RequestHeaderModel;

namespace PensionsDataService.HttpClients;

public class RetrievalRecordServiceClient(IHttpClientFactory httpClientFactory, ILogger<RetrievalRecordServiceClient> logger) : IRetrievalRecordServiceClient
{
    public async Task<PensionsRetrievalRecord> GetAsync(RequestHeaderModel requestHeader)
    {
        try
        {
            var httpClient = httpClientFactory.CreateClient(HttpClientNames.PensionRetrievalService);
            
            // Add request ID header
            httpClient.DefaultRequestHeaders.Add(HeaderConstants.UserSessionId, requestHeader.UserSessionId);
            
            // Send the request to the constructed endpoint
            var response = await httpClient.GetAsync(HttpEndpoints.Internal.PensionsRetrievalRecords);

            // Check if the response is successful
            response.EnsureSuccessStatusCode();
            
            // Read the response content as a string
            var content = await response.Content.ReadAsStringAsync();

            var result = new PensionsRetrievalRecord();
                
            // Check if the content is null, empty, or consists only of whitespace
            if (string.IsNullOrWhiteSpace(content))
            {
                logger.LogInformation("No pension data for {UserSessionId}", requestHeader.UserSessionId);
                return result;
            }
            
            // Attempt to read the response content
            result = await response.Content.ReadFromJsonAsync<PensionsRetrievalRecord>();
            return result ?? throw new InvalidOperationException("Response content was null.");
        }
        catch (HttpRequestException ex)
        {
            // Log the error and throw a more specific exception
            logger.LogError(ex, "An HTTP request error occurred while calling the retrieval record endpoint");
            throw new ServiceCommunicationException("Error communicating with retrieval record endpoint", ex);
        }
        catch (InvalidOperationException ex)
        {
            // Log and handle specific invalid operation errors with contextual information
            logger.LogError(ex, "Invalid operation: {Message}", ex.Message);
            throw new InvalidOperationException("An invalid operation occurred during retrieval record function communication", ex);
        }
        catch (Exception ex)
        {
            // Log any other exceptions with context, but do not throw a generic Exception
            logger.LogError(ex, "An unexpected error occurred in get pensions-retrieval-records");
            throw new ServiceCommunicationException("An unexpected error occurred during retrieval record function communication", ex);
        }
    }
}
