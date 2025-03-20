using MhpdCommon.Extensions;
using MhpdCommon.Utils;
using Microsoft.AspNetCore.Mvc;
using PeiIntegrationService.HttpClients.Interfaces;
using PeiIntegrationService.Models;
using PeiIntegrationService.Models.CdaPiesService;
using PeiIntegrationService.Models.PeiIntegrationService;

namespace PeiIntegrationService.Controllers;

[Route("/")]
[ApiController]
public class PeIController(ICdaPiesServiceClient iCDAPiesService, IIdValidator validator, ILogger<PeIController> logger) : ControllerBase
{
    private readonly ICdaPiesServiceClient _peisServiceClient = iCDAPiesService;
    private readonly IIdValidator _iIdValidator = validator;
    private readonly ILogger<PeIController> _logger = logger;

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
            CorrelationId = requestModel.CorrelationId,
        };

        var peidData =  await _peisServiceClient.GetPiesAsync(cdaPeisRequest);

        _logger.LogResponse(peidData);

        return Ok(peidData);
    }

    private bool TryValidateRequestHeaders (PeiIntegrationServiceRequestModel request, out string? message)
    {
        if (string.IsNullOrWhiteSpace(request.Iss))
        {
            message = Constants.ResponseType.Iss;
            return false;
        }

        if (!_iIdValidator.IsValidPeisId(request.PeisId))
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
}
