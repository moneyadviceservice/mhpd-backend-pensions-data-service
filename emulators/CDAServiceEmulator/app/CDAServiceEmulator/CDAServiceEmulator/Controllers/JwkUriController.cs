using CDAServiceEmulator.Models.JwkUri;
using MhpdCommon.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace CDAServiceEmulator.Controllers;

[Route("/")]
[ApiController]
public class JwkUriController(ILogger<JwkUriController> logger) : ControllerBase
{
    [HttpGet]
    [Route("jwk-uri")]
    public Task<IActionResult> GetAsync()
    {
        var response = new JwkUriResponseModel();
        logger.LogResponse(response);
        return Task.FromResult<IActionResult>(Ok(response));
    }
}