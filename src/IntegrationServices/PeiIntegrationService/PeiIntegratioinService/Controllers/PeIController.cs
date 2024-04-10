using Microsoft.AspNetCore.Mvc;
using PeiIntegratioinService.HttpClients;
using PeiIntegrationService.Models;

namespace PeiIntegratioinService.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class PeIController : ControllerBase
    {
        private readonly ICDAService _iCDAService;
        private const int UserGuidLength = 36;
        private CDAServiceRequestModel? _cdaServiceRequestModel;

        public PeIController(ICDAService iCDAService)
        {
            _iCDAService = iCDAService;
        }

        [HttpGet]
        [Route("/peis")]
        public async Task<IActionResult> GetAsync([FromBody] PeiIntegrationServiceRequestModel requestBody)
        {
            var request = CreateCDAServiceRequestModel(requestBody);

            if (!ValidateCDAServiceRequestModel(request, out var headersOk, out var bodyOk))
            {
                if (!headersOk)
                    return Unauthorized("Unauthorized");

                if (!bodyOk)
                    return BadRequest("Bad Request");
            }

            if (!ValidateAuthHeader(request))
            {
                return Unauthorized("Unauthorized");
            }

            var result = await _iCDAService.GetPies(request);
            
            CreateSuccessResponseHeaders(request);

            return Ok(result!);
        }

        private void CreateSuccessResponseHeaders(CDAServiceRequestModel request)
        {
            var headers = Response.Headers;
            headers.Append("rpt", request.Authorization);
        }

        private bool ValidateAuthHeader(CDAServiceRequestModel request)
        {
            if (string.IsNullOrEmpty(request.Authorization))
            {
                return false;
            }

            return true;
        }

        private bool ValidateCDAServiceRequestModel(CDAServiceRequestModel request, out bool headersOK, out bool bodyOK)
        {
            headersOK = true;
            bodyOK = true;

            if (string.IsNullOrEmpty(request.CdaUserGuid) || string.IsNullOrEmpty(request.Issuer) || string.IsNullOrEmpty(request.UserSessionId))
            {
                headersOK = false;
                bodyOK = false;
                
                return false;
            }

            if (string.IsNullOrEmpty(request.CdaServiceUrl) || string.IsNullOrEmpty(request.RequestId))
            {
                headersOK = true;
                bodyOK = false;
                return false;
            }

            if (!Guid.TryParse(request.CdaUserGuid, out var xUserId))
            {
                headersOK = false;

                return false;
            }

            return true;
        }

        private CDAServiceRequestModel CreateCDAServiceRequestModel (PeiIntegrationServiceRequestModel requestBody)
        {
            Request.Headers.TryGetValue("cdaUserGuid", out var cdaUserGuid);
            Request.Headers.TryGetValue("iss", out var iss);
            Request.Headers.TryGetValue("userSessionId", out var userSessionId);
            Request.Headers.TryGetValue("rpt", out var authorisation);

            _cdaServiceRequestModel = new CDAServiceRequestModel
            {
                Authorization = authorisation.ToString(),
                CdaUserGuid = cdaUserGuid.ToString(),
                Issuer = iss.ToString(),
                UserSessionId = userSessionId.ToString(),
                RequestId = requestBody.RequestId,
                CdaServiceUrl = requestBody.PeisBaseUrl
            };

            return _cdaServiceRequestModel;
        }
    }
}
