using System.Net.Http.Json;
using MhpdCommon.CustomExceptions;
using MhpdCommon.Extensions;
using Microsoft.Extensions.Logging;

namespace MhpdCommon.SharedHttpClient;

public abstract class BaseHttpClientExecutor(
    IHttpClientFactory httpClientFactory,
    ILogger<BaseHttpClientExecutor> logger)
{
    protected async Task<TResponse> ExecuteAsync<TResponse>(
        string httpClientName,
        Func<HttpClient, Task<HttpResponseMessage>> httpRequestFunc,
        string operationDescription)
    {
        try
        {
            var httpClient = httpClientFactory.CreateClient(httpClientName);
            logger.LogWarning("Sending request for operation: {OperationDescription}", operationDescription);

            var response = await httpRequestFunc(httpClient);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<TResponse>();
            logger.LogResponse(result);

            return result ?? throw new InvalidOperationException("Response content was null.");
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "An HTTP request error occurred during {OperationDescription}", operationDescription);
            throw new ServiceCommunicationException($"Error during {operationDescription}", ex);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError(ex, "Invalid operation during {OperationDescription}: {Message}", operationDescription, ex.Message);
            throw new InvalidOperationException($"An invalid operation occurred during {operationDescription}", ex);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred during {OperationDescription}", operationDescription);
            throw new ServiceCommunicationException($"An unexpected error occurred during {operationDescription}.", ex);
        }
    }
}