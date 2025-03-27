using MhpdCommon.Models.MHPDModels;

namespace CDAServiceEmulatorUnitTests.Mock;

public static class HolderConfigurationMock
{
    internal const string MatchingId = "550e8400-e29b-41d4-a716-446655440000";

    private static readonly List<HolderNameViewDataResponse> HolderConfigurations =
    [
        new()
        {
            Id = Guid.NewGuid().ToString(),
            HolderNameGuid = "7075aa11-10ad-4b2f-a9f5-1068e79119bf",
            Configuration = new HolderNameConfigurationModel
            {
                ViewDataUrl = "https://exampleprovider/pensiondataprovider/view-data"
            }
        },
        new()
        {
            Id = Guid.NewGuid().ToString(),
            HolderNameGuid = MatchingId,
            Configuration = new HolderNameConfigurationModel
            {
                ViewDataUrl = "https://exampleprovider2/pensiondataprovider/view-data"
            }
        }
    ];

    public static List<HolderNameViewDataResponse> GetHolderConfiguration()
    {
        return HolderConfigurations;
    }

    public static HolderNameViewDataResponse? FilterConfigurations(string holdernameGuid)
    {
        return HolderConfigurations
            .FirstOrDefault(config => config.HolderNameGuid == holdernameGuid);
    }
}

