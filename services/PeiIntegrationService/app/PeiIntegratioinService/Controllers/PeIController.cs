using MhpdCommon.Constants;
using MhpdCommon.Extensions;
using MhpdCommon.Utils;
using Microsoft.AspNetCore.Mvc;
using PeiIntegrationService.HttpClients.Interfaces;
using PeiIntegrationService.Models;
using PeiIntegrationService.Models.CdaPiesService;
using PeiIntegrationService.Models.MapsCdaService;
using PeiIntegrationService.Models.PeiIntegrationService;
using PeiIntegrationService.Models.TokenIntegrationService;
using System.Net;

namespace PeiIntegrationService.Controllers;

[Route("/")]
[ApiController]
public class PeIController(ICdaPiesServiceClient iCDAPiesService, IMapsRqpServiceClient iMapsRqpService,
    ITokenIntegrationServiceClient iTokenIntegrationService, IIdValidator validator, ILogger<PeIController> logger) : ControllerBase
{
    private readonly ICdaPiesServiceClient _iCDAPiesService = iCDAPiesService;
    private readonly IMapsRqpServiceClient _iMapsRqpService = iMapsRqpService;
    private readonly IIdValidator _iIdValidator = validator;
    private readonly ILogger<PeIController> _logger = logger;
    private readonly ITokenIntegrationServiceClient _iTokenIntegrationService = iTokenIntegrationService;

    [HttpGet]
    [Route("peis")]
    public async Task<IActionResult> GetAsync([FromHeader] PeiIntegrationServiceRequestModel requestModel)
    {
        if (!TryValidateRequestHeaders(requestModel, out var message))
            return BadRequest(message);

        using var scope = _logger.BeginCorrelationScope(requestModel.CorrelationId, Constants.LogSource);

        _logger.LogRequest(requestModel);

        var cdaPeisRequest = new CdaPiesServiceRequestModel
        {
            Rpt = requestModel.Rpt,
            PeisId = requestModel.PeisId,
            RequestId = Guid.NewGuid().ToString(),
        };

        var peidData =  await FetchPeiData(requestModel, cdaPeisRequest);

        _logger.LogResponse(peidData);

        if (peidData == null)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError);
        }

        return Ok(peidData);
    }

    private async Task<IEnumerable<PeiModel>?> FetchPeiData(PeiIntegrationServiceRequestModel requestModel, CdaPiesServiceRequestModel cdaPeisRequest)
    {
        var result = await CallCdaPiesService(cdaPeisRequest);

        if (result!.Peis != null)
        {
            CreateSuccessResponseHeaders(cdaPeisRequest);
            return result!.Peis;
        }
        else
        {
            var resultAuthorisationDance = await PerformAuthorisationDance(result!, cdaPeisRequest, requestModel);
            if (resultAuthorisationDance!.Peis != null)
            {
                CreateSuccessResponseHeaders(cdaPeisRequest);
                return resultAuthorisationDance!.Peis;
            }
        }

        return null;
    }

    #region Private Methods

    private bool TryValidateRequestHeaders (PeiIntegrationServiceRequestModel request, out string? message)
    {
        if (string.IsNullOrWhiteSpace(request.Iss))
        {
            message = Constants.ResponseType.Iss;
            return false;
        }

        if (!_iIdValidator.IsValidGuid(request.PeisId))
        {
            message = Constants.ResponseType.PeisId;
            return false;
        }

        if (!_iIdValidator.IsValidGuid(request.UserSessionId))
        {
            message = Constants.ResponseType.UserSessionId;
            return false;
        }

        if (string.IsNullOrEmpty(request.CorrelationId))
        {
            request.CorrelationId = Guid.NewGuid().ToString();
        }

        if (!_iIdValidator.IsValidGuid(request.CorrelationId))
        {
            message = Constants.ResponseType.CorrelationId;
            return false;
        }

        message = null;
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
        MapsRqpServiceRequestModel mapsRqpServiceRequestModel = new()
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
