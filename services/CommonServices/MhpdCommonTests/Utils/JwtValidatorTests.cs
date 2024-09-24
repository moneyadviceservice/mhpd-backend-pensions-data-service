
using MhpdCommon.Utils;

namespace MhpdCommonTests.Utils;

public class JwtValidatorTests
{
    [Theory]
    [InlineData("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c", true)] // Valid JWT
    [InlineData("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxM", false)] // Invalid JWT (too short)
    [InlineData("invalid.jwt.token", false)] // Invalid JWT (not matching pattern)
    [InlineData("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.", false)] // Missing signature
    [InlineData("", false)] // Empty token
    [InlineData(null, false)] // Null token
    public void IsValidJwt_ShouldReturnExpectedResult(string token, bool expectedResult)
    {
        // Act
        var result = JwtValidator.IsJwtFormatValid(token);

        // Assert
        Assert.Equal(expectedResult, result);
    }
}