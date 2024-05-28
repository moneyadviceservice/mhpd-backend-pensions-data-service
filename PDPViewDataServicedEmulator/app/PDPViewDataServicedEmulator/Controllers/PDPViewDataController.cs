using Microsoft.AspNetCore.Mvc;

namespace PDPViewDataServicedEmulator.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PDPViewDataController : ControllerBase
    {   
        private const string CDATokenEmulatorEndPoint = "https://auth-server/";

        public PDPViewDataController() {}

        [HttpGet]
        [Route("/view-data/{asset_guid?}")]
        public async Task<IActionResult> GetAsync([FromRoute] string? asset_guid, [FromQuery] string? scope)
        {
            if (!ValidateAuthHeader())
            {
                return Unauthorized("Unauthorized");
            }

            return await Task.FromResult(Ok());
        }

        private bool ValidateAuthHeader()
        {
            Request.Headers.TryGetValue("Authorisation", out var authorisation);

            if (string.IsNullOrEmpty(authorisation.ToString()))
            {
                var headers = Response.Headers;
                headers.Append("WWW-Authenticate", "realm=\"PensionDashboard\", " +
                    $"as_uri=\"{CDATokenEmulatorEndPoint}\", " +
                    "ticket=\"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.cThIIoDvwdueQB468K5xDc5633seEFoqwxjF_xSJyQQ\"");
                return false;
            }

            return true;
        }
    }
}
