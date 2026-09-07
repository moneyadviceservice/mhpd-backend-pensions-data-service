using MhpdCommon.Constants;
using MhpdCommon.ErrorHandling;
using MhpdCommon.Extensions;
using MhpdCommon.Models.RequestHeaderModel;
using MhpdCommon.SharedHttpClient;

namespace PensionsDataService.HttpClients;

public abstract class PensionDataClient(
    IHttpClientFactory httpClientFactory,
    IErrorResolver errorResolver,
    ILogger<PensionDataClient> logger) : BaseHttpClientExecutor(httpClientFactory, errorResolver, logger)
{
    protected abstract string ClientName { get; }

    protected async Task<T> ExecuteAsync<T>(RequestHeaderModel requestHeader, Func<HttpRequestMessage> requestDelegate, string clientName, string clientAction) where T : new()
    {
        var response = await ExecuteAsync(requestHeader, requestDelegate, clientName, clientAction);

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

    protected async Task<HttpResponseMessage> ExecuteAsync(RequestHeaderModel requestHeader, Func<HttpRequestMessage> requestDelegate, string clientName, string clientAction)
    {
        logger.LogRequestSent(requestHeader);

        var response = await ExecuteAsync(clientName, $"{ClientName} - {clientAction}",
            _ => requestDelegate(),
            message =>
            {
                message.Headers.Add(HeaderConstants.UserSessionId, requestHeader.UserSessionId);
                message.Headers.Add(HeaderConstants.CorrelationId, requestHeader.CorrelationId);
            });

        response.EnsureSuccessStatusCode();

        return response;
    }
}
