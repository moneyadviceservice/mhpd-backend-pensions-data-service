using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.TokenValidation;
using Microsoft.Extensions.Logging;
using Moq;

namespace MhpdCommonTests.TokenValidationTests;

public class ClaimTokenFormatNotPensionDashboardRqpValidatorTests
{
    private readonly ClaimTokenFormatNotPensionDashboardRqpValidator _validator;

    public ClaimTokenFormatNotPensionDashboardRqpValidatorTests()
    {
        Mock<ILogger<ClaimTokenFormatNotPensionDashboardRqpValidator>> loggerMock = new();
        _validator = new ClaimTokenFormatNotPensionDashboardRqpValidator(loggerMock.Object);
    }

    [Fact]
    public void Validate_ShouldReturnFailure_WhenClaimTokenFormatIsNotPensionsDashboardRQP()
    {
        var result = _validator.Validate(new CdaTokenRequestModel { GrantType = TokenQueryParams.UmaGrantType, ClaimTokenFormat = "random_test" });

        Assert.False(result.IsValid);
        Assert.Equal(TokenValidationMessages.InvalidClaimTokenFormat, result.ErrorMessage);
    }

    [Fact]
    public void Validate_ShouldReturnSuccess_WhenClaimTokenFormatIsPensionsDashboardRQP()
    {
        var result = _validator.Validate(new CdaTokenRequestModel { GrantType = TokenQueryParams.UmaGrantType, ClaimTokenFormat = TokenQueryParams.PensionDashboardRqp });

        Assert.True(result.IsValid);
        Assert.Equal(8, _validator.Order);
    }
}