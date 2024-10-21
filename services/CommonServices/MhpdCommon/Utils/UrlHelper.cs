using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace MhpdCommon.Utils;

public static class UrlHelper
{
    public static string ConstructEndPoint<T>(T request, string endpointName) where T : class
    {
        ArgumentNullException.ThrowIfNull(request);

        var queryList = new List<Tuple<string, string>>();

        // Use reflection to iterate over properties and get the [FromQuery] attribute's name
        foreach (var property in typeof(T).GetProperties())
        {
            var fromQueryAttribute = property.GetCustomAttribute<FromQueryAttribute>();
            var queryParamName = fromQueryAttribute?.Name ?? property.Name; // Use the Name in FromQuery or default to property name

            var value = property.GetValue(request)?.ToString();
            if (!string.IsNullOrEmpty(value))
            {
                queryList.Add(new Tuple<string, string>(queryParamName, value));
            }
        }

        return endpointName + GenerateQueryString(queryList);
    }

    /// <summary>
    /// Builds a fully qualified path by combining the domain root with a relative path.
    /// The function handles trailing and leading slashes in both the root and the suffix path
    /// </summary>
    /// <param name="baseUrl">The domain root of the url. For example https://api.domain.com/</param>
    /// <param name="relativeUrl">The path to the resource you need. For example /basket/details or /employees?department=IT</param>
    /// <returns>A fully qualified url. For example https://api.domain.com/basket/details</returns>
    /// <exception cref="ArgumentNullException">Thrown if the domain root is null or empty spaces</exception>
    public static string ConstructPath(string? baseUrl, string? relativeUrl)
    {
        if (string.IsNullOrWhiteSpace(baseUrl))
            throw new ArgumentNullException(nameof(baseUrl));

        if (string.IsNullOrWhiteSpace(relativeUrl))
            return baseUrl;

        baseUrl = baseUrl.TrimEnd('/');
        relativeUrl = relativeUrl.TrimStart('/');

        return $"{baseUrl}/{relativeUrl}";
    }

    private static string GenerateQueryString(List<Tuple<string, string>> queryList)
    {
        return "?" + string.Join("&", queryList.Select(q => $"{q.Item1}={q.Item2}"));
    }
}
