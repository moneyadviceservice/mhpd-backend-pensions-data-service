using MhpdCommon.Models.MessageBodyModels;

namespace MhpdCommon.Utils;

public interface IMessageParser
{
    /// <summary>
    /// Attempts to parse the message body from a queue to a pension record.
    /// The message is validated against the schema for a pension record.
    /// </summary>
    /// <param name="message">The queue message to parse</param>
    /// <returns>A retrieved pension instance</returns>
    /// <exception cref="AggregateException">Thrown if the message is not in compliance with the schema</exception>
    RetrievedPensionDetailsPayload? ToRetrivedPensionRecord(string message);
}
