using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.TokenValidation;
using Microsoft.Extensions.Logging;
using Moq;

namespace MhpdCommonTests.TokenValidationTests;

public class ClientIdNotPresentValidationPensionDataTests
{
    private readonly ClientIdNotPresentValidationPensionData _notPresentValidator;

    public ClientIdNotPresentValidationPensionDataTests()
    {
        Mock<ILogger<ClientIdNotPresentValidationPensionData>> loggerMock = new();
        _notPresentValidator = new ClientIdNotPresentValidationPensionData(loggerMock.Object);
    }

    [Fact]
    public void Validate_ShouldReturnFailure_WhenClientIdIsMissing()
    {
        var result = _notPresentValidator.Validate(new PensionsDataRequestModel { ClientId = string.Empty });

        Assert.False(result.IsValid);
        Assert.Equal(TokenValidationMessages.InvalidRequest, result.ErrorMessage);
    }

    [Fact]
    public void Validate_ShouldReturnSuccess_WhenClientIdIsProvided()
    {
        var result = _notPresentValidator.Validate(new PensionsDataRequestModel { ClientId = "123e4567-e89b-12d3-a456-426614174000" });
        Assert.True(result.IsValid);
    }
}
