using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.TokenValidation;
using Microsoft.Extensions.Logging;
using Moq;

namespace MhpdCommonTests.TokenValidationTests;

public class ClientIdInvalidFormatValidationPensionDataTests
{
    private readonly ClientIdInvalidFormatValidationPensionData _invalidFormatValidator;

    public ClientIdInvalidFormatValidationPensionDataTests()
    {
        Mock<ILogger<ClientIdInvalidFormatValidationPensionData>> loggerMock = new();
        _invalidFormatValidator = new ClientIdInvalidFormatValidationPensionData(loggerMock.Object);
    }

    [Fact]
    public void Validate_ShouldReturnFailure_WhenClientIdIsInvalidFormat()
    {
        var result = _invalidFormatValidator.Validate(new PensionsDataRequestModel { ClientId = string.Empty });

        Assert.False(result.IsValid);
        Assert.Equal(TokenValidationMessages.InvalidClientIdFormat, result.ErrorMessage);
    }

    [Fact]
    public void Validate_ShouldReturnSuccess_WhenClientIdIsValidFormat()
    {
        var result = _invalidFormatValidator.Validate(new PensionsDataRequestModel { ClientId = "123e4567-e89b-12d3-a456-426614174000" });
        Assert.True(result.IsValid);
    }
}
