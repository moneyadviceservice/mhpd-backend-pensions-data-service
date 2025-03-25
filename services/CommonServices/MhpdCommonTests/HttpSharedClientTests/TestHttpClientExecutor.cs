using MhpdCommon.SharedHttpClient;
using Microsoft.Extensions.Logging;

namespace MhpdCommonTests.HttpSharedClientTests;

public class TestHttpClientExecutor(
    IHttpClientFactory httpClientFactory,
    ILogger<BaseHttpClientExecutor> logger)
    : BaseHttpClientExecutor(httpClientFactory, logger)
{
    public Task<TResponse> TestExecuteAsync<TResponse>(string operationDescription)
    {
        return ExecuteAsync<TResponse>(
            "TestClient",
            client => new HttpRequestMessage(HttpMethod.Get, "/test"),
            operationDescription);
    }
}