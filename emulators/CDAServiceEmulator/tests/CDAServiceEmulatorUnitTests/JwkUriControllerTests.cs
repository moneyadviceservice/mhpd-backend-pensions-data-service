using System.Net;
using CDAServiceEmulator.Controllers;
using MhpdCommon.Models.MHPDModels.JwkUri;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace CDAServiceEmulatorUnitTests;

public class JwkUriControllerTests
{
    [Theory]
    [InlineData(JwkConstants.KeyType, JwkConstants.KeyId, JwkConstants.Modulus, JwkConstants.Exponent)]
    public async Task GetAsync_ReturnsExpectedJwkResponse(string expectedKeyType, string expectedKeyId, string expectedModulus, string expectedExponent)
    {
        // Arrange
        Mock<ILogger<JwkUriController>> mockLogger = new();
        var controller = new JwkUriController(mockLogger.Object);

        // Act
        var result = await controller.GetAsync() as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal((int)HttpStatusCode.OK, result.StatusCode);

        var responseModel = result.Value as JwkUriResponseModel;
        Assert.NotNull(responseModel);

        var key = responseModel.Keys.First(k => k.KeyId == expectedKeyId);
        Assert.Equal(expectedKeyType, key.KeyType);
        Assert.Equal(expectedKeyId, key.KeyId);
        Assert.Equal(expectedModulus, key.Modulus);
        Assert.Equal(expectedExponent, key.Exponent);
    }
}