using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using PDPViewDataServicedEmulator.CosmosRepository;
using PDPViewDataServicedEmulator.Mocks;
using PDPViewDataServicedEmulator.Models;
using static PDPViewDataServicedEmulator.Utils.ViewDataTokenUtils;

namespace PDPViewDataServicedEmulator.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PDPViewDataController(ICosmosDbRepository<ViewDataPayload> cosmosDbRepository) : ControllerBase
    {
        private const string Owner = "owner";
        private const string BearerValue = "Bearer";
        private const string Subject =   "324bqfw348f9q4398h3";       
        private const string Audience = "https://pdp/ig/token";
        private const string Kid = "c00b40ea-6da1-408a-a6c9-17b1ff45bb9a";
        private const string Issuer = "DATA_PROVIDER_1fd1da88-9fb3-461c-a48a-3dba21bfba17";

        [HttpGet]
        [Route("/view-data/{assetGuid?}")]
        public async Task<IActionResult> GetAsync([FromRoute] string? assetGuid, [FromQuery] string? scope)
        {
            if (!ValidateAuthHeader())
                return Unauthorized("Unauthorized");

            Request.Headers.TryGetValue("X-Request-ID", out var xRequestId);

            if (!ValidateGuid(xRequestId!) || !ValidateGuid(assetGuid!) || string.IsNullOrEmpty(scope) || scope != Owner)
                return BadRequest("Bad Request");

            var viewData = await cosmosDbRepository.GetByIdAsync(assetGuid, assetGuid);

            if (viewData == null)
                return NotFound("Not Found");

            var viewDataToken = GenerateViewDataToken(Kid, Audience, Subject, 
                new ViewDataPayload { AssetGuid = viewData.AssetGuid, 
                                    ViewData = viewData.ViewData },
                Issuer);

            return Ok(await Task.FromResult(new ViewDataResponseModel { ViewDataToken = viewDataToken }));
        }

        private bool ValidateAuthHeader()
        {
            var accessTokenValue = Request.Headers[HeaderNames.Authorization];
            var parameter = string.Empty;
            if (AuthenticationHeaderValue.TryParse(accessTokenValue, out var headerValue))
            {
                var scheme = headerValue.Scheme;
                if (scheme != BearerValue)
                {
                    return false;
                }
                parameter = headerValue.Parameter;
            }
            if (string.IsNullOrEmpty(parameter))
            {
                var headers = Response.Headers;
                headers.Append("WWW-Authenticate", "realm=\"PensionDashboard\", " +
                        $"as_uri=\"https://pdp/ig/token\", " +
                        "ticket=\"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.cThIIoDvwdueQB468K5xDc5633seEFoqwxjF_xSJyQQ\"");
                return false;

            }
            return true;
        }

        private bool ValidateGuid(string guid)
        {
            Guid.TryParse(guid, out var xguid);

            if (xguid == Guid.Empty || string.IsNullOrEmpty(guid))
                return false;

            return true;
        }
        private string GenerateViewDataToken(string kid, string audience, string subject, ViewDataPayload viewData, string issuer)
        {
            var tokenManager = new ViewDataTokenManager(kid, audience, subject, viewData, issuer);
            return tokenManager.GenerateToken();
        }
    }
}