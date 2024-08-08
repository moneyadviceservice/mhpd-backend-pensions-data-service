
namespace PensionRequestFunction
{
    public static class HolderNameResolver
    {
        public static string GetViewDataUrl(string holderNameGuid )
        {
            return Environment.GetEnvironmentVariable("PdpViewDataUrl")!;
        }
    }
}

