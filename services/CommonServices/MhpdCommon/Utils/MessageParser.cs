using MhpdCommon.Constants;
using MhpdCommon.Models.MessageBodyModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

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

    public ViewDataPayload? ToViewDataPayload(string message)
    {
        return Parse<ViewDataPayload>(message, FileConstants.ViewDataPayloadSchema);
    }

    private static TPayload? Parse<TPayload>(string messageContent, string schemaName)
    {
        var schemaData = ResourceProvider.GetString(schemaName);

        JSchema schema = JSchema.Parse(schemaData);

        List<string> errors = [];

        using (JsonTextReader reader = new(new StringReader(messageContent)))
        {
            JSchemaValidatingReader validatingReader = new(reader)
            {
                Schema = schema
            };

            validatingReader.ValidationEventHandler += (sender, args) =>
            {
                UnwrapNestedErrors(args.ValidationError, errors);
            };

            using JsonReader jsonReader = new JsonTextReader(new StringReader(messageContent));
            JObject.Load(validatingReader);
        }

        if (errors.Count > 0)
        {
            throw new AggregateException(errors.Distinct().Select(error => new Exception(error)));
        }

        return System.Text.Json.JsonSerializer.Deserialize<TPayload>(messageContent);
    }

    static void UnwrapNestedErrors(ValidationError error, List<string> errorList, string parentPath = "")
    {
        string errorPath = string.IsNullOrEmpty(error.Path) ? "root" : error.Path;
        string errorMessage = error.Message;

        // Get the actual name of failed schemas
        if (errorMessage.Contains("$ref"))
        {
            errorMessage = errorMessage.Replace("$ref", error.Schema!.Ref!.Title);
        }

        // Handle 'oneOf' validation errors for schema branches
        if (errorMessage.Contains("oneOf"))
        {
            errorMessage += $" (Possible expected schemas: {string.Join(", ", error.Schema.OneOf.Select(one => one.Title))})";
        }

        if (!string.IsNullOrEmpty(parentPath))
        {
            errorPath = $"{parentPath}.{errorPath}";
        }

        errorList.Add($"Error at {errorPath}: {errorMessage}");

        // Recursively process all child errors
        foreach (var childError in error.ChildErrors)
        {
            UnwrapNestedErrors(childError, errorList, errorPath);
        }
    }
}
