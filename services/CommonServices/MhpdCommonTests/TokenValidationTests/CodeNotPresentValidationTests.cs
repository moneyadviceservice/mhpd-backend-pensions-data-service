using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.TokenValidation;
using Microsoft.Extensions.Logging;
using Moq;

namespace MhpdCommonTests.TokenValidationTests;

public class CodeNotPresentValidationTests
{
    private readonly CodeNotPresentValidation _notPresentValidationValidator;

    public CodeNotPresentValidationTests()
    {
        Mock<ILogger<CodeNotPresentValidation>> loggerMock = new();
        _notPresentValidationValidator = new CodeNotPresentValidation(loggerMock.Object);
    }

    [Fact]
    public void Validate_ShouldReturnFailure_WhenCodeIsMissing()
    {
        var result = _notPresentValidationValidator.Validate(new CdaTokenRequestModel { Code = string.Empty });

        Assert.False(result.IsValid);
        Assert.Equal(TokenValidationMessages.InvalidRequest, result.ErrorMessage);
    }

    [Fact]
    public void Validate_ShouldReturnSuccess_WhenCodeIsProvided()
    {
        var result = _notPresentValidationValidator.Validate(new CdaTokenRequestModel { Code = "123e4567-e89b-12d3-a456-426614174000" });
        Assert.True(result.IsValid);
    }
}
