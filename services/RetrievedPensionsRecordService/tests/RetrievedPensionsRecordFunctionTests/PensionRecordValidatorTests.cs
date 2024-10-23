using MhpdCommon.Utils;
using RetrievedPensionsRecordFunction.Utils;
using RetrievedPensionsRecordFunctionTests.Data;

namespace RetrievedPensionsRecordFunctionTests;

public class PensionRecordValidatorTests
{
    private readonly PensionRecordValidator _recordValidator;

    public PensionRecordValidatorTests()
    {
        _recordValidator = new PensionRecordValidator(new IdValidator());
    }

    [Theory]
    [InlineData("EmptyArrangementsPayload.json", true)]
    [InlineData("EmptyGuidRecordIdPayload.json", false)]
    [InlineData("InvalidPeiPatternPayload.json", false)]
    [InlineData("ValidRetrievedPensionPayload.json", true)]
    [InlineData("InvalidRecordIdPayload.json", false)]
    public void WhenAPayloadIsLoaded_ItIsValidated(string payloadFile, bool expectedResult)
    {
        var payload = DataProvider.GetPayload(payloadFile);

        Assert.NotNull(payload);
        var actualResult = _recordValidator.ValidateRecord(payload, out var reason);

        Assert.Equal(expectedResult, actualResult);
        Assert.True(expectedResult ? string.IsNullOrWhiteSpace(reason) : !string.IsNullOrWhiteSpace(reason));
    }

    [Fact]
    public void WhenPayloadIsNull_ItIsNotValidated()
    {
        var actualResult = _recordValidator.ValidateRecord(null, out var reason);

        Assert.False(actualResult);
        Assert.False(string.IsNullOrWhiteSpace(reason));
    }
}
