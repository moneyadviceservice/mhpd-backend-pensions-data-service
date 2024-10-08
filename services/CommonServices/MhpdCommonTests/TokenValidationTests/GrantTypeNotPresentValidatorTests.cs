using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.TokenValidation;
using Microsoft.Extensions.Logging;
using Moq;

namespace MhpdCommonTests.TokenValidationTests;

public class GrantTypeNotPresentValidatorTests
{
    private readonly GrantTypeNotPresentValidator _notPresentValidator;

    public GrantTypeNotPresentValidatorTests()
    {
        Mock<ILogger<GrantTypeNotPresentValidator>> loggerMock = new();
        _notPresentValidator = new GrantTypeNotPresentValidator(loggerMock.Object);
    }

    [Fact]
    public void Validate_ShouldReturnFailure_WhenGrantTypeIsMissing()
    {
        var result = _notPresentValidator.Validate(new CdaTokenRequestModel { GrantType = "" });

        Assert.False(result.IsValid);
        Assert.Equal(TokenValidationMessages.MissingGrantType, result.ErrorMessage);
    }

    [Fact]
    public void Validate_ShouldReturnSuccess_WhenGrantTypeIsProvided()
    {
        var result = _notPresentValidator.Validate(new CdaTokenRequestModel { GrantType = "valid_grant_type" });

        Assert.True(result.IsValid);
        Assert.Equal(1, _notPresentValidator.Order);
    }
}
