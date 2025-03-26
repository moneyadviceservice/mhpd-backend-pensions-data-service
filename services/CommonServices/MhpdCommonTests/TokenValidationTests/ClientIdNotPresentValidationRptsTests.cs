using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.TokenValidation;
using Microsoft.Extensions.Logging;
using Moq;

namespace MhpdCommonTests.TokenValidationTests;

public class ClientIdNotPresentValidationRptsTests
{
    private readonly ClientIdNotPresentValidationRpts _invalidFormatValidator;

    public ClientIdNotPresentValidationRptsTests()
    {
        Mock<ILogger<ClientIdNotPresentValidationRpts>> loggerMock = new();
        _invalidFormatValidator = new ClientIdNotPresentValidationRpts(loggerMock.Object);
    }

    [Fact]
    public void Validate_ShouldReturnFailure_WhenClientIdIsInvalidFormat()
    {
        var result = _invalidFormatValidator.Validate(new TokenClientRequestModel { ClientId = string.Empty });

        Assert.False(result.IsValid);
        Assert.Equal(TokenValidationMessages.InvalidRequest, result.ErrorMessage);
    }

    [Fact]
    public void Validate_ShouldReturnSuccess_WhenClientIdIsValidFormat()
    {
        var result = _invalidFormatValidator.Validate(new TokenClientRequestModel { ClientId = "123e4567-e89b-12d3-a456-426614174000" });
        Assert.True(result.IsValid);
    }
}
