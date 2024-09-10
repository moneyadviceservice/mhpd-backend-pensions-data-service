using System.Text.Json;
using CDAServiceEmulator.Models.Peis;

namespace CDAServiceEmulator.Mocks;

public static class PiesMockService
{
    public static async Task<PeiModel[]> GetPeisJsonMock()
    {
        string allText = await File.ReadAllTextAsync(@"./Mocks/peis-200-Ok.json");
        var peis = JsonSerializer.Deserialize<List<PeiModel>>(allText);

        return peis!.ToArray();
    }
}