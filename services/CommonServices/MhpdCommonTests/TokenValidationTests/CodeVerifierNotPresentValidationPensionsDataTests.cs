using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.TokenValidation;
using Microsoft.Extensions.Logging;
using Moq;

namespace MhpdCommonTests.TokenValidationTests;

public class CodeVerifierNotPresentValidationPensionsDataTests
{
    private readonly CodeVerifierNotPresentValidationPensionsData _notPresentValidator;

    public CodeVerifierNotPresentValidationPensionsDataTests()
    {
        Mock<ILogger<CodeVerifierNotPresentValidationPensionsData>> loggerMock = new();
        _notPresentValidator = new CodeVerifierNotPresentValidationPensionsData(loggerMock.Object);
    }

    [Fact]
    public void Validate_ShouldReturnFailure_WhenCodeVerifierIsMissing()
    {
        var result = _notPresentValidator.Validate(new PensionsDataRequestModel { CodeVerifier = string.Empty });

        Assert.False(result.IsValid);
        Assert.Equal(TokenValidationMessages.CodeVerifierNotPresent, result.ErrorMessage);
    }

    [Fact]
    public void Validate_ShouldReturnSuccess_WhenCodeVerifierIsProvided()
    {
        var result = _notPresentValidator.Validate(new PensionsDataRequestModel { CodeVerifier = TokenQueryParams.ValidCodeVerifier });
        Assert.True(result.IsValid);
    }
}
