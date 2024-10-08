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

    private static string GenerateQueryString(List<Tuple<string, string>> queryList)
    {
        return "?" + string.Join("&", queryList.Select(q => $"{q.Item1}={q.Item2}"));
    }
}
