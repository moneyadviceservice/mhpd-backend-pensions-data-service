namespace MhpdCommon.Extensions;

public static class HttpResponseMessageExtensions
{
    public static string? GetResponseHeader(this HttpResponseMessage response, string headerKey)
    {
        if (response.Headers.TryGetValues(headerKey, out var headerValues))
        {
            return headerValues.FirstOrDefault();
        }

        return null;
    }
}
