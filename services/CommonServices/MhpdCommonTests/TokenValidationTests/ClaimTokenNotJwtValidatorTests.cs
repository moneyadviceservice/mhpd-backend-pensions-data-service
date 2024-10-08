using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.TokenValidation;
using Microsoft.Extensions.Logging;
using Moq;

namespace MhpdCommonTests.TokenValidationTests;

public class ClaimTokenNotJwtValidatorTests
{
    private readonly ClaimTokenNotJwtValidator _validator;

    public ClaimTokenNotJwtValidatorTests()
    {
        Mock<ILogger<ClaimTokenNotJwtValidator>> loggerMock = new();
        _validator = new ClaimTokenNotJwtValidator(loggerMock.Object);
    }

    [Fact]
    public void Validate_ShouldReturnFailure_WhenTicketIsNotJwt()
    {
        // Arrange
        var request = new CdaTokenRequestModel
        {
            ClaimToken = "invalid.jwt.token", // Invalid JWT format
            GrantType = TokenQueryParams.UmaGrantType
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid); // Validation should fail
        Assert.Equal(TokenValidationMessages.InvalidClaimToken, result.ErrorMessage);
    }

    [Fact]
    public void Validate_ShouldReturnSuccess_WhenTicketIsValidJwt()
    {
        // Arrange
        var request = new CdaTokenRequestModel
        {
            ClaimToken = TokenQueryParams.ValidJwtToken,
            GrantType = TokenQueryParams.UmaGrantType
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.True(result.IsValid); // Validation should succeed
        Assert.Equal(6, _validator.Order);
    }
}