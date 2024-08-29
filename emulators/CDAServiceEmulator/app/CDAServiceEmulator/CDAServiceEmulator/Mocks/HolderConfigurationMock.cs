namespace CDAServiceEmulator.Mocks
{
    public static class HolderConfigurationMock
    {
        public async static Task<string> GetHolderConfiguration ()
        {
            return await Task.FromResult("{\r\n  \"holder_configurations\": [\r\n    {\r\n      \"holdername_guid\": \"7075aa11-10ad-4b2f-a9f5-1068e79119bf\",\r\n      \"veiw_data_url\": \"https://local.exampleprovider/pensiondataprovider\"\r\n    },\r\n    {\r\n      \"holdername_guid\": \"550e8400-e29b-41d4-a716-446655440000\",\r\n      \"veiw_data_url\": \"https://local.exampleprovider2/pensiondataprovider\"\r\n    }\r\n  ]\r\n}");
        }
    }
}
