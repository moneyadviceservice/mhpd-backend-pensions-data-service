using System.Reflection;

namespace MhpdCommon.Utils;

public static class ResourceProvider
{
    public static Stream GetStream(string resourceName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var stream = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.{resourceName}");
        return stream ?? throw new InvalidOperationException($"Resource '{resourceName}' not found.");
    }

    public static string GetString(string resourceName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        using Stream? stream = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.{resourceName}") ??
            throw new InvalidOperationException($"Resource '{resourceName}' not found.");
        using StreamReader reader = new(stream);
        return reader.ReadToEnd();
    }
}
