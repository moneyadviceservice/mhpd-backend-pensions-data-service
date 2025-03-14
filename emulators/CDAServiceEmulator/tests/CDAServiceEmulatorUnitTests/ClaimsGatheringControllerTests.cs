using CDAServiceEmulator.Controllers;
using MhpdCommon.Constants;
using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.Models.MHPDModels.JwkUri;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace CDAServiceEmulatorUnitTests;

public class ClaimsGatheringControllerTests
{
    private readonly ClaimsGatheringController _controller;

    public ClaimsGatheringControllerTests()
    {
        Mock<ILogger<ClaimsGatheringController>> logger = new();

        _controller = new ClaimsGatheringController(logger.Object);
    }

    [Theory]
    [InlineData("This is not a url", SecurityConstants.Jwe.ClaimsRequiredPermissionTicket, JwkConstants.TestTokenValidKey, "requestId", "clientId", "find")]
    [InlineData("http://dashboard.mhpd.com", SecurityConstants.Jwe.AuthorizationRequiredPermissionTicket, JwkConstants.TestTokenValidKey, "requestId", "clientId", "find")]
    [InlineData("http://dashboard.mhpd.com", SecurityConstants.Jwe.ClaimsRequiredPermissionTicket, "NotAnRqp", "requestId", "clientId", "find")]
    [InlineData("http://dashboard.mhpd.com", SecurityConstants.Jwe.ClaimsRequiredPermissionTicket, JwkConstants.TestTokenValidKey, "", "clientId", "find")]
    [InlineData("http://dashboard.mhpd.com", SecurityConstants.Jwe.ClaimsRequiredPermissionTicket, JwkConstants.TestTokenValidKey, "requestId", "", "find")]
    [InlineData("http://dashboard.mhpd.com", SecurityConstants.Jwe.ClaimsRequiredPermissionTicket, JwkConstants.TestTokenValidKey, "requestId", "clientId", "")]
    public void WhenClaimsGatheringCalledWithInvalidRequest_ReturnsBadRequestResponse(string redirectUri, string ticket, string rqp, string requestId, string clientId, string state)
    {
        var request = new ClaimsGatheringRequestModel
        {
            ClaimsRedirectUri = redirectUri,
            ClientId = clientId,
            RequestId = requestId,
            State = state,
            Ticket = ticket,
            Rqp = rqp
        };

        var result = _controller.CollectClaims(request);

        Assert.True(result.GetType() == typeof(BadRequestObjectResult));
    }

    [Fact]
    public void WhenClaimsGatheringCalledWithValidRequest_ReturnsRedirectResponse()
    {
        var request = new ClaimsGatheringRequestModel
        {
            ClaimsRedirectUri = "https://www.dashboard.mhpd.com",
            ClientId = "clientId",
            RequestId = "requestId",
            State = "find",
            Ticket = SecurityConstants.Jwe.ClaimsRequiredPermissionTicket,
            Rqp = JwkConstants.TestTokenValidKey
        };

        var result = _controller.CollectClaims(request);

        Assert.True(result.GetType() == typeof(RedirectResult));
    }
}
