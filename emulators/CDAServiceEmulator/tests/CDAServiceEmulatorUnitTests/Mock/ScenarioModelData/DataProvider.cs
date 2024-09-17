using System.Reflection;
using Newtonsoft.Json;

namespace CDAServiceEmulatorUnitTests.Mock.ScenarioModelData;

internal static class DataProvider
{
    private static Stream GetStream(string resourceName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var stream = assembly.GetManifestResourceStream($"{typeof(DataProvider).Namespace}.{resourceName}");
        return stream ?? throw new InvalidOperationException($"Resource '{resourceName}' not found.");
    }

    internal static string GetString(string resourceName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        using Stream? stream = assembly.GetManifestResourceStream($"{typeof(DataProvider).Namespace}.{resourceName}") ??
                               throw new InvalidOperationException($"Resource '{resourceName}' not found.");
        using StreamReader reader = new(stream);
        return reader.ReadToEnd();
    }

    internal static T? GetPayload<T>(string payloadFile)
    {
        using Stream stream = GetStream(payloadFile);
        using StreamReader reader = new(stream);

        string jsonContent = reader.ReadToEnd();

        // Deserialize using Newtonsoft.Json
        return JsonConvert.DeserializeObject<T>(jsonContent);
    }
}