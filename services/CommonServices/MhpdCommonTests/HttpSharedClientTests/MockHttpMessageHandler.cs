using System.Net;
using System.Net.Http.Json;

namespace MhpdCommonTests.HttpSharedClientTests;

public class MockHttpMessageHandler : DelegatingHandler
{
    private HttpResponseMessage _mockResponse;
    public void SetupResponse(HttpStatusCode statusCode, object? content = null)
    {
        _mockResponse = new HttpResponseMessage(statusCode)
        {
            Content = content != null
                ? JsonContent.Create(content)
                : null
        };
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return Task.FromResult(_mockResponse);
    }
}

public class ApiResponse
{
    public string Message { get; set; }
}