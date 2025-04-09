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
        Func<HttpClient, HttpRequestMessage> createRequestFunc,
        string operationDescription,
        Action<HttpRequestMessage>? configureRequest = null)
    {
        try
        {
            var response = await ExecuteAsync(httpClientName, operationDescription, createRequestFunc, configureRequest);
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
        catch (ServiceCommunicationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred during {OperationDescription}", operationDescription);
            throw new ServiceCommunicationException($"An unexpected error occurred during {operationDescription}.", ex);
        }
    }

    protected async Task<HttpResponseMessage> ExecuteAsync(
        string httpClientName,
        string operationDescription,
        Func<HttpClient, HttpRequestMessage> createRequestFunc,
        Action<HttpRequestMessage>? configureRequest = null)
    {
        try
        {
            var httpClient = httpClientFactory.CreateClient(httpClientName);
            logger.LogWarning("Sending request for operation: {OperationDescription}", operationDescription);

            var request = createRequestFunc(httpClient);

            // Apply optional header modifications
            configureRequest?.Invoke(request);

            return await httpClient.SendAsync(request);
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "An HTTP request error occurred during {OperationDescription}", operationDescription);
            throw new ServiceCommunicationException($"Error during {operationDescription}", ex);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred during {OperationDescription}", operationDescription);
            throw new ServiceCommunicationException($"An unexpected error occurred during {operationDescription}.", ex);
        }
    }
}