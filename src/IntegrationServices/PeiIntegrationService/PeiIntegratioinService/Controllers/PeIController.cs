using System.Net;
using Microsoft.AspNetCore.Mvc;
using PeiIntegrationService.HttpClients.Interfaces;
using PeiIntegrationService.Models.CdaPiesService;
using PeiIntegrationService.Models.MapsCdaService;
using PeiIntegrationService.Models.PeiIntegrationService;
using PeiIntegrationService.Models.TokenIntegrationService;

namespace PeiIntegratioinService.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class PeIController : ControllerBase
    {
        private const int GuidLength = 36;
        private readonly ICdaPiesServiceClient _iCDAPiesService;
        private readonly IMapsRqpServiceClient _iMapsRqpService;
        private readonly ITokenIntegrationServiceClient _iTokenIntegrationService;

        public PeIController(ICdaPiesServiceClient iCDAPiesService, IMapsRqpServiceClient iMapsRqpService, ITokenIntegrationServiceClient iTokenIntegrationService)
        {
            _iCDAPiesService = iCDAPiesService;
            _iMapsRqpService = iMapsRqpService;
            _iTokenIntegrationService = iTokenIntegrationService;
        }

        [HttpGet]
        [Route("/peis")]
        public async Task<IActionResult> GetAsync([FromBody] PeiIntegrationServiceRequestModel mapsPeisRequestBody)
        {

            if (!ValidateMapsPeisServiceHeadersAndBody (mapsPeisRequestBody))
                return BadRequest("Bad Request");
            
            var cdaPeisRequest = CreateCdaPeisServiceRequestModel(mapsPeisRequestBody);

            var result = await CallCdaPiesService(cdaPeisRequest);

            if (result!.Peis != null)
            {
                CreateSuccessResponseHeaders(cdaPeisRequest);
                return Ok(result!.Peis);
            }
            else
            {
                var resultAuthorisationDance = await PerformAuthorisationDance(result!, cdaPeisRequest);
                if (resultAuthorisationDance!.Peis != null)
                {
                    CreateSuccessResponseHeaders(cdaPeisRequest);
                    return Ok(resultAuthorisationDance!.Peis);
                }
            }

            return StatusCode((int)HttpStatusCode.InternalServerError);
            
        }

        #region Private Methods

        private bool ValidateGuid(string guid)
        {
            Guid.TryParse(guid, out var xGuid);

            if (xGuid == Guid.Empty || guid.ToString().Length != GuidLength)
            {
                return false;
            }

            return true;
        }

        private bool ValidateMapsPeisServiceHeadersAndBody (PeiIntegrationServiceRequestModel request)
        {

            Request.Headers.TryGetValue("iss", out var xiss);
            Request.Headers.TryGetValue("userSessionId", out var userSessionId);

            if (string.IsNullOrEmpty(request.RequestId) ||
                string.IsNullOrEmpty(request.PeisId) ||
                string.IsNullOrEmpty(xiss) || 
                string.IsNullOrEmpty(userSessionId))
            {
                return false;
            }

            if (!ValidateGuid(request.RequestId) || !ValidateGuid(request.PeisId) || !ValidateGuid(userSessionId!))
            {
                return false;
            }

            return true;
        }

        private void CreateSuccessResponseHeaders(CdaPiesServiceRequestModel request)
        {
            var headers = Response.Headers;
            headers.Append("rpt", request.Rpt);
        }

        private CdaPiesServiceRequestModel CreateCdaPeisServiceRequestModel(PeiIntegrationServiceRequestModel requestBody)
        {
            Request.Headers.TryGetValue("rpt", out var rpt);

            return new CdaPiesServiceRequestModel
            {
                Rpt = rpt.ToString(),
                RequestId = requestBody.RequestId,
                PeisId = requestBody.PeisId,
            };
        }

        private async Task<CdaPiesServiceResponseModel?> PerformAuthorisationDance(CdaPiesServiceResponseModel cdaPiesServiceResponseModel, CdaPiesServiceRequestModel cdaPeisRequest)
        {
            var mapsRqpServiceRqpResponseModel = CallMapsRqpService().Result;
            var tokenIntegrationResponseModel = CallTokenIntegrationService(cdaPiesServiceResponseModel, mapsRqpServiceRqpResponseModel).Result;

            cdaPeisRequest!.Rpt = tokenIntegrationResponseModel!.Rpt;

            return await CallCdaPiesService(cdaPeisRequest);
        }

        private async Task<CdaPiesServiceResponseModel?> CallCdaPiesService(CdaPiesServiceRequestModel cdaPiesServiceRequestModel)
        {
            return await _iCDAPiesService.GetPiesAsync(cdaPiesServiceRequestModel);
        }

        private async Task<TokenIntegrationResponseModel> CallTokenIntegrationService(CdaPiesServiceResponseModel cdaServiceResponseModel, MapsRqpServiceResponseModel mapsCdaServiceResponseModel)
        {
            var ticketValue = ExtractWWWAuthenticateHeaderValue(cdaServiceResponseModel.ResponseMessage!.WWWAuthenticateResponseHeader!, "ticket=");
            var asUriValue = ExtractWWWAuthenticateHeaderValue(cdaServiceResponseModel.ResponseMessage!.WWWAuthenticateResponseHeader!, "as_uri=");

            var tokenIntegrationServiceRequestModel = new TokenIntegrationServiceRequestModel
            {
                Ticket = ticketValue,
                Rqp = mapsCdaServiceResponseModel!.Rqp,
                As_Uri = asUriValue
            };

            var tokenIntegrationResponseModel = await _iTokenIntegrationService.PostRpt(tokenIntegrationServiceRequestModel);

            return tokenIntegrationResponseModel;
        }

        private async Task<MapsRqpServiceResponseModel> CallMapsRqpService()
        {
            Request.Headers.TryGetValue("iss", out var iss);
            Request.Headers.TryGetValue("userSessionId", out var userSessionId);

            MapsRqpServiceRequestModel mapsRqpServiceRequestModel = new MapsRqpServiceRequestModel
            {
                Iss = iss,
                UserSessionId = userSessionId
            };

            var mapsRqpServiceResponseModel = await _iMapsRqpService.PostRqp(mapsRqpServiceRequestModel);

            return mapsRqpServiceResponseModel!;
        }

        private string ExtractWWWAuthenticateHeaderValue(string wwwAuthenticateHeader, string tokenToExtract)
        {
            var token = wwwAuthenticateHeader.Split(tokenToExtract)[1];
            return token.Split(",")[0].Replace("\"", "");
        }

        #endregion
    }
}
