using System.Text.Json;
using PeIsServiceEmulator.Models.Peis;

namespace PeIsServiceEmulator.Mocks
{
    public static class PiesMockService
    {
        public static async Task<PeiModel[]> GetPeisJsonMock()
        {
            string allText = await File.ReadAllTextAsync(@"./Mocks/peis-200-Ok.json");
            var peis = JsonSerializer.Deserialize<List<PeiModel>>(allText);

            return peis!.ToArray();
        }
    }
}
