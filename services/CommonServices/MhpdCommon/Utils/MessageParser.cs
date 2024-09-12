using MhpdCommon.Constants;
using MhpdCommon.Models.MessageBodyModels;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System.Text.Json;

namespace MhpdCommon.Utils;

public class MessageParser : IMessageParser
{
    public RetrievedPensionDetailsPayload? ToRetrivedPensionRecord(string message)
    {
        var schemaData = ResourceProvider.GetString(FileConstants.RetrievedPensionPayloadSchema);

        JSchema schema = JSchema.Parse(schemaData);
        JObject json = JObject.Parse(message);

        if (!json.IsValid(schema, out IList<string> errors))
        {
            throw new AggregateException(errors.Select(error =>
            {
                return new Exception(error);
            }));
        }

        return JsonSerializer.Deserialize<RetrievedPensionDetailsPayload>(message);
    }
}
