using MhpdCommon.Constants;
using MhpdCommon.Constants.HttpClient;
using MhpdCommon.CustomExceptions;
using MhpdCommon.Extensions;
using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.Models.MHPDModels;
using MhpdCommon.Models.RequestHeaderModel;

namespace PensionsDataService.HttpClients;

public class RetrievalRecordServiceClient(IHttpClientFactory httpClientFactory, ILogger<RetrievalRecordServiceClient> logger) : IRetrievalRecordServiceClient
{
    public async Task DeleteAsync(string userSessionId, string correlationId)
    {
        logger.LogWarning("Sending request to delete pensions retrieval records for session {Session}", userSessionId);

        var httpClient = httpClientFactory.CreateClient(HttpClientNames.PensionRetrievalService);

        // Add request headers
        httpClient.DefaultRequestHeaders.Add(HeaderConstants.UserSessionId, userSessionId);
        httpClient.DefaultRequestHeaders.Add(HeaderConstants.CorrelationId, correlationId);

        // Send the request to the constructed endpoint
        var response = await httpClient.DeleteAsync(HttpEndpoints.Internal.PensionsRetrievalRecords);

        // Check if the response is successful
        response.EnsureSuccessStatusCode();
    }

    public Task<PensionsRetrievalRecord> PostAsync(RequestHeaderModel requestHeader, PensionRetrievalPayload payload)
    {
        return HandleHttpResponseResultAsync<PensionsRetrievalRecord>(requestHeader, httpClient => httpClient.PostAsJsonAsync(HttpEndpoints.Internal.PensionsRetrievalRecords, payload));
    }

    public Task<PensionsRetrievalRecord> GetAsync(RequestHeaderModel requestHeader)
    {
        return HandleHttpResponseResultAsync<PensionsRetrievalRecord>(requestHeader, httpClient => httpClient.GetAsync(HttpEndpoints.Internal.PensionsRetrievalRecords));
    }

    private async Task<T> HandleHttpResponseResultAsync<T>(RequestHeaderModel requestHeader, Func<HttpClient, Task<HttpResponseMessage>> ClientMethod)
        where T : new()
    {
        try
        {
            logger.LogRequestSent(requestHeader);
            var httpClient = httpClientFactory.CreateClient(HttpClientNames.PensionRetrievalService);
            httpClient.DefaultRequestHeaders.Add(HeaderConstants.UserSessionId, requestHeader.UserSessionId);
            httpClient.DefaultRequestHeaders.Add(HeaderConstants.CorrelationId, requestHeader.CorrelationId);

            var response = await ClientMethod(httpClient);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var result = new T();
            if (string.IsNullOrWhiteSpace(content))
            {
                logger.LogWarning("No pension data for {UserSessionId}", requestHeader.UserSessionId);
                return result;
            }

            result = await response.Content.ReadFromJsonAsync<T>();
            logger.LogResponseReceived(result);

            return result ?? throw new InvalidOperationException("Response content was null.");
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "An HTTP request error occurred while calling the retrieval record endpoint");
            throw new ServiceCommunicationException("Error communicating with retrieval record endpoint", ex);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError(ex, "Invalid operation: {Message}", ex.Message);
            throw new InvalidOperationException("An invalid operation occurred during retrieval record function communication", ex);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred in get pensions-retrieval-records");
            throw new ServiceCommunicationException("An unexpected error occurred during retrieval record function communication", ex);
        }
    }
}
