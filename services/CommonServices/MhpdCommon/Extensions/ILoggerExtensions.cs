using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MhpdCommon.Extensions;

public static class ILoggerExtensions
{
    public const string CorrelationId = "CorrelationId";
    public const string Source = "MhpdSource";

    public static void LogRequest<T,K>(this ILogger<T> logger, K request)
    {
        logger.LogWarning("Request Received: {RequestPayload}", JsonConvert.SerializeObject(request));
    }

    public static void LogResponse<T, K>(this ILogger<T> logger, K response)
    {
        logger.LogWarning("Response Sent: {Response}", JsonConvert.SerializeObject(response));
    }

    public static IDisposable? BeginCorrelationScope(this ILogger logger, string correlationId, string source)
    {
        return logger.BeginScope(new Dictionary<string, object> 
        { 
            { CorrelationId, correlationId },
            { Source, source }
        });
    }
}