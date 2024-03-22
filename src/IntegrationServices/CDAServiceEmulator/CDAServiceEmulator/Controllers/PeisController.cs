using System.Text.RegularExpressions;
using PeIsServiceEmulator.Mocks;
using Microsoft.AspNetCore.Mvc;
using PeIsServiceEmulator.Models.Peis;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PeisServiceEmulator.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PeisController : ControllerBase
    {
        private const int UserGuidLength = 36;

        public PeisController()
        {
        }

        [HttpGet]
        [Route("/peis/{user_guid}")]
        public async Task<IActionResult> GetAsync([FromRoute] string user_guid, [FromQuery] string? scope)
        {
            if (!ValidateAuthHeader())
            {
                return Unauthorized("Unauthorized");
            }

            if (Validate(user_guid, scope!) == false)
            {
                return BadRequest("Bad Request");
            }

            return Ok(await MockService.GetPeisJsonMock());
        }

        private bool ValidateAuthHeader ()
        {
            Request.Headers.TryGetValue("Authorisation", out var authorisation);

            if (string.IsNullOrEmpty(authorisation.ToString()))
            {
                var headers = Response.Headers;
                headers.Append("WWW-Authenticate", "realm=\"PensionDashboard\", " +
                    "as_uri=\"https://as.pdp.com\", " +
                    "ticket=\"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.cThIIoDvwdueQB468K5xDc5633seEFoqwxjF_xSJyQQ\"");
                return false;
            }

            return true;

        }

        private bool Validate(string userGuid, string scope)
        {
            Request.Headers.TryGetValue("X-Request-ID", out var xRequestId);
            Request.Headers.TryGetValue("X-Version", out var xVersion);

            Guid.TryParse(userGuid, out var xUserId);

            if (xUserId == Guid.Empty || xUserId.ToString().Length != UserGuidLength || string.IsNullOrEmpty(xRequestId))
            {
                return false;
            }

            Regex apiVersionRegex = new Regex("[0-9]+\\.[0-9]+");
            Match m = apiVersionRegex.Match(xVersion.ToString());
            if (!m.Success)
            {
                return false;
            }

            if (!string.IsNullOrEmpty(scope))
            {
                if (!(scope == ScopeEnum.Owner || scope == ScopeEnum.UmaProtection))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
