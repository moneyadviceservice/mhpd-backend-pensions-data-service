using MhpdCommon.Models.MessageBodyModels;

namespace MhpdCommon.Utils;

public interface IMessageParser
{
    /// <summary>
    /// Attempts to parse the message body from a queue to a retrieved pension object.<br/>
    /// The message is validated against the schema for a retrieved pension message.<br/>
    /// </summary>
    /// <param name="message">The queue message to parse</param>
    /// <returns>A retrieved pension instance</returns>
    /// <exception cref="AggregateException">Thrown if the message is not in compliance with the schema</exception>
    RetrievedPensionDetailsPayload? ToRetrievedPensionPayload(string message);

    /// <summary>
    /// Attempts to parse the message body from a queue to a pension retrieval object.<br/>
    /// The message is validated against the schema for a pension retrieval message.<br/>
    /// </summary>
    /// <param name="message">The queue message to parse</param>
    /// <returns>A pension retrieval instance</returns>
    /// <exception cref="AggregateException">Thrown if the message is not in compliance with the schema</exception>
    PensionRetrievalPayload? ToPensionRetrievalPayload(string message);

    /// <summary>
    /// Attempts to parse the message body from a queue to a pension request object.<br/>
    /// The message is validated against the schema for a pension request message.<br/>
    /// </summary>
    /// <param name="message">The queue message to parse</param>
    /// <returns>A pension request instance</returns>
    /// <exception cref="AggregateException">Thrown if the message is not in compliance with the schema</exception>
    PensionRequestPayload? ToPensionRequestPayload(string message);
}
