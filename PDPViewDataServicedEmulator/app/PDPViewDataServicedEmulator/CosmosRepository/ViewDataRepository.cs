using PDPViewDataServicedEmulator.Mocks;

namespace PDPViewDataServicedEmulator.CosmosRepository
{
    public class ViewDataRepository : IViewDataRepository
    {
        public ViewDataPayload GetViewData(string assetGuid)
        {
            return new ViewDataPayload
            {
                AssetGuid = "1ba03e25-659a-43b8-ae77-b956df168969",
                ViewData = "{\r\n\t\"arrangements\": [\r\n\t\t{\r\n\t\t\t\"pensionProviderSchemeName\": \"My Company Direct Contribution Scheme\",\r\n\t\t\t\"alternateSchemeName\": {\r\n\t\t\t\t\"name\": \"Converted from My Old Direct Contribution Scheme\",\r\n\t\t\t\t\"alternateNameType\": \"FOR\"\r\n\t\t\t},\r\n\t\t\t\"possibleMatch\": true,\r\n\t\t\t\"possibleMatchReference\": \"Q12345\",\r\n\t\t\t\"pensionAdministrator\": {\r\n\t\t\t\t\"name\": \"Pension Company 1\",\r\n\t\t\t\t\"contactMethods\": [\r\n\t\t\t\t\t{\r\n\t\t\t\t\t\t\"preferred\": false,\r\n\t\t\t\t\t\t\"contactMethodDetails\": {\r\n\t\t\t\t\t\t\t\"email\": \"example@examplemyline.com\"\r\n\t\t\t\t\t\t}\r\n\t\t\t\t\t},\r\n\t\t\t\t\t{\r\n\t\t\t\t\t\t\"preferred\": true,\r\n\t\t\t\t\t\t\"contactMethodDetails\": {\r\n\t\t\t\t\t\t\t\"number\": \"+123 1111111111\",\r\n\t\t\t\t\t\t\t\"usage\": [\r\n\t\t\t\t\t\t\t\t\"A\",\r\n\t\t\t\t\t\t\t\t\"M\"\r\n\t\t\t\t\t\t\t]\r\n\t\t\t\t\t\t}\r\n\t\t\t\t\t}\r\n\t\t\t\t]\r\n\t\t\t}\r\n\t\t}\r\n\t]\r\n}"
            };
        }
    }
}