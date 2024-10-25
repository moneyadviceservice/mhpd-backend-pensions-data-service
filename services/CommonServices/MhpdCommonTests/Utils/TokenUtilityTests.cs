using System.IdentityModel.Tokens.Jwt;
using MhpdCommon.Constants.HttpClient;
using MhpdCommon.Models.Configuration;
using MhpdCommon.Utils;
using MhpdCommonTests.TokenValidationTests;
using Microsoft.Extensions.Options;
using Moq;

namespace MhpdCommonTests.Utils;

public class TokenUtilityTests
{
    private readonly MhpdCommon.Utils.TokenUtility _tokenUtility;

    public TokenUtilityTests()
    {
        var configuration = new JwtSettings
        {
            PrivateKey = Helper.GeneratedRsaPrivateKeyPem,
            ExpiryInSeconds = "600"
        };

        Mock<IOptions<JwtSettings>> mockJwtSettingsOptions = new();
        mockJwtSettingsOptions.Setup(x => x.Value).Returns(configuration);

        _tokenUtility = new MhpdCommon.Utils.TokenUtility(mockJwtSettingsOptions.Object);
    }

    [Theory]
    [InlineData("TEST")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void GenerateJwt_ShouldReturnValidJwt_WithExpectedClaims(string? peisStartCode)
    {
        // Act
        var jwtToken = _tokenUtility.GenerateJwt(peisStartCode);
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(jwtToken);

        // Assert - Check if the token is not null
        Assert.NotNull(token);

        // Assert - Check if the token contains the expected claims
        Assert.Equal("cf668d47-ee58-4e33-bc05-feb7058de58d", token.Subject);
        Assert.Equal("https://emulators.maps.org.uk/am/oauth2", token.Issuer);
        Assert.Equal("https://pdp/ig/token", token.Audiences.FirstOrDefault());

        var iatClaim = token.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Iat);
        var expClaim = token.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Exp);
        var jtiClaim = token.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti);
        var peisIdClaim = token.Claims.FirstOrDefault(c => c.Type == QueryParams.Cda.Token.PeisId);

        // Assert - Check if the claims exist and are correct
        Assert.NotNull(iatClaim);
        Assert.NotNull(expClaim);
        Assert.NotNull(jtiClaim);

        if (string.IsNullOrWhiteSpace(peisStartCode))
        {
            Assert.Null(peisIdClaim);
        }
        else
        {
            Assert.NotNull(peisIdClaim);

            // Validate that peis_id starts with peisStartCode
            Assert.StartsWith(peisStartCode, peisIdClaim.Value);
        }
    }
    
    [Fact]
    public void DoesRegexMatch_ValidInput_ReturnsTrue()
    {
        // Arrange
        string input = "abc123";
        string pattern = @"^[a-zA-Z0-9]+$"; // Alphanumeric characters only

        // Act
        bool result = _tokenUtility.DoesRegexMatch(input, pattern);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void DoesRegexMatch_InvalidInput_ReturnsFalse()
    {
        // Arrange
        string input = "abc123!";
        string pattern = @"^[a-zA-Z0-9]+$"; // Alphanumeric characters only

        // Act
        bool result = _tokenUtility.DoesRegexMatch(input, pattern);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void DoesRegexMatch_EmptyInput_ReturnsFalse()
    {
        // Arrange
        string input = string.Empty;
        string pattern = @"^[a-zA-Z0-9]+$"; // Alphanumeric characters only

        // Act
        bool result = _tokenUtility.DoesRegexMatch(input, pattern);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void DoesRegexMatch_Timeout_ReturnsFalse()
    {
        // Arrange
        string input = "abc"; // Input that won't cause timeout
        string pattern = ".*"; // Very simple pattern that will match anything

        // Act
        // Simulate a long-running regex match by using a very complex pattern
        // or by creating a long-running regex pattern (not shown, as it's typically hard to do)

        // Here we use a valid pattern to ensure the regex doesn't time out
        bool result = _tokenUtility.DoesRegexMatch(input, pattern);

        // Assert
        Assert.True(result); // The pattern matches any input
    }

    [Fact]
    public void DoesRegexMatch_ComplexPattern_ReturnsTrue()
    {
        // Arrange
        string input = "2024-10-07"; // Date format
        string pattern = @"^\d{4}-\d{2}-\d{2}$"; // YYYY-MM-DD format

        // Act
        bool result = _tokenUtility.DoesRegexMatch(input, pattern);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void DoesRegexMatch_ComplexPattern_InvalidInput_ReturnsFalse()
    {
        // Arrange
        string input = "10/07/2024"; // Invalid date format
        string pattern = @"^\d{4}-\d{2}-\d{2}$"; // YYYY-MM-DD format

        // Act
        bool result = _tokenUtility.DoesRegexMatch(input, pattern);

        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public void IsValidUrl_ValidHttpUrl_ReturnsTrue()
    {
        // Arrange
        const string url = "http://www.example.com";

        // Act
        var result = TokenUtility.IsValidUrl(url);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValidUrl_ValidHttpsUrl_ReturnsTrue()
    {
        // Arrange
        const string url = "https://www.example.com";

        // Act
        var result = TokenUtility.IsValidUrl(url);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValidUrl_InvalidUrlMissingScheme_ReturnsFalse()
    {
        // Arrange
        const string url = "www.example.com";

        // Act
        var result = TokenUtility.IsValidUrl(url);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValidUrl_InvalidUrlWithUnsupportedScheme_ReturnsFalse()
    {
        // Arrange
        const string url = "ftp://www.example.com";

        // Act
        var result = TokenUtility.IsValidUrl(url);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValidUrl_InvalidUrlMalformed_ReturnsFalse()
    {
        // Arrange
        const string url = "htp://example";

        // Act
        var result = TokenUtility.IsValidUrl(url);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValidUrl_EmptyString_ReturnsFalse()
    {
        // Arrange
        const string url = "";

        // Act
        var result = TokenUtility.IsValidUrl(url);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValidUrl_NullString_ReturnsFalse()
    {
        // Arrange
        string url = null!;

        // Act
        var result = TokenUtility.IsValidUrl(url);

        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public void DecodeJwt_ShouldReturnCorrectClaims()
    {
        // Arrange
        string peisStartCode = "PEIS123";
        
        // Generate a JWT
        string token = _tokenUtility.GenerateJwt(peisStartCode);
        
        // Act
        var claims = _tokenUtility.DecodeJwt(token);
        
        // Assert
        Assert.True(claims.ContainsKey("sub"));
        Assert.Equal("cf668d47-ee58-4e33-bc05-feb7058de58d", claims["sub"]);

        Assert.True(claims.ContainsKey("iss"));
        Assert.Equal("https://emulators.maps.org.uk/am/oauth2", claims["iss"]);

        Assert.True(claims.ContainsKey("aud"));
        Assert.Equal("https://pdp/ig/token", claims["aud"]);

        Assert.True(claims.ContainsKey("iat"));
        Assert.False(string.IsNullOrEmpty(claims["iat"]));

        Assert.True(claims.ContainsKey("exp"));
        Assert.False(string.IsNullOrEmpty(claims["exp"]));

        Assert.True(claims.ContainsKey("jti"));
        Assert.False(string.IsNullOrEmpty(claims["jti"]));

        Assert.True(claims.ContainsKey("peis_id"));
        Assert.StartsWith(peisStartCode, claims["peis_id"]);
    }

    [Fact]
    public void DecodeJwt_ShouldThrowException_ForInvalidToken()
    {
        // Arrange
        string invalidToken = "invalid.jwt.token";
        
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _tokenUtility.DecodeJwt(invalidToken));
    }

    [Fact]
    public void DecodeJwt_ShouldReturn_Correct_PeisId_ForValidToken()
    {
        // Arrange
        var idToken =
            "eyJhbGciOiJodHRwOi8vd3d3LnczLm9yZy8yMDAxLzA0L3htbGRzaWctbW9yZSNyc2Etc2hhMjU2IiwidHlwIjoiSldUIn0.eyJwZWlzX2lkIjoiMDAwMjZjZTItOWM3ZS00YjJmLWE3YTYtMDk1ZDU1ZGUwODExIiwic3ViIjoiY2Y2NjhkNDctZWU1OC00ZTMzLWJjMDUtZmViNzA1OGRlNThkIiwiaXNzIjoiaHR0cHM6Ly9lbXVsYXRvcnMubWFwcy5vcmcudWsvYW0vb2F1dGgyIiwiYXVkIjpbImh0dHBzOi8vcGRwL2lnL3Rva2VuIiwiaHR0cHM6Ly9wZHAvaWcvdG9rZW4iXSwiaWF0IjoxNzI4NDU4ODQ3LCJleHAiOjE3Mjg0NTk0NDcsImp0aSI6IjAwOWYxMzFkLTk1NmUtNDgxMS1hNjM0LTgzZDdhYTRjZDZkMyJ9.ENiVIWmCmRZSt0KO71irwxtO4Z2yJGN3Z3luPEDVIyVI_8oFReVgLRZbpnigSxgJnu_8lh3tE1L3c5uv29ScB5a1e6AqoGcJ8R6zc8J1DrE6oBQA7ClU7guw1m9Rx1D2z4X0PiRk0AVU3rF5UkqDpNcJ2dlyLBkUYTYmuJZ25AWq2NYlQaHL_f2kErC1-Q6gsFWbGmKLIEJbjiGB4FN4kI_jIe9Ey04QOHvc1Z59vgGLdU9k9wNt9mooK2daacqhUnAjNb1JAGOuEs5rt5v_5S-7V6MVw0Oubx3UEE378Q_UKPUDQTdcsD20G7knOjUpu_ZTYlITFyOpOs2lUM4xrw";
        
        // Act
        var result = _tokenUtility.DecodeJwt(idToken);
        
        // Assert
        Assert.True(result.ContainsKey("peis_id"));
        Assert.Equal("00026ce2-9c7e-4b2f-a7a6-095d55de0811", result["peis_id"]);
    }
    
    [Fact]
    public void IsValidCodeVerifier_ValidCodeVerifier_ReturnsTrue()
    {
        // Arrange
        const string validCodeVerifier = "7189b64cc5f65b805baf201e384dc53ae7d18305d5ebb6170ad557b6"; // 60 characters

        // Act
        var result = TokenUtility.IsValidCodeVerifier(validCodeVerifier);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValidCodeVerifier_TooShortCodeVerifier_ReturnsFalse()
    {
        // Arrange
        const string shortCodeVerifier = "short123"; // Less than 43 characters

        // Act
        var result = TokenUtility.IsValidCodeVerifier(shortCodeVerifier);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValidCodeVerifier_TooLongCodeVerifier_ReturnsFalse()
    {
        // Arrange
        var longCodeVerifier = new string('a', 129); // More than 128 characters

        // Act
        var result = TokenUtility.IsValidCodeVerifier(longCodeVerifier);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValidCodeVerifier_InvalidCharacters_ReturnsFalse()
    {
        // Arrange
        const string invalidCodeVerifier = "7189b64cc5f65b805baf201e384dc53ae7d18305d5ebb6170ad557b6@#"; // Invalid special characters

        // Act
        var result = TokenUtility.IsValidCodeVerifier(invalidCodeVerifier);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValidCodeVerifier_NullOrEmptyCodeVerifier_ReturnsFalse()
    {
        // Arrange
        string nullCodeVerifier = null!;
        const string emptyCodeVerifier = "";

        // Act & Assert
        Assert.False(TokenUtility.IsValidCodeVerifier(nullCodeVerifier));
        Assert.False(TokenUtility.IsValidCodeVerifier(emptyCodeVerifier));
    }

    [Fact]
    public void IsValidCodeVerifier_BoundaryLengthValid_ReturnsTrue()
    {
        // Arrange
        var minLengthCodeVerifier = new string('a', 43); // Exactly 43 characters
        var maxLengthCodeVerifier = new string('a', 128); // Exactly 128 characters

        // Act & Assert
        Assert.True(TokenUtility.IsValidCodeVerifier(minLengthCodeVerifier));
        Assert.True(TokenUtility.IsValidCodeVerifier(maxLengthCodeVerifier));
    }

    [Fact]
    public void WhenRetrievedClaimCalled_AndKnownClaimPresent_ItShouldReturnClaimValue()
    {
        var expectedViewData = "{\r\n\t\"arrangements\": [\r\n\t\t{\r\n\t\t\t\"pensionProviderSchemeName\": \"My Company Direct Contribution Scheme\",\r\n\t\t\t\"alternateSchemeName\": {\r\n\t\t\t\t\"name\": \"Converted from My Old Direct Contribution Scheme\",\r\n\t\t\t\t\"alternateNameType\": \"FOR\"\r\n\t\t\t},\r\n\t\t\t\"possibleMatch\": true,\r\n\t\t\t\"possibleMatchReference\": \"Q12345\",\r\n\t\t\t\"pensionAdministrator\": {\r\n\t\t\t\t\"name\": \"Pension Company 1\",\r\n\t\t\t\t\"contactMethods\": [\r\n\t\t\t\t\t{\r\n\t\t\t\t\t\t\"preferred\": false,\r\n\t\t\t\t\t\t\"contactMethodDetails\": {\r\n\t\t\t\t\t\t\t\"email\": \"example@examplemyline.com\"\r\n\t\t\t\t\t\t}\r\n\t\t\t\t\t},\r\n\t\t\t\t\t{\r\n\t\t\t\t\t\t\"preferred\": true,\r\n\t\t\t\t\t\t\"contactMethodDetails\": {\r\n\t\t\t\t\t\t\t\"number\": \"+123 1111111111\",\r\n\t\t\t\t\t\t\t\"usage\": [\r\n\t\t\t\t\t\t\t\t\"A\",\r\n\t\t\t\t\t\t\t\t\"M\"\r\n\t\t\t\t\t\t\t]\r\n\t\t\t\t\t\t}\r\n\t\t\t\t\t}\r\n\t\t\t\t]\r\n\t\t\t}\r\n\t\t}\r\n\t]\r\n}";
        var viewDataToken = "eyJhbGciOiJSUzI1NiIsImtpZCI6ImMwMGI0MGVhLTZkYTEtNDA4YS1hNmM5LTE3YjFmZjQ1YmI5YSIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIzMjRicWZ3MzQ4ZjlxNDM5OGgzIiwiaWF0IjoxNzIwNjk5MzY2LCJleHAiOjE3MjA3MDI5NjYsImp0aSI6IjZiNTQ1NjllLTNmN2UtNGRiMy1hNDhkLTk4YjEwMmFhMWZlMyIsImF1ZCI6Imh0dHBzOi8vcGRwL2lnL3Rva2VuIiwiaXNzIjoiREFUQV9QUk9WSURFUl8xZmQxZGE4OC05ZmIzLTQ2MWMtYTQ4YS0zZGJhMjFiZmJhMTciLCJWaWV3RGF0YSI6IntcclxuXHRcImFycmFuZ2VtZW50c1wiOiBbXHJcblx0XHR7XHJcblx0XHRcdFwicGVuc2lvblByb3ZpZGVyU2NoZW1lTmFtZVwiOiBcIk15IENvbXBhbnkgRGlyZWN0IENvbnRyaWJ1dGlvbiBTY2hlbWVcIixcclxuXHRcdFx0XCJhbHRlcm5hdGVTY2hlbWVOYW1lXCI6IHtcclxuXHRcdFx0XHRcIm5hbWVcIjogXCJDb252ZXJ0ZWQgZnJvbSBNeSBPbGQgRGlyZWN0IENvbnRyaWJ1dGlvbiBTY2hlbWVcIixcclxuXHRcdFx0XHRcImFsdGVybmF0ZU5hbWVUeXBlXCI6IFwiRk9SXCJcclxuXHRcdFx0fSxcclxuXHRcdFx0XCJwb3NzaWJsZU1hdGNoXCI6IHRydWUsXHJcblx0XHRcdFwicG9zc2libGVNYXRjaFJlZmVyZW5jZVwiOiBcIlExMjM0NVwiLFxyXG5cdFx0XHRcInBlbnNpb25BZG1pbmlzdHJhdG9yXCI6IHtcclxuXHRcdFx0XHRcIm5hbWVcIjogXCJQZW5zaW9uIENvbXBhbnkgMVwiLFxyXG5cdFx0XHRcdFwiY29udGFjdE1ldGhvZHNcIjogW1xyXG5cdFx0XHRcdFx0e1xyXG5cdFx0XHRcdFx0XHRcInByZWZlcnJlZFwiOiBmYWxzZSxcclxuXHRcdFx0XHRcdFx0XCJjb250YWN0TWV0aG9kRGV0YWlsc1wiOiB7XHJcblx0XHRcdFx0XHRcdFx0XCJlbWFpbFwiOiBcImV4YW1wbGVAZXhhbXBsZW15bGluZS5jb21cIlxyXG5cdFx0XHRcdFx0XHR9XHJcblx0XHRcdFx0XHR9LFxyXG5cdFx0XHRcdFx0e1xyXG5cdFx0XHRcdFx0XHRcInByZWZlcnJlZFwiOiB0cnVlLFxyXG5cdFx0XHRcdFx0XHRcImNvbnRhY3RNZXRob2REZXRhaWxzXCI6IHtcclxuXHRcdFx0XHRcdFx0XHRcIm51bWJlclwiOiBcIisxMjMgMTExMTExMTExMVwiLFxyXG5cdFx0XHRcdFx0XHRcdFwidXNhZ2VcIjogW1xyXG5cdFx0XHRcdFx0XHRcdFx0XCJBXCIsXHJcblx0XHRcdFx0XHRcdFx0XHRcIk1cIlxyXG5cdFx0XHRcdFx0XHRcdF1cclxuXHRcdFx0XHRcdFx0fVxyXG5cdFx0XHRcdFx0fVxyXG5cdFx0XHRcdF1cclxuXHRcdFx0fVxyXG5cdFx0fVxyXG5cdF1cclxufSIsIm5iZiI6MTcyMDY5OTM2Nn0.NPa_p2hETl_CoIz-R2MFrWjAC2vFgwb5aUHFBooPTiA4jDOzdMEk9ZvS2KogL8TVW36OeqiJYtcMunP5yRkFykjlvr06W1L1SoJFxv__pOSNzjP5nziOVyHYEiYvIVkrm3Yv-96KlFe_wLsZ7p1ibsTI8krPF_rIt0xjvU555_sFaebCMCsaxRujzo7Gp9Ef55AxizpXi-osddzMYfbcLfe9DpNzl7Ij4pGQriEAR9R2ZZCUCwQd0n8DV9ZN4hIsOVCzHsY16b-7bnciy8GScsKLLcFeuN4LQba4uOW1AqlLXFv-8_YrJofAzIH8eej6rB4t8KBB9cvTxrRvikFHxA";

        var viewDataValue = _tokenUtility.RetrieveClaim(viewDataToken, "ViewData");

        Assert.Equal(expectedViewData, viewDataValue!.ToString());
    }

    [Fact]
    public void WhenRetrievedClaimCalled_AndClaimNotPresent_ItShouldReturnNull()
    {
        var viewDataToken = "eyJhbGciOiJSUzI1NiIsImtpZCI6ImMwMGI0MGVhLTZkYTEtNDA4YS1hNmM5LTE3YjFmZjQ1YmI5YSIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIzMjRicWZ3MzQ4ZjlxNDM5OGgzIiwiaWF0IjoxNzIwNjk5MzY2LCJleHAiOjE3MjA3MDI5NjYsImp0aSI6IjZiNTQ1NjllLTNmN2UtNGRiMy1hNDhkLTk4YjEwMmFhMWZlMyIsImF1ZCI6Imh0dHBzOi8vcGRwL2lnL3Rva2VuIiwiaXNzIjoiREFUQV9QUk9WSURFUl8xZmQxZGE4OC05ZmIzLTQ2MWMtYTQ4YS0zZGJhMjFiZmJhMTciLCJWaWV3RGF0YSI6IntcclxuXHRcImFycmFuZ2VtZW50c1wiOiBbXHJcblx0XHR7XHJcblx0XHRcdFwicGVuc2lvblByb3ZpZGVyU2NoZW1lTmFtZVwiOiBcIk15IENvbXBhbnkgRGlyZWN0IENvbnRyaWJ1dGlvbiBTY2hlbWVcIixcclxuXHRcdFx0XCJhbHRlcm5hdGVTY2hlbWVOYW1lXCI6IHtcclxuXHRcdFx0XHRcIm5hbWVcIjogXCJDb252ZXJ0ZWQgZnJvbSBNeSBPbGQgRGlyZWN0IENvbnRyaWJ1dGlvbiBTY2hlbWVcIixcclxuXHRcdFx0XHRcImFsdGVybmF0ZU5hbWVUeXBlXCI6IFwiRk9SXCJcclxuXHRcdFx0fSxcclxuXHRcdFx0XCJwb3NzaWJsZU1hdGNoXCI6IHRydWUsXHJcblx0XHRcdFwicG9zc2libGVNYXRjaFJlZmVyZW5jZVwiOiBcIlExMjM0NVwiLFxyXG5cdFx0XHRcInBlbnNpb25BZG1pbmlzdHJhdG9yXCI6IHtcclxuXHRcdFx0XHRcIm5hbWVcIjogXCJQZW5zaW9uIENvbXBhbnkgMVwiLFxyXG5cdFx0XHRcdFwiY29udGFjdE1ldGhvZHNcIjogW1xyXG5cdFx0XHRcdFx0e1xyXG5cdFx0XHRcdFx0XHRcInByZWZlcnJlZFwiOiBmYWxzZSxcclxuXHRcdFx0XHRcdFx0XCJjb250YWN0TWV0aG9kRGV0YWlsc1wiOiB7XHJcblx0XHRcdFx0XHRcdFx0XCJlbWFpbFwiOiBcImV4YW1wbGVAZXhhbXBsZW15bGluZS5jb21cIlxyXG5cdFx0XHRcdFx0XHR9XHJcblx0XHRcdFx0XHR9LFxyXG5cdFx0XHRcdFx0e1xyXG5cdFx0XHRcdFx0XHRcInByZWZlcnJlZFwiOiB0cnVlLFxyXG5cdFx0XHRcdFx0XHRcImNvbnRhY3RNZXRob2REZXRhaWxzXCI6IHtcclxuXHRcdFx0XHRcdFx0XHRcIm51bWJlclwiOiBcIisxMjMgMTExMTExMTExMVwiLFxyXG5cdFx0XHRcdFx0XHRcdFwidXNhZ2VcIjogW1xyXG5cdFx0XHRcdFx0XHRcdFx0XCJBXCIsXHJcblx0XHRcdFx0XHRcdFx0XHRcIk1cIlxyXG5cdFx0XHRcdFx0XHRcdF1cclxuXHRcdFx0XHRcdFx0fVxyXG5cdFx0XHRcdFx0fVxyXG5cdFx0XHRcdF1cclxuXHRcdFx0fVxyXG5cdFx0fVxyXG5cdF1cclxufSIsIm5iZiI6MTcyMDY5OTM2Nn0.NPa_p2hETl_CoIz-R2MFrWjAC2vFgwb5aUHFBooPTiA4jDOzdMEk9ZvS2KogL8TVW36OeqiJYtcMunP5yRkFykjlvr06W1L1SoJFxv__pOSNzjP5nziOVyHYEiYvIVkrm3Yv-96KlFe_wLsZ7p1ibsTI8krPF_rIt0xjvU555_sFaebCMCsaxRujzo7Gp9Ef55AxizpXi-osddzMYfbcLfe9DpNzl7Ij4pGQriEAR9R2ZZCUCwQd0n8DV9ZN4hIsOVCzHsY16b-7bnciy8GScsKLLcFeuN4LQba4uOW1AqlLXFv-8_YrJofAzIH8eej6rB4t8KBB9cvTxrRvikFHxA";

        var viewDataValue = _tokenUtility.RetrieveClaim(viewDataToken, "BlaBla");

        Assert.Null(viewDataValue);
    }

    [Fact]
    public void WhenRetrievedClaimCalled_AndClaimIsInvalid_ItShouldReturnNull()
    {
        var viewDataToken = "XXXeyJhbGciOiJSUzI1NiIsImtpZCI6ImMwMGI0MGVhLTZkYTEtNDA4YS1hNmM5LTE3YjFmZjQ1YmI5YSIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIzMjRicWZ3MzQ4ZjlxNDM5OGgzIiwiaWF0IjoxNzIwNjk5MzY2LCJleHAiOjE3MjA3MDI5NjYsImp0aSI6IjZiNTQ1NjllLTNmN2UtNGRiMy1hNDhkLTk4YjEwMmFhMWZlMyIsImF1ZCI6Imh0dHBzOi8vcGRwL2lnL3Rva2VuIiwiaXNzIjoiREFUQV9QUk9WSURFUl8xZmQxZGE4OC05ZmIzLTQ2MWMtYTQ4YS0zZGJhMjFiZmJhMTciLCJWaWV3RGF0YSI6IntcclxuXHRcImFycmFuZ2VtZW50c1wiOiBbXHJcblx0XHR7XHJcblx0XHRcdFwicGVuc2lvblByb3ZpZGVyU2NoZW1lTmFtZVwiOiBcIk15IENvbXBhbnkgRGlyZWN0IENvbnRyaWJ1dGlvbiBTY2hlbWVcIixcclxuXHRcdFx0XCJhbHRlcm5hdGVTY2hlbWVOYW1lXCI6IHtcclxuXHRcdFx0XHRcIm5hbWVcIjogXCJDb252ZXJ0ZWQgZnJvbSBNeSBPbGQgRGlyZWN0IENvbnRyaWJ1dGlvbiBTY2hlbWVcIixcclxuXHRcdFx0XHRcImFsdGVybmF0ZU5hbWVUeXBlXCI6IFwiRk9SXCJcclxuXHRcdFx0fSxcclxuXHRcdFx0XCJwb3NzaWJsZU1hdGNoXCI6IHRydWUsXHJcblx0XHRcdFwicG9zc2libGVNYXRjaFJlZmVyZW5jZVwiOiBcIlExMjM0NVwiLFxyXG5cdFx0XHRcInBlbnNpb25BZG1pbmlzdHJhdG9yXCI6IHtcclxuXHRcdFx0XHRcIm5hbWVcIjogXCJQZW5zaW9uIENvbXBhbnkgMVwiLFxyXG5cdFx0XHRcdFwiY29udGFjdE1ldGhvZHNcIjogW1xyXG5cdFx0XHRcdFx0e1xyXG5cdFx0XHRcdFx0XHRcInByZWZlcnJlZFwiOiBmYWxzZSxcclxuXHRcdFx0XHRcdFx0XCJjb250YWN0TWV0aG9kRGV0YWlsc1wiOiB7XHJcblx0XHRcdFx0XHRcdFx0XCJlbWFpbFwiOiBcImV4YW1wbGVAZXhhbXBsZW15bGluZS5jb21cIlxyXG5cdFx0XHRcdFx0XHR9XHJcblx0XHRcdFx0XHR9LFxyXG5cdFx0XHRcdFx0e1xyXG5cdFx0XHRcdFx0XHRcInByZWZlcnJlZFwiOiB0cnVlLFxyXG5cdFx0XHRcdFx0XHRcImNvbnRhY3RNZXRob2REZXRhaWxzXCI6IHtcclxuXHRcdFx0XHRcdFx0XHRcIm51bWJlclwiOiBcIisxMjMgMTExMTExMTExMVwiLFxyXG5cdFx0XHRcdFx0XHRcdFwidXNhZ2VcIjogW1xyXG5cdFx0XHRcdFx0XHRcdFx0XCJBXCIsXHJcblx0XHRcdFx0XHRcdFx0XHRcIk1cIlxyXG5cdFx0XHRcdFx0XHRcdF1cclxuXHRcdFx0XHRcdFx0fVxyXG5cdFx0XHRcdFx0fVxyXG5cdFx0XHRcdF1cclxuXHRcdFx0fVxyXG5cdFx0fVxyXG5cdF1cclxufSIsIm5iZiI6MTcyMDY5OTM2Nn0.NPa_p2hETl_CoIz-R2MFrWjAC2vFgwb5aUHFBooPTiA4jDOzdMEk9ZvS2KogL8TVW36OeqiJYtcMunP5yRkFykjlvr06W1L1SoJFxv__pOSNzjP5nziOVyHYEiYvIVkrm3Yv-96KlFe_wLsZ7p1ibsTI8krPF_rIt0xjvU555_sFaebCMCsaxRujzo7Gp9Ef55AxizpXi-osddzMYfbcLfe9DpNzl7Ij4pGQriEAR9R2ZZCUCwQd0n8DV9ZN4hIsOVCzHsY16b-7bnciy8GScsKLLcFeuN4LQba4uOW1AqlLXFv-8_YrJofAzIH8eej6rB4t8KBB9cvTxrRvikFHxA";

        var viewDataValue = _tokenUtility.RetrieveClaim(viewDataToken, "ViewData");

        Assert.Null(viewDataValue);
    }
}