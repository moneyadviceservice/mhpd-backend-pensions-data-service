using System.Net.Http.Headers;
using CDAServiceEmulator.Mocks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace CDAServiceEmulator.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PeisController : ControllerBase
{
    private const int PeisIdLength = 36;

    [HttpGet]
    [Route("/peis/{peis_id}")]
    public async Task<IActionResult> GetAsync([FromRoute] string peis_id)
    {
        if (!ValidateAuthHeader())
        {
            return Unauthorized("Unauthorized");
        }

        if (!Validate(peis_id))
        {
            return BadRequest("Bad Request");
        }

        return Ok(await PiesMockService.GetPeisJsonMock());
    }

    private bool ValidateAuthHeader()
    {
        var accessToken = Request.Headers[HeaderNames.Authorization];
        var parameter = string.Empty;

        if (AuthenticationHeaderValue.TryParse(accessToken, out var headerValue))
        {
            var scheme = headerValue.Scheme;
            if (scheme != "Bearer")
            {
                return false;
            }

            parameter = headerValue.Parameter;
        }

        if (string.IsNullOrEmpty(parameter))
        {
            var headers = Response.Headers;
            headers.Append("WWW-Authenticate", "realm=\"PensionDashboard\", " +
                                               "as_uri=\"https://as.pdp.com\", " +
                                               "ticket=\"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.cThIIoDvwdueQB468K5xDc5633seEFoqwxjF_xSJyQQ\"");
                
            return false;
        }

        return true;

    }

    private bool Validate(string peisId)
    {
        Request.Headers.TryGetValue("X-Request-ID", out var xRequestId);

        Guid.TryParse(xRequestId, out var xRequestIdGuid);
        if (xRequestIdGuid == Guid.Empty || xRequestId.ToString().Length != PeisIdLength)
        {
            return false;
        }

        Guid.TryParse(peisId, out var peisIdGuid);
        if (peisIdGuid == Guid.Empty || peisId.Length != PeisIdLength)
        {
            return false;
        }

        return true;
    }
}