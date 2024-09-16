using CDAServiceEmulator.Models.HolderConfiguration;

namespace CDAServiceEmulator.Mocks
{
    public static class HolderConfigurationMock
    {
        private static readonly List<HolderConfigurationModel> HolderConfigurations = new()
        {
            new HolderConfigurationModel
            {
                HolderNameGuid = "7075aa11-10ad-4b2f-a9f5-1068e79119bf",
                ViewDataUrl = "https://exampleprovider/pensiondataprovider/view-data"
            },
            new HolderConfigurationModel
            {
                HolderNameGuid = "550e8400-e29b-41d4-a716-446655440000",
                ViewDataUrl = "https://exampleprovider2/pensiondataprovider/view-data"
            }
        };

        public static Task<List<HolderConfigurationModel>> GetHolderConfiguration()
        {
            return Task.FromResult(HolderConfigurations);
        }

        public static List<HolderConfigurationModel> FilterConfigurations(string holdernameGuid)
        {
            return HolderConfigurations
                .Where(config => config.HolderNameGuid == holdernameGuid)
                .ToList();
        }
    }
}

