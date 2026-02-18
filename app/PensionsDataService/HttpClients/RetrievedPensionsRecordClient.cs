using MhpdCommon.Constants;
using MhpdCommon.Constants.HttpClient;
using MhpdCommon.CustomExceptions;
using MhpdCommon.Extensions;
using MhpdCommon.Models.MHPDModels;
using MhpdCommon.Models.RequestHeaderModel;
using MhpdCommon.Utils;
using PensionsDataService.Models;

namespace PensionsDataService.HttpClients;

public class RetrievedPensionsRecordClient(IHttpClientFactory httpClientFactory, ILogger<RetrievedPensionsRecordClient> logger) : IRetrievedPensionsRecordClient
{
    public async Task DeleteAsync(string userSessionId, string correlationId)
    {
        logger.LogWarning("Sending request to delete retrieved pension records for session {Session}", correlationId);

        var httpClient = httpClientFactory.CreateClient(HttpClientNames.RetrievedPensionsService);
        httpClient.DefaultRequestHeaders.Add(HeaderConstants.CorrelationId, correlationId);
        httpClient.DefaultRequestHeaders.Add(HeaderConstants.UserSessionId, userSessionId);

        // Send the request to the constructed endpoint
        var response = await httpClient.DeleteAsync(HttpEndpoints.Internal.RetrievedPensionRecords);

        response.EnsureSuccessStatusCode();
    }

    public async Task<List<RetrievedPensionRecord>> GetRetrievedPensionsAsync(RetrievedPensionsRequest request, RequestHeaderModel requestHeader)
    {
        return await GetAsync<List<RetrievedPensionRecord>>(request, requestHeader, HttpEndpoints.Internal.RetrievedPensionRecords);
    }

    public async Task<List<string>> GetRetrievedPeisAsync(RequestHeaderModel requestHeader)
    {
        return await GetAsync<List<string>>(new RetrievedPensionsRequest(), requestHeader, HttpEndpoints.Internal.RetrievedPeis);
    }

    private async Task<T> GetAsync<T>(RetrievedPensionsRequest request, RequestHeaderModel requestHeader, string endpoint) where T : class, new()
    {
        try
        {
            logger.LogRequestReceived(request);
            logger.LogRequestReceived(requestHeader);
            var httpClient = httpClientFactory.CreateClient(HttpClientNames.RetrievedPensionsService);
            httpClient.DefaultRequestHeaders.Add(HeaderConstants.CorrelationId, requestHeader.CorrelationId);
            httpClient.DefaultRequestHeaders.Add(HeaderConstants.UserSessionId, requestHeader.UserSessionId);

            var path = endpoint;

            if (!string.IsNullOrWhiteSpace(request.AssetId) ||
                !string.IsNullOrWhiteSpace(request.Category))
            {
                path = UrlHelper.ConstructEndPoint(request, endpoint);
            }

            // Send the request to the constructed endpoint
            var response = await httpClient.GetAsync(path);

            response.EnsureSuccessStatusCode();

            // Read the response content as a string
            var content = await response.Content.ReadAsStringAsync();

            var result = new T();

            // Check if the content is null, empty, or consists only of whitespace
            if (string.IsNullOrWhiteSpace(content))
            {
                logger.LogWarning("No retrieved pensions record for session {Id}", requestHeader.UserSessionId);
                return result;
            }

            result = await response.Content.ReadFromJsonAsync<T>();
            logger.LogResponseSent(result);
            return result ?? throw new InvalidOperationException("Response content was null.");
        }
        catch (HttpRequestException ex)
        {
            // Log the error and throw a more specific exception
            logger.LogError(ex, "An HTTP request error occurred while calling the {Endpoint} endpoint", endpoint);
            throw new ServiceCommunicationException("Error communicating with retrieved record endpoint", ex);
        }
        catch (InvalidOperationException ex)
        {
            // Log and handle specific invalid operation errors with contextual information
            logger.LogError(ex, "Invalid operation: {Message}", ex.Message);
            throw new InvalidOperationException("An invalid operation occurred during retrieved pensions service communication", ex);
        }
        catch (Exception ex)
        {
            // Log any other exceptions with context, but do not throw a generic Exception
            logger.LogError(ex, "An unexpected error occurred in get {Endpoint}", endpoint);
            throw new ServiceCommunicationException("An unexpected error occurred during retrieved pensions service communication", ex);
        }
    }
}