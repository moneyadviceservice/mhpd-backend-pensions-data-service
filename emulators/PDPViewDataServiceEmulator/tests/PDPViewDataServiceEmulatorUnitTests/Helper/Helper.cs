using Newtonsoft.Json.Linq;

namespace PDPViewDataServiceEmulatorUnitTests;

public class Helper
{
    public static JObject ReadViewDataPayloadFile(string fileName)
    {
        var jsonContent = File.ReadAllText(fileName);
        return JObject.Parse(jsonContent);;
    }
}