using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.TokenValidation;
using MhpdCommon.Utils;
using Microsoft.Extensions.Logging;
using Moq;

namespace MhpdCommonTests.TokenValidationTests;

public class ClientSecretNotGuidValidationPensionDataTests
{
    private readonly ClientSecretNotGuidValidationPensionData _notGuidValidator;

    public ClientSecretNotGuidValidationPensionDataTests()
    {
        Mock<ILogger<ClientSecretNotGuidValidationPensionData>> loggerMock = new();
        Mock<IdValidator> idValidator = new();
        _notGuidValidator = new ClientSecretNotGuidValidationPensionData(loggerMock.Object, idValidator.Object);
    }

    [Fact]
    public void Validate_ShouldReturnFailure_WhenClientSecretIsNotGuid()
    {
        var result = _notGuidValidator.Validate(new PensionsDataRequestModel { ClientSecret = "test-string" });

        Assert.False(result.IsValid);
        Assert.Equal(TokenValidationMessages.InvalidClientSecretFormat, result.ErrorMessage);
    }

    [Fact]
    public void Validate_ShouldReturnSuccess_WhenClientSecretIsGuid()
    {
        var result = _notGuidValidator.Validate(new PensionsDataRequestModel { ClientSecret = "123e4567-e89b-12d3-a456-426614174000" });
        Assert.True(result.IsValid);
    }
}
