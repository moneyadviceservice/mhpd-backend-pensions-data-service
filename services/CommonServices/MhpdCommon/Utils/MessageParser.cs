using MhpdCommon.Constants;
using MhpdCommon.Models.MessageBodyModels;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System.Text.Json;

namespace MhpdCommon.Utils;

public class MessageParser : IMessageParser
{
    public RetrievedPensionDetailsPayload? ToRetrievedPensionPayload(string message)
    {
        return Parse<RetrievedPensionDetailsPayload>(message, FileConstants.RetrievedPensionPayloadSchema);
    }

    public PensionRetrievalPayload? ToPensionRetrievalPayload(string message)
    {
        return Parse<PensionRetrievalPayload>(message, FileConstants.PensionsRetrievalPayloadSchema);
    }

    public PensionRequestPayload? ToPensionRequestPayload(string message)
    {
        return Parse<PensionRequestPayload>(message, FileConstants.PensionDetailsRequestPayloadSchema);
    }

    private static TPayload? Parse<TPayload>(string messageContent, string schemaName)
    {
        var schemaData = ResourceProvider.GetString(schemaName);

        JSchema schema = JSchema.Parse(schemaData);
        JObject json = JObject.Parse(messageContent);

        if (!json.IsValid(schema, out IList<string> errors))
        {
            throw new AggregateException(errors.Select(error =>
            {
                return new Exception(error);
            }));
        }

        return JsonSerializer.Deserialize<TPayload>(messageContent);
    }
}
