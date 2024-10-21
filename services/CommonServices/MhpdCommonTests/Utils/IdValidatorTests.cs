using MhpdCommon.Utils;

namespace MhpdCommonTests.Utils;

public class IdValidatorTests
{
    private readonly IdValidator _idValidator;
    private const string EmptyString = "";

    public IdValidatorTests()
    {
        _idValidator = new IdValidator();
    }

    [Theory]
    [InlineData(EmptyString, false)]
    [InlineData("NotAGuid", false)]
    [InlineData("{1761DC7D-9FD5-47B4-BC77-E3B54AEF0F07}", true)]
    [InlineData("53E30A9D-DB28-4273-8C3D-EDE691C3511A", true)]
    [InlineData("00000000-0000-0000-0000-000000000000", false)]
    [InlineData("?><>(*)&&-8586-4483-9899-17dd85af9074", false)]
    [InlineData(null, false)]
    public void WhenAGuidStringIsValidated_ReturnsCorrectResult(string? idString, bool expectedResult)
    {
        // Act
        var actualResult = _idValidator.IsValidGuid(idString);

        // Assert
        Assert.Equal(expectedResult, actualResult);
    }

    [Theory]
    [InlineData(EmptyString, false)]
    [InlineData("NotAPei", false)]
    [InlineData("e01a9df7-f147-4a3a-a1dd-0507432a5b7f:1ba03e25-659a-43b8-ae77-b956df168969", true)]
    [InlineData("7075aa11-10ad-4b2f-a9f5-1068e79119bx:1ba03e25-659a-43b8-ae77-b956df168965", false)]
    [InlineData("7075aa11-10ad-4b2f-a9f5-1068e79119b2:1ba03e25-659a-43b8-ae77-b956df16896%", false)]
    [InlineData("00000000-0000-0000-0000-000000000000:53E30A9D-DB28-4273-8C3D-EDE691C3511A:1761DC7D-9FD5-47B4-BC77-E3B54AEF0F07", false)]
    [InlineData(null, false)]
    public void WhenAPeiIsValidated_ReturnsCorrectResult(string? idString, bool expectedResult)
    {
        // Act
        var actualResult = _idValidator.IsValidPeI(idString);

        // Assert
        Assert.Equal(expectedResult, actualResult);
    }

    [Theory]
    [InlineData(EmptyString, false, EmptyString, EmptyString)]
    [InlineData("NotAPei", false, EmptyString, EmptyString)]
    [InlineData("e01a9df7-f147-4a3a-a1dd-0507432a5b7f:1ba03e25-659a-43b8-ae77-b956df168969", true, "e01a9df7-f147-4a3a-a1dd-0507432a5b7f", "1ba03e25-659a-43b8-ae77-b956df168969")]
    [InlineData("7075aa11-10ad-4b2f-a9f5-1068e79119bx:1ba03e25-659a-43b8-ae77-b956df168965", false, EmptyString, EmptyString)]
    [InlineData("7075aa11-10ad-4b2f-a9f5-1068e79119b2:1ba03e25-659a-43b8-ae77-b956df16896%", false, EmptyString, EmptyString)]
    [InlineData("00000000-0000-0000-0000-000000000000:53E30A9D-DB28-4273-8C3D-EDE691C3511A:1761DC7D-9FD5-47B4-BC77-E3B54AEF0F07", false, EmptyString, EmptyString)]
    [InlineData(null, false, EmptyString, EmptyString)]
    public void WhenTryExtractPei_ReturnsComponentParts(string? idString, bool expectedResult, string holderId, string assetId)
    {
        // Act
        var actualResult = _idValidator.TryExtractPei(idString, out var holderNameId, out var externalAssetId);

        // Assert
        Assert.Equal(expectedResult, actualResult);
        Assert.Equal(holderId, holderNameId);
        Assert.Equal(assetId, externalAssetId);
    }


    [Theory]
    [InlineData("Hello, World!", true)]  // Valid string, all characters are within the printable ASCII range
    [InlineData("1234567890", true)]      // Valid string, all numeric characters
    [InlineData(" ", true)]               // Valid string, single space
    [InlineData("!@#$%^&*()", true)]      // Valid string, special characters
    [InlineData("ASCII: \x7E\x20", true)] // Valid string, exact boundaries (space and tilde)
    [InlineData(EmptyString, false)]      // Invalid: empty string
    [InlineData("\x19Hello", false)]      // Invalid: control character
    [InlineData("Hello\x80", false)]      // Invalid: non-ASCII character (> 0x7E)
    [InlineData("Valid\x7EChars\x80Here", false)] // Mixed case with non-ASCII character

    // Test method
    public void TestIsValidString(string input, bool expected)
    {
        // Act
        var result = IdValidator.IsValidString(input);

        // Assert
        Assert.Equal(expected, result);
    }
}