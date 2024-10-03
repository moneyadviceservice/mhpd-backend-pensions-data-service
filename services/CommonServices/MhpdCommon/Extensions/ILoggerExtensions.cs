using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MhpdCommon.Extensions;

public static class ILoggerExtensions
{
    public static void LogRequest<T,K>(this ILogger<T> logger, K request)
    {
        logger.LogInformation("Request Received: {RequestPayload}", JsonConvert.SerializeObject(request));
    }

    public static void LogResponse<T, K>(this ILogger<T> logger, K response)
    {
        logger.LogInformation("Response Sent: {Response}", JsonConvert.SerializeObject(response));
    }                
}