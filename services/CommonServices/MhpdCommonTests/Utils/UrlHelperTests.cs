using MhpdCommon.Constants.HttpClient;
using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.Utils;
using Microsoft.AspNetCore.Mvc;

namespace MhpdCommonTests.Utils;

public class UrlHelperTests
{
    [Fact]
    public void ConstructEndPoint_ShouldGenerateQueryString_ForCdaTokenRequestModel()
    {
        // Arrange
        var request = new CdaTokenRequestModel
        {
            GrantType = "authorization_code",
            Ticket = "my_ticket",
            ClaimToken = "my_token",
            ClaimTokenFormat = "format",
            Scope = "read"
        };

        // Act
        var result = UrlHelper.ConstructEndPoint(request, HttpEndpoints.External.CdaTokenServiceEndpoint);

        // Assert
        Assert.Equal("token?grant_type=authorization_code&ticket=my_ticket&claim_token=my_token&claim_token_format=format&scope=read", result);
    }

    [Fact]
    public void ConstructEndPoint_ShouldGenerateQueryString_ForUserRequestModel()
    {
        // Arrange
        var request = new UserRequestModel
        {
            UserId = "123",
            Email = "test@example.com"
        };

        // Act
        var result = UrlHelper.ConstructEndPoint(request, HttpEndpoints.External.CdaTokenServiceEndpoint);

        // Assert
        Assert.Equal("token?user_id=123&email=test@example.com", result);
    }

    [Fact]
    public void ConstructEndPoint_ShouldIgnoreNullValues()
    {
        // Arrange
        var request = new CdaTokenRequestModel
        {
            GrantType = "authorization_code",
            Ticket = null,  // This should be ignored in the query string
            ClaimTokenFormat = "format"
        };

        // Act
        var result = UrlHelper.ConstructEndPoint(request, HttpEndpoints.External.CdaTokenServiceEndpoint);

        // Assert
        Assert.Equal("token?grant_type=authorization_code&claim_token_format=format", result);
    }

    [Fact]
    public void ConstructEndPoint_ShouldHandleEmptyQueryStringWhenNoPropertiesAreSet()
    {
        // Arrange
        var request = new CdaTokenRequestModel(); // No values set

        // Act
        var result = UrlHelper.ConstructEndPoint(request, HttpEndpoints.External.CdaTokenServiceEndpoint);

        // Assert
        Assert.Equal("token?", result); // Empty query string
    }

    [Theory]
    [InlineData("http://www.pdp.com", "/view/data", "http://www.pdp.com/view/data")]
    [InlineData("http://www.pdp.com", "view/data", "http://www.pdp.com/view/data")]
    [InlineData("http://www.pdp.com/", "/view/data", "http://www.pdp.com/view/data")]
    [InlineData("http://www.pdp.com/", "view/data", "http://www.pdp.com/view/data")]
    [InlineData("http://www.pdp.com/view", "/data", "http://www.pdp.com/view/data")]
    [InlineData("http://www.pdp.com/view", "data", "http://www.pdp.com/view/data")]
    [InlineData("http://www.pdp.com/view/", "/data", "http://www.pdp.com/view/data")]
    [InlineData("http://www.pdp.com/view/", "data", "http://www.pdp.com/view/data")]
    [InlineData("http://www.pdp.com", null, "http://www.pdp.com")]
    public void One_Base_Path_Should_Be_Combined_With_Relative_Path(string baseUrl, string route, string expected)
    {
        var actual = UrlHelper.ConstructPath(baseUrl, route);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Base_Path_Null_Should_Throw_Exception()
    {
        Assert.Throws<ArgumentNullException>(() => UrlHelper.ConstructPath(null, "relative/path"));
    }
}

public class UserRequestModel
{
    [FromQuery(Name = "user_id")]
    public string? UserId { get; set; }

    [FromQuery(Name = "email")]
    public string? Email { get; set; }
}