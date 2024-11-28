using System.Net.Http.Headers;
using MhpdCommon.Constants;
using MhpdCommon.Models.MHPDModels;
using MhpdCommon.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using PDPViewDataServiceEmulator.CosmosRepository;
using PDPViewDataServiceEmulator.Models;

namespace PDPViewDataServiceEmulator.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PdpViewDataController(ILogger<PdpViewDataController> logger,
        ViewDataRepository viewDataRepository,
        IIdValidator validator,
        ITokenUtility tokenUtility) : ControllerBase
    {
        private const string Owner = "owner";
        private const string BearerValue = "Bearer";

        [HttpGet]
        [Route("/view-data/{assetGuid?}")]
        public async Task<IActionResult> GetAsync([FromRoute] string? assetGuid, [FromQuery] string? scope,
            [FromHeader(Name = HeaderConstants.RequestId)] string? xRequestId)
        {
            if (!ValidateAuthHeader())
            {
                logger.LogError("Unauthorized");
                return Unauthorized("Unauthorized");
            }

            if (!validator.IsValidGuid(xRequestId!) ||
                !validator.IsValidGuid(assetGuid!) ||
                string.IsNullOrEmpty(scope) || scope != Owner ||
                string.IsNullOrEmpty(assetGuid))
            {
                logger.LogError("Bad Request");
                return BadRequest("Bad Request");
            }

            var viewData = await viewDataRepository.GetByIdAsync(assetGuid, assetGuid);
            if (viewData == null)
            {
                logger.LogError("View data not found for the given assetGuid {AssetGuid}", assetGuid);
                return NotFound("Not Found");
            }

            var viewDataToken = tokenUtility.GenerateJwt(new CustomClaimDataModel
            {
                Name = "view_data",
                Data = viewData.ViewData?.ToString()
            });

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
    }
}