using MhpdCommon.Extensions;
using MhpdCommon.Models.MHPDModels.JwkUri;
using Microsoft.AspNetCore.Mvc;

namespace CDAServiceEmulator.Controllers;

[Route("/")]
[ApiController]
public class JwkUriController(ILogger<JwkUriController> logger) : ControllerBase
{
    [HttpGet]
    [Route("jwk_uri")]
    public Task<IActionResult> GetAsync()
    {
        var response = new JwkUriResponseModel();
        logger.LogResponse(response);
        return Task.FromResult<IActionResult>(Ok(response));
    }
}