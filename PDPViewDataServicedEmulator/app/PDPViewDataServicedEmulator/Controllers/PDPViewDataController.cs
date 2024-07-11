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
    public class PDPViewDataController : ControllerBase
    {
        public readonly string Owner = "owner";
        private readonly string BearerValue = "Bearer";
        private readonly string Subject = "324bqfw348f9q4398h3";
        private readonly IViewDataRepository _viewDataRepository;
        private readonly string Audience = "https://pdp/ig/token";
        private readonly string Kid = "c00b40ea-6da1-408a-a6c9-17b1ff45bb9a";
        private readonly string Issuer = "Provider1-75b68255-444e-4d5f-bbfe-249c26d69963";

        public PDPViewDataController(IViewDataRepository viewDataRepository)
        {
            _viewDataRepository = viewDataRepository;
        }

        [HttpGet]
        [Route("/view-data/{asset_guid?}")]
        public async Task<IActionResult> GetAsync([FromRoute] string? asset_guid, [FromQuery] string? scope)
        {
            if (!ValidateAuthHeader())
                return Unauthorized("Unauthorized");

            Request.Headers.TryGetValue("X-Request-ID", out var xRequestId);

            if (!ValidateGuid(xRequestId!) || !ValidateGuid(asset_guid!) || string.IsNullOrEmpty(scope) || scope != Owner)
                return BadRequest("Bad Request");

            var viewData =  _viewDataRepository.GetViewData(asset_guid!);

            if (viewData == null)
                return NotFound("Not Found");

            string viewDataToken = GenerateViewDataToken(Kid, Audience, Subject, viewData, Issuer);

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
            ViewDataTokenManager _tokenManager = new ViewDataTokenManager(kid, audience, subject, viewData, issuer);
            return _tokenManager.GenerateToken();
        }
    }
}