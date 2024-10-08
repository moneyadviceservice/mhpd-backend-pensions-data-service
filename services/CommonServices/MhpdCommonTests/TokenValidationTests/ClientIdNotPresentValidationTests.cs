using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.TokenValidation;
using Microsoft.Extensions.Logging;
using Moq;

namespace MhpdCommonTests.TokenValidationTests;

public class ClientIdNotPresentValidationTests
{
    private readonly ClientIdNotPresentValidation _notPresentValidator;

    public ClientIdNotPresentValidationTests()
    {
        Mock<ILogger<ClientIdNotPresentValidation>> loggerMock = new();
        _notPresentValidator = new ClientIdNotPresentValidation(loggerMock.Object);
    }

    [Fact]
    public void Validate_ShouldReturnFailure_WhenClientIdIsMissing()
    {
        var result = _notPresentValidator.Validate(new CdaTokenRequestModel { ClientId = string.Empty });

        Assert.False(result.IsValid);
        Assert.Equal(TokenValidationMessages.InvalidRequest, result.ErrorMessage);
    }

    [Fact]
    public void Validate_ShouldReturnSuccess_WhenClientIdIsProvided()
    {
        var result = _notPresentValidator.Validate(new CdaTokenRequestModel { ClientId = "123e4567-e89b-12d3-a456-426614174000" });
        Assert.True(result.IsValid);
    }
}
