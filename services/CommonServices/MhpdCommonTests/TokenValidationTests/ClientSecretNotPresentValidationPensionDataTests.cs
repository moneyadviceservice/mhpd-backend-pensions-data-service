using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.TokenValidation;
using Microsoft.Extensions.Logging;
using Moq;

namespace MhpdCommonTests.TokenValidationTests;

public class ClientSecretNotPresentValidationPensionDataTests
{
    private readonly ClientSecretNotPresentValidationPensionData _notPresentValidationValidator;

    public ClientSecretNotPresentValidationPensionDataTests()
    {
        Mock<ILogger<ClientSecretNotPresentValidationPensionData>> loggerMock = new();
        _notPresentValidationValidator = new ClientSecretNotPresentValidationPensionData(loggerMock.Object);
    }

    [Fact]
    public void Validate_ShouldReturnFailure_WhenClientSecretIsMissing()
    {
        var result = _notPresentValidationValidator.Validate(new PensionsDataRequestModel { ClientSecret = string.Empty });

        Assert.False(result.IsValid);
        Assert.Equal(TokenValidationMessages.InvalidRequest, result.ErrorMessage);
    }

    [Fact]
    public void Validate_ShouldReturnSuccess_WhenClientSecretIsProvided()
    {
        var result = _notPresentValidationValidator.Validate(new PensionsDataRequestModel { ClientSecret = "123e4567-e89b-12d3-a456-426614174000" });
        Assert.True(result.IsValid);
    }
}
