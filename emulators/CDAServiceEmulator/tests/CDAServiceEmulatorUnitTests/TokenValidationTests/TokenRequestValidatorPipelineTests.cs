using CDAServiceEmulator.Models.Token;
using CDAServiceEmulator.TokenValidation;
using Moq;

namespace CDAServiceEmulatorUnitTests.TokenValidationTests;

public class TokenRequestValidatorPipelineTests
{
    [Fact]
    public void Validate_AllValidatorsPass_ReturnsSuccess()
    {
        // Arrange
        var request = new CdaTokenRequestModel(); // Create a valid request model
        var validators = new List<ITokenRequestValidator>
        {
            CreateValidatorMock(1, true).Object,
            CreateValidatorMock(2, true).Object,
            CreateValidatorMock(3, true).Object
        };

        var pipeline = new TokenRequestValidatorPipeline(validators);

        // Act
        var result = pipeline.Validate(request);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_FirstValidatorFails_ReturnsFailure()
    {
        // Arrange
        var request = new CdaTokenRequestModel(); // Create a valid request model
        var validators = new List<ITokenRequestValidator>
        {
            CreateValidatorMock(1, false).Object, // This validator fails
            CreateValidatorMock(2, true).Object,
            CreateValidatorMock(3, true).Object
        };

        var pipeline = new TokenRequestValidatorPipeline(validators);

        // Act
        var result = pipeline.Validate(request);

        // Assert
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_ValidatorsExecutedInOrder()
    {
        // Arrange
        var request = new CdaTokenRequestModel(); // Create a valid request model
        var mock1 = CreateValidatorMock(1, true); // First, should pass
        var mock2 = CreateValidatorMock(2, false); // Second, should fail
        var mock3 = CreateValidatorMock(3, true); // Should not be executed

        var validators = new List<ITokenRequestValidator>
        {
            mock1.Object,
            mock2.Object,
            mock3.Object
        };

        var pipeline = new TokenRequestValidatorPipeline(validators);

        // Act
        var result = pipeline.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        // Ensure the last validator was not called
        mock1.Verify(v => v.Validate(It.IsAny<CdaTokenRequestModel>()), Times.Once());
        mock2.Verify(v => v.Validate(It.IsAny<CdaTokenRequestModel>()), Times.Once());
        mock3.Verify(v => v.Validate(It.IsAny<CdaTokenRequestModel>()), Times.Never());
    }

    private static Mock<ITokenRequestValidator> CreateValidatorMock(int order, bool isValid)
    {
        var mock = new Mock<ITokenRequestValidator>();
        mock.Setup(v => v.Order).Returns(order);
        mock.Setup(v => v.Validate(It.IsAny<CdaTokenRequestModel>()))
            .Returns(isValid ? ValidationResult.Success() : ValidationResult.Failure("Validation failed"));
        return mock;
    }
}