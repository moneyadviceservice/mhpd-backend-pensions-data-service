using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.TokenValidation;
using MhpdCommon.Utils;
using Microsoft.Extensions.Logging;
using Moq;

namespace MhpdCommonTests.TokenValidationTests;

public class ClientSecretNotGuidValidationTests
{
    private readonly ClientSecretNotGuidValidation _notGuidValidator;

    public ClientSecretNotGuidValidationTests()
    {
        Mock<ILogger<ClientSecretNotGuidValidation>> loggerMock = new();
        Mock<IdValidator> idValidator = new();
        _notGuidValidator = new ClientSecretNotGuidValidation(loggerMock.Object, idValidator.Object);
    }

    [Fact]
    public void Validate_ShouldReturnFailure_WhenClientSecretIsNotGuid()
    {
        var result = _notGuidValidator.Validate(new CdaTokenRequestModel { ClientSecret = "test-string" });

        Assert.False(result.IsValid);
        Assert.Equal(TokenValidationMessages.InvalidClientSecretFormat, result.ErrorMessage);
    }

    [Fact]
    public void Validate_ShouldReturnSuccess_WhenClientSecretIsGuid()
    {
        var result = _notGuidValidator.Validate(new CdaTokenRequestModel { ClientSecret = "123e4567-e89b-12d3-a456-426614174000" });
        Assert.True(result.IsValid);
    }
}
