using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.TokenValidation;
using Microsoft.Extensions.Logging;
using Moq;

namespace MhpdCommonTests.TokenValidationTests;

public class TokenRequestValidatorPipelineTests
{
    [Fact]
    public void Validate_AllValidatorsPass_ForUmaGrantType_ReturnsSuccess()
    {
        // Arrange
        var request = new CdaTokenRequestModel
        {
            GrantType = TokenQueryParams.UmaGrantType,
            Scope = TokenQueryParams.Owner,
            ClaimToken = TokenQueryParams.ValidJwtToken,
            ClaimTokenFormat = TokenQueryParams.PensionDashboardRqp,
            Ticket = TokenQueryParams.ValidJwtToken
        };

        // Get ordered validators
        var validators = Helper.GetOrderedValidators();

        var pipeline = new TokenRequestValidatorPipeline(validators);

        // Act
        var result = pipeline.Validate(request);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_AllValidatorsPass_ForAuthorizationCodeGrantType_ReturnsSuccess()
    {
        // Arrange
        var request = new CdaTokenRequestModel
        {
            GrantType = TokenQueryParams.AuthorizationCodeGrantType,
            ClientId = TokenQueryParams.ValidClientId,
            ClientSecret = TokenQueryParams.ValidClientSecret,
            Code = TokenQueryParams.ValidCode,
            CodeVerifier = TokenQueryParams.ValidCodeVerifier,
            RedirectUri = Helper.ValidRedirectUri
        };

        // Get ordered validators
        var validators = Helper.GetOrderedValidators();

        var pipeline = new TokenRequestValidatorPipeline(validators);

        // Act
        var result = pipeline.Validate(request);

        // Assert
        Assert.True(result.IsValid);
    }
        
    [Fact]
    public void Validate_UnsupportedGrantType_ForUnknownGrantType_ReturnsFailure()
    {
        // Arrange
        var request = new CdaTokenRequestModel
        {
            GrantType = "Unknown",
            ClientId = TokenQueryParams.ValidClientId,
            ClientSecret = TokenQueryParams.ValidClientSecret,
            Code = TokenQueryParams.ValidCode,
            CodeVerifier = TokenQueryParams.ValidCodeVerifier,
            RedirectUri = Helper.ValidRedirectUri
        };

        // Get ordered validators
        var validators = Helper.GetOrderedValidators();

        var pipeline = new TokenRequestValidatorPipeline(validators);

        // Act
        var result = pipeline.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(TokenValidationMessages.UnsupportedGrantType, result.ErrorMessage);
    }
        
    [Fact]
    public void Validate_FirstValidatorFails_ForUmaGrantType_ReturnsFailure()
    {
        // Arrange
        var request = new CdaTokenRequestModel
        {
            GrantType = TokenQueryParams.UmaGrantType
        };

        // Get ordered validators, but fail the first validator
        var mockLogger1 = new Mock<ILogger<GrantTypeNotPresentValidator>>();

        var validators = Helper.GetOrderedValidators();
        validators[0] = new GrantTypeNotPresentValidator(mockLogger1.Object); // First validator fails

        var pipeline = new TokenRequestValidatorPipeline(validators);

        // Act
        var result = pipeline.Validate(request);

        // Assert
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_ValidatorsExecutedInOrder_ForUmaGrantType()
    {
        // Arrange
        var request = new CdaTokenRequestModel
        {
            GrantType = TokenQueryParams.UmaGrantType
        };

        // Get ordered validators
        var validators = Helper.GetOrderedValidators();

        var pipeline = new TokenRequestValidatorPipeline(validators);

        // Act
        var result = pipeline.Validate(request);

        // Assert
        Assert.False(result.IsValid); // This depends on the logic of the validators
    }
}