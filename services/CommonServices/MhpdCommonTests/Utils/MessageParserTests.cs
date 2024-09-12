using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.Utils;

namespace MhpdCommonTests.Utils;

public class MessageParserTests
{
    private readonly MessageParser _messageParser;

    public MessageParserTests()
    {
        _messageParser = new MessageParser();
    }

    [Theory]
    [InlineData(@"TestData/RetrievedPensions/InvalidPeiPatternPayload.json")]
    [InlineData(@"TestData/RetrievedPensions/InvalidRecordIdPayload.json")]
    public void WhenAnInvalidPayloadIsParsed_ItThrowsAnException(string payloadFile)
    {
        var payloadData = File.ReadAllText(payloadFile);

        RetrievedPensionDetailsPayload? act() => _messageParser.ToRetrivedPensionRecord(payloadData);

        var error = Record.Exception((Func<RetrievedPensionDetailsPayload?>)act);
        Assert.NotNull(error);
        Assert.IsType<AggregateException>(error);
    }


    [Theory]
    [InlineData(@"TestData/RetrievedPensions/EmptyArrangementsPayload.json")]
    [InlineData(@"TestData/RetrievedPensions/ValidRetrievedPensionPayload.json")]
    [InlineData(@"TestData/RetrievedPensions/EmptyGuidRecordIdPayload.json")]
    public void WhenAValidPayloadIsParsed_ItReturnsARecord(string payloadFile)
    {
        var payloadData = File.ReadAllText(payloadFile);

        var payload = _messageParser.ToRetrivedPensionRecord(payloadData);

        Assert.NotNull(payload);
    }
}
