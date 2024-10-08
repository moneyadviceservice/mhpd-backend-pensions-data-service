using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.TokenValidation;
using Microsoft.Extensions.Logging;
using Moq;

namespace MhpdCommonTests.TokenValidationTests;

public class ClaimTokenFormatNotPresentValidatorTests
{
    private readonly ClaimTokenFormatNotPresentValidator _validator;

    public ClaimTokenFormatNotPresentValidatorTests()
    {
        Mock<ILogger<ClaimTokenFormatNotPresentValidator>> loggerMock = new();
        _validator = new ClaimTokenFormatNotPresentValidator(loggerMock.Object);
    }

    [Fact]
    public void Validate_ShouldReturnFailure_WhenClaimTokenFormatIsMissing()
    {
        var result = _validator.Validate(new CdaTokenRequestModel { GrantType = TokenQueryParams.UmaGrantType, ClaimTokenFormat = "" });

        Assert.False(result.IsValid);
        Assert.Equal(TokenValidationMessages.InvalidClaimTokenFormat, result.ErrorMessage);
    }

    [Fact]
    public void Validate_ShouldReturnSuccess_WhenClaimTokenFormatIsProvided()
    {
        var result = _validator.Validate(new CdaTokenRequestModel { GrantType = TokenQueryParams.UmaGrantType, ClaimTokenFormat = TokenQueryParams.PensionDashboardRqp});

        Assert.True(result.IsValid);
        Assert.Equal(7, _validator.Order);
    }
}
