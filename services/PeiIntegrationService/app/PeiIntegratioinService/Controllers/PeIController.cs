using MhpdCommon.Constants;
using MhpdCommon.Utils;
using Microsoft.AspNetCore.Mvc;
using PeiIntegrationService.HttpClients.Interfaces;
using PeiIntegrationService.Models.CdaPiesService;
using PeiIntegrationService.Models.MapsCdaService;
using PeiIntegrationService.Models.PeiIntegrationService;
using PeiIntegrationService.Models.TokenIntegrationService;
using System.Net;

namespace PeiIntegrationService.Controllers;

[Route("/")]
[ApiController]
public class PeIController(ICdaPiesServiceClient iCDAPiesService, IMapsRqpServiceClient iMapsRqpService,
    ITokenIntegrationServiceClient iTokenIntegrationService, IIdValidator validator) : ControllerBase
{
    private readonly ICdaPiesServiceClient _iCDAPiesService = iCDAPiesService;
    private readonly IMapsRqpServiceClient _iMapsRqpService = iMapsRqpService;
    private readonly IIdValidator _iIdValidator = validator;
    private readonly ITokenIntegrationServiceClient _iTokenIntegrationService = iTokenIntegrationService;

    [HttpGet]
    [Route("peis")]
    public async Task<IActionResult> GetAsync([FromHeader] PeiIntegrationServiceRequestModel requestModel)
    {
        if (!ValidateRequestHeaders(requestModel))
            return BadRequest("Bad Request");

        var cdaPeisRequest = new CdaPiesServiceRequestModel
        {
            Rpt = requestModel.Rpt,
            PeisId = requestModel.PeisId,
            RequestId = Guid.NewGuid().ToString(),
        };

        var result = await CallCdaPiesService(cdaPeisRequest);

        if (result!.Peis != null)
        {
            CreateSuccessResponseHeaders(cdaPeisRequest);
            return Ok(result!.Peis);
        }
        else
        {
            var resultAuthorisationDance = await PerformAuthorisationDance(result!, cdaPeisRequest, requestModel);
            if (resultAuthorisationDance!.Peis != null)
            {
                CreateSuccessResponseHeaders(cdaPeisRequest);
                return Ok(resultAuthorisationDance!.Peis);
            }
        }

        return StatusCode((int)HttpStatusCode.InternalServerError);
    }

    #region Private Methods

    private bool ValidateRequestHeaders (PeiIntegrationServiceRequestModel request)
    {
        if (string.IsNullOrWhiteSpace(request.Iss)) return false;

        if(!_iIdValidator.IsValidGuid(request.PeisId) || 
            !_iIdValidator.IsValidGuid(request.UserSessionId)) return false;

        return true;
    }

    private void CreateSuccessResponseHeaders(CdaPiesServiceRequestModel request)
    {
        var headers = Response.Headers;
        headers.Append(HeaderConstants.Rpt, request.Rpt);
    }

    private async Task<CdaPiesServiceResponseModel?> PerformAuthorisationDance(CdaPiesServiceResponseModel cdaPiesServiceResponseModel, 
        CdaPiesServiceRequestModel cdaPeisRequest, PeiIntegrationServiceRequestModel requestModel)
    {
        var mapsRqpServiceRqpResponseModel = CallMapsRqpService(requestModel).Result;
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
        var ticketValue = ExtractWWWAuthenticateHeaderValue(cdaServiceResponseModel.ResponseMessage!.WWWAuthenticateResponseHeader!, HeaderConstants.AuthenticateTicket);
        var asUriValue = ExtractWWWAuthenticateHeaderValue(cdaServiceResponseModel.ResponseMessage!.WWWAuthenticateResponseHeader!, HeaderConstants.AuthenticateUri);

        var tokenIntegrationServiceRequestModel = new TokenIntegrationServiceRequestModel
        {
            Ticket = ticketValue,
            Rqp = mapsCdaServiceResponseModel!.Rqp,
            As_Uri = asUriValue
        };

        var tokenIntegrationResponseModel = await _iTokenIntegrationService.PostRpt(tokenIntegrationServiceRequestModel);

        return tokenIntegrationResponseModel;
    }

    private async Task<MapsRqpServiceResponseModel> CallMapsRqpService(PeiIntegrationServiceRequestModel requestModel)
    {
        MapsRqpServiceRequestModel mapsRqpServiceRequestModel = new MapsRqpServiceRequestModel
        {
            Iss = requestModel.Iss,
            UserSessionId = requestModel.UserSessionId
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
