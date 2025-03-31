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
    [InlineData(@"TestData/RetrievedPensions/InvalidRetrievedPensionPayload2.json")]
    public void WhenAnInvalidPayloadIsParsed_ItThrowsAnException(string payloadFile)
    {
        var payloadData = File.ReadAllText(payloadFile);

        Assert.Throws<AggregateException>(() => _messageParser.ToRetrievedPensionPayload(payloadData));
    }


    [Theory]
    [InlineData(@"TestData/RetrievedPensions/EmptyArrangementsPayload.json")]
    [InlineData(@"TestData/RetrievedPensions/ValidRetrievedPensionPayload.json")]
    [InlineData(@"TestData/RetrievedPensions/EmptyGuidRecordIdPayload.json")]
    [InlineData(@"TestData/RetrievedPensions/FailedRetrieval.json")]
    [InlineData(@"TestData/RetrievedPensions/ValidRetrievedPensionPayload2.json")]
    public void WhenAValidPayloadIsParsed_ItReturnsARecord(string payloadFile)
    {
        var payloadData = File.ReadAllText(payloadFile);

        var payload = _messageParser.ToRetrievedPensionPayload(payloadData);

        Assert.NotNull(payload);
    }

    [Theory]
    [InlineData(@"TestData/PensionRetrieval/BadPeisIdPensionRetrievalMessage.json")]
    [InlineData(@"TestData/PensionRetrieval/EmptyIssPensionRetrievalMessage.json")]
    [InlineData(@"TestData/PensionRetrieval/InvalidPensionRetrievalMessage.json")]
    [InlineData(@"TestData/PensionRetrieval/NullSessionPensionRetrievalMessage.json")]
    public void WhenAnInvalidRetrievalPayloadIsParsed_ItThrowsAnException(string payloadFile)
    {
        var payloadData = File.ReadAllText(payloadFile);

        Assert.Throws<AggregateException>(() => _messageParser.ToPensionRetrievalPayload(payloadData));
    }


    [Theory]
    [InlineData(@"TestData/PensionRetrieval/ValidPensionRetrievalMessage.json")]
    public void WhenAValidRetrievalPayloadIsParsed_ItReturnsARecord(string payloadFile)
    {
        var payloadData = File.ReadAllText(payloadFile);

        var payload = _messageParser.ToPensionRetrievalPayload(payloadData);

        Assert.NotNull(payload);
    }

    [Theory]
    [InlineData(@"TestData/PensionRequest/InvalidPeiRequest.json")]
    [InlineData(@"TestData/PensionRequest/InvalidRecordIdRequest.json")]
    [InlineData(@"TestData/PensionRequest/MissingIssuerRequest.json")]
    public void WhenAnInvalidRequestPayloadIsParsed_ItThrowsAnException(string payloadFile)
    {
        var payloadData = File.ReadAllText(payloadFile);

        Assert.Throws<AggregateException>(() => _messageParser.ToPensionRequestPayload(payloadData));
    }


    [Theory]
    [InlineData(@"TestData/PensionRequest/ValidPensionRequest.json")]
    public void WhenAValidRequestPayloadIsParsed_ItReturnsARecord(string payloadFile)
    {
        var payloadData = File.ReadAllText(payloadFile);

        var payload = _messageParser.ToPensionRequestPayload(payloadData);

        Assert.NotNull(payload);
    }

    [Theory]
    [InlineData(@"TestData/PensionRequest/ValidDefiniteMatchViewData.json")]
    [InlineData(@"TestData/PensionRequest/ValidPossibleMatchViewData.json")]
    public void WhenAValidViewDataPayloadIsParsed_ItReturnsARecord(string payloadFile)
    {
        var payloadData = File.ReadAllText(payloadFile);

        var payload = _messageParser.ToViewDataPayload(payloadData);

        Assert.NotNull(payload);
    }

    [Theory]
    [InlineData(@"TestData/PensionRequest/InvalidPensionTypeViewData.json")]
    [InlineData(@"TestData/PensionRequest/InvalidPensionStatusViewData.json")]
    [InlineData(@"TestData/PensionRequest/BlankAdministratorNameViewData.json")]
    public void WhenAnInvalidViewDataPayloadIsParsed_ItThrowsAnException(string payloadFile)
    {
        var payloadData = File.ReadAllText(payloadFile);

        Assert.Throws<AggregateException>(() => _messageParser.ToViewDataPayload(payloadData));
    }
}
