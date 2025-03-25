using MhpdCommon.Extensions;
using MhpdCommon.Utils;
using Microsoft.AspNetCore.Mvc;
using PeiIntegrationService.HttpClients.Interfaces;
using PeiIntegrationService.Models;
using PeiIntegrationService.Models.CdaPeisServiceClient;
using PeiIntegrationService.Models.PeiIntegrationServiceClient;

namespace PeiIntegrationService.Controllers;

[Route("/")]
[ApiController]
public class PeIController(ICdaPiesServiceClient iCdaPiesService, IIdValidator validator, ILogger<PeIController> logger) : ControllerBase
{
    [HttpGet]
    [Route("peis")]
    public async Task<IActionResult> GetAsync([FromHeader] PeiIntegrationServiceRequestModel requestModel)
    {
        if (!TryValidateRequestHeaders(requestModel, out var message))
            return BadRequest(message);

        using var scope = logger.BeginCorrelationScope(requestModel.CorrelationId, Constants.LogSource);

        logger.LogRequest(requestModel);

        var cdaPeisRequest = new CdaPiesServiceRequestModel
        {
            Rpt = requestModel.Rpt,
            PeisId = requestModel.PeisId,
            CorrelationId = requestModel.CorrelationId,
        };

        var peisData =  await iCdaPiesService.GetPiesAsync(cdaPeisRequest);

        logger.LogResponse(peisData);

        return Ok(peisData);
    }

    private bool TryValidateRequestHeaders (PeiIntegrationServiceRequestModel request, out string? message)
    {
        if (string.IsNullOrWhiteSpace(request.Iss))
        {
            message = Constants.ResponseType.Iss;
            return false;
        }

        if (!validator.IsValidPeisId(request.PeisId))
        {
            message = Constants.ResponseType.PeisId;
            return false;
        }

        if (!validator.IsValidGuid(request.UserSessionId))
        {
            message = Constants.ResponseType.UserSessionId;
            return false;
        }

        if (string.IsNullOrEmpty(request.CorrelationId))
        {
            request.CorrelationId = Guid.NewGuid().ToString();
        }

        if (!validator.IsValidGuid(request.CorrelationId))
        {
            message = Constants.ResponseType.CorrelationId;
            return false;
        }

        message = null;
        return true;
    }
}
