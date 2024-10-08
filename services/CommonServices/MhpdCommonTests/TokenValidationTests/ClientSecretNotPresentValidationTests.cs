using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.TokenValidation;
using Microsoft.Extensions.Logging;
using Moq;

namespace MhpdCommonTests.TokenValidationTests;

public class ClientSecretNotPresentValidationTests
{
    private readonly ClientSecretNotPresentValidation _notPresentValidationValidator;

    public ClientSecretNotPresentValidationTests()
    {
        Mock<ILogger<ClientSecretNotPresentValidation>> loggerMock = new();
        _notPresentValidationValidator = new ClientSecretNotPresentValidation(loggerMock.Object);
    }

    [Fact]
    public void Validate_ShouldReturnFailure_WhenClientSecretIsMissing()
    {
        var result = _notPresentValidationValidator.Validate(new CdaTokenRequestModel { ClientSecret = string.Empty });

        Assert.False(result.IsValid);
        Assert.Equal(TokenValidationMessages.InvalidRequest, result.ErrorMessage);
    }

    [Fact]
    public void Validate_ShouldReturnSuccess_WhenClientSecretIsProvided()
    {
        var result = _notPresentValidationValidator.Validate(new CdaTokenRequestModel { ClientSecret = "123e4567-e89b-12d3-a456-426614174000" });
        Assert.True(result.IsValid);
    }
}
