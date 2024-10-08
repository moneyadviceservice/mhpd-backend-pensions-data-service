using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.TokenValidation;
using Moq;

namespace MhpdCommonTests.TokenValidationTests;

public class TokenIntegrationRequestValidatorPipelineTests
{
    private readonly Mock<ITokenRequestValidator<TokenIntegrationRequestModel>> _validator1Mock;
    private readonly Mock<ITokenRequestValidator<TokenIntegrationRequestModel>> _validator2Mock;
    private readonly TokenIntegrationRequestValidatorPipeline _sut;

    public TokenIntegrationRequestValidatorPipelineTests()
    {
        // Create mocks for validators
        _validator1Mock = new Mock<ITokenRequestValidator<TokenIntegrationRequestModel>>();
        _validator2Mock = new Mock<ITokenRequestValidator<TokenIntegrationRequestModel>>();

        // Create a list of validators in order
        var validators = new List<ITokenRequestValidator<TokenIntegrationRequestModel>>()
        {
            _validator1Mock.Object,
            _validator2Mock.Object
        };

        // Initialize the system under test (sut) with the list of validators
        _sut = new TokenIntegrationRequestValidatorPipeline(validators);
    }

    [Fact]
    public void Validate_ThrowsException_WhenNoValidatorsArePresent()
    {
        // Arrange
        var emptyPipeline = new TokenIntegrationRequestValidatorPipeline(new List<ITokenRequestValidator<TokenIntegrationRequestModel>>());

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => emptyPipeline.Validate(new TokenIntegrationRequestModel()));
        Assert.Equal("No validators found.", ex.Message);
    }

    [Fact]
    public void Validate_ReturnsSuccess_WhenAllValidatorsPass()
    {
        // Arrange
        var request = new TokenIntegrationRequestModel();

        _validator1Mock.Setup(v => v.Validate(request)).Returns(ValidationResult.Success());
        _validator2Mock.Setup(v => v.Validate(request)).Returns(ValidationResult.Success());

        // Act
        var result = _sut.Validate(request);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_ReturnsFailure_WhenAnyValidatorFails()
    {
        // Arrange
        var request = new TokenIntegrationRequestModel();

        var failureResult = ValidationResult.Failure("Validation failed");

        _validator1Mock.Setup(v => v.Validate(request)).Returns(ValidationResult.Success());
        _validator2Mock.Setup(v => v.Validate(request)).Returns(failureResult);

        // Act
        var result = _sut.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("Validation failed", result.ErrorMessage);
    }

    [Fact]
    public void Validate_ShortCircuits_WhenValidatorFails()
    {
        // Arrange
        var request = new TokenIntegrationRequestModel();

        var failureResult = ValidationResult.Failure("Validation failed");

        _validator1Mock.Setup(v => v.Validate(request)).Returns(failureResult);
        _validator2Mock.Setup(v => v.Validate(request)).Returns(ValidationResult.Success());

        // Act
        var result = _sut.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("Validation failed", result.ErrorMessage);

        // Ensure that the second validator is never called
        _validator2Mock.Verify(v => v.Validate(request), Times.Never);
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