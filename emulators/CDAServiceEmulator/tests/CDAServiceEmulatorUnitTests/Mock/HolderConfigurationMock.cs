using CDAServiceEmulator.Models.HolderConfiguration;

namespace CDAServiceEmulatorUnitTests.Mock;

public static class HolderConfigurationMock
{
    internal const string MatchingId = "550e8400-e29b-41d4-a716-446655440000";

    private static readonly List<HolderNameConfigurationModel> HolderConfigurations =
    [
        new HolderNameConfigurationModel
        {
            Id = Guid.NewGuid().ToString(),
            HolderNameGuid = "7075aa11-10ad-4b2f-a9f5-1068e79119bf",
            ViewDataUrl = "https://exampleprovider/pensiondataprovider/view-data"
        },
        new HolderNameConfigurationModel
        {
            Id = Guid.NewGuid().ToString(),
            HolderNameGuid = MatchingId,
            ViewDataUrl = "https://exampleprovider2/pensiondataprovider/view-data"
        }
    ];

    public static List<HolderNameConfigurationModel> GetHolderConfiguration()
    {
        return HolderConfigurations;
    }

    public static HolderNameConfigurationModel? FilterConfigurations(string holdernameGuid)
    {
        return HolderConfigurations
            .FirstOrDefault(config => config.HolderNameGuid == holdernameGuid);
    }
}

