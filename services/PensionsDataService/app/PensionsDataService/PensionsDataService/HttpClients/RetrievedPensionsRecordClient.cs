using MhpdCommon.Constants.HttpClient;
using MhpdCommon.CustomExceptions;
using MhpdCommon.Models.MHPDModels;
using MhpdCommon.Utils;
using PensionsDataService.Models;

namespace PensionsDataService.HttpClients;

public class RetrievedPensionsRecordClient(IHttpClientFactory httpClientFactory, ILogger<RetrievedPensionsRecordClient> logger) : IRetrievedPensionsRecordClient
{
    public async Task<List<RetrievedPensionRecord>> GetAsync(PensionsRetrievalRecordIdModel request)
    {
        try
        {
            var httpClient = httpClientFactory.CreateClient(HttpClientNames.RetrievedPensionsService);
            
            // Send the request to the constructed endpoint
            var response = await httpClient.GetAsync(
                UrlHelper.ConstructEndPoint(request, 
                    HttpEndpoints.Internal.RetrievedPensionRecords));
            
            // Check if the response is successful
            response.EnsureSuccessStatusCode();
            
            // Read the response content as a string
            var content = await response.Content.ReadAsStringAsync();

            var result = new List<RetrievedPensionRecord>();
                
            // Check if the content is null, empty, or consists only of whitespace
            if (string.IsNullOrWhiteSpace(content))
            {
                logger.LogInformation("No retrieved pensions record for {Id}", request.PensionsRetrievalRecordId);
                return result;
            }
            
            result = await response.Content.ReadFromJsonAsync<List<RetrievedPensionRecord>>();
            return result ?? throw new InvalidOperationException("Response content was null.");
        }
        catch (HttpRequestException ex)
        {
            // Log the error and throw a more specific exception
            logger.LogError(ex, "An HTTP request error occurred while calling the retrieved record endpoint");
            throw new ServiceCommunicationException("Error communicating with retrieved record endpoint", ex);
        }
        catch (InvalidOperationException ex)
        {
            // Log and handle specific invalid operation errors with contextual information
            logger.LogError(ex, "Invalid operation: {Message}", ex.Message);
            throw new InvalidOperationException("An invalid operation occurred during retrieved record service communication", ex);
        }
        catch (Exception ex)
        {
            // Log any other exceptions with context, but do not throw a generic Exception
            logger.LogError(ex, "An unexpected error occurred in get retrieved-pension-records");
            throw new ServiceCommunicationException("An unexpected error occurred during retrieved record service communication", ex);
        }
    }
}