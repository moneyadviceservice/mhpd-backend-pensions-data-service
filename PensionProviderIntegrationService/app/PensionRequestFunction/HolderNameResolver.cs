
namespace PensionRequestFunction
{
    public static class HolderNameResolver
    {
        private const string ViewDataUrl = "https://pdpviewdataservicedemulator.azurewebsites.net/view-data/";

        public static string GetViewDataUrl(string holderNameGuid )
        {
            return ViewDataUrl;
        }
    }
}
