using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.TokenValidation;
using Microsoft.Extensions.Logging;
using Moq;

namespace MhpdCommonTests.TokenValidationTests;

public class CodeVerifierNotPresentValidationTests
{
    private readonly CodeVerifierNotPresentValidation _notPresentValidator;

    public CodeVerifierNotPresentValidationTests()
    {
        Mock<ILogger<CodeVerifierNotPresentValidation>> loggerMock = new();
        _notPresentValidator = new CodeVerifierNotPresentValidation(loggerMock.Object);
    }

    [Fact]
    public void Validate_ShouldReturnFailure_WhenCodeVerifierIsMissing()
    {
        var result = _notPresentValidator.Validate(new CdaTokenRequestModel { CodeVerifier = string.Empty });

        Assert.False(result.IsValid);
        Assert.Equal(TokenValidationMessages.InvalidRequest, result.ErrorMessage);
    }

    [Fact]
    public void Validate_ShouldReturnSuccess_WhenCodeVerifierIsProvided()
    {
        var result = _notPresentValidator.Validate(new CdaTokenRequestModel { CodeVerifier = TokenQueryParams.ValidCodeVerifier });
        Assert.True(result.IsValid);
    }
}
