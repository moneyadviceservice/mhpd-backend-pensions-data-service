using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.TokenValidation;
using Microsoft.Extensions.Logging;
using Moq;

namespace MhpdCommonTests.TokenValidationTests;

public class ClaimTokenNotPresentValidationTests
{
    private readonly ClaimTokenNotPresentValidation _validator;

    public ClaimTokenNotPresentValidationTests()
    {
        Mock<ILogger<ClaimTokenNotPresentValidation>> loggerMock = new();
        _validator = new ClaimTokenNotPresentValidation(loggerMock.Object);
    }

    [Fact]
    public void Validate_ShouldReturnFailure_WhenClaimTokenIsMissing()
    {
        var result = _validator.Validate(new CdaTokenRequestModel { GrantType = TokenQueryParams.UmaGrantType, ClaimToken = "" });

        Assert.False(result.IsValid);
        Assert.Equal(TokenValidationMessages.InvalidClaimToken, result.ErrorMessage);
    }

    [Fact]
    public void Validate_ShouldReturnSuccess_WhenClaimTokenIsProvided()
    {
        var result = _validator.Validate(new CdaTokenRequestModel { GrantType = TokenQueryParams.UmaGrantType, ClaimToken = TokenQueryParams.ValidJwtToken});

        Assert.True(result.IsValid);
        Assert.Equal(5, _validator.Order);
    }
}
