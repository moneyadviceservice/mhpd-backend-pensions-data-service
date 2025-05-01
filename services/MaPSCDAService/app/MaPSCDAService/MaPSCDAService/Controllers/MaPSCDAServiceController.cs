using MaPSCDAService.Configuration;
using MaPSCDAService.Models;
using MaPSCDAService.Utils;
using MhpdCommon.Extensions;
using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.Models.MHPDModels;
using MhpdCommon.Models.RequestHeaderModel;
using MhpdCommon.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace MaPSCDAService.Controllers;

[Route("/")]
[ApiController]
public class MapsCdaServiceController(IOptions<UriSettings> uriSettings,
    ILogger<MapsCdaServiceController> logger,
    IPkceGenerator pkceGenerator,
    ITokenUtility tokenUtility,
    IIdValidator idValidator) : ControllerBase
{
    [HttpGet("rqp")]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public IActionResult PostRqp([FromHeader] RequestHeaderModel headerModel)
    {
        if (!TryValidateRequest(headerModel, out var message))
        {
            logger.LogWarning("Invalid request: {Message}", message);
            return BadRequest(message);
        }

        using var scope = logger.BeginCorrelationScope(headerModel.CorrelationId!, $"{Constants.LogSource} Rqp");
        logger.LogRequest(headerModel);
        
        var response = new RqpResponseModel { Rqp = GetToken(headerModel.UserSessionId, headerModel.Iss) };
        logger.LogResponse(response);

        return Ok(response);
    }

    [Route("redirect-details")]
    [HttpPost]
    [ProducesResponseType(typeof(RedirectRequestPayload), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public IActionResult RedirectDetails([FromHeader] RequestHeaderModel headerModel)
    {
        if (!TryValidateRequest(headerModel, out var message))
        {
            logger.LogWarning("Invalid request: {Message}", message);
            return BadRequest(message);
        }

        using var scope = logger.BeginCorrelationScope(headerModel.CorrelationId!, $"{ Constants.LogSource} Redirect");
        logger.LogRequest(headerModel);
        
        var response = CreateRedirectResponse(headerModel);
        logger.LogResponse(response);

        return Ok(response);
    }

    private bool TryValidateRequestBase(string? iss, string? userSessionId, string? correlationId, out string? updatedCorrelationId, out string? message)
    {
        if (string.IsNullOrWhiteSpace(iss))
        {
            message = Constants.MissingOrInvalidIss;
            updatedCorrelationId = correlationId;
            return false;
        }

        if (!idValidator.IsValidGuid(userSessionId))
        {
            message = Constants.MissingOrInvalidUserSessionId;
            updatedCorrelationId = correlationId;
            return false;
        }

        if (string.IsNullOrEmpty(correlationId))
        {
            updatedCorrelationId = Guid.NewGuid().ToString();
        }
        else
        {
            updatedCorrelationId = correlationId;
        }

        if (!idValidator.IsValidGuid(updatedCorrelationId))
        {
            message = Constants.InvalidCorrelationId;
            return false;
        }

        message = null;
        return true;
    }

    private bool TryValidateRequest(RequestHeaderModel request, out string? message)
    {
        var isValid = TryValidateRequestBase(request.Iss, request.UserSessionId, request.CorrelationId, out var updatedCorrelationId, out message);
        request.CorrelationId = updatedCorrelationId;
        return isValid;
    }

    private string GetToken(string? userSessionId, string? iss)
    {
        return tokenUtility.GenerateJwt(new CustomClaimDataModel
        {
            Subject = userSessionId + "@" + iss,
            Issuer = iss
        });
    }
    
    private RedirectResponseModel CreateRedirectResponse(RequestHeaderModel request)
    {            
        var (codeVerifier, codeChallenge) = pkceGenerator.GeneratePkce();

        return new RedirectResponseModel
        {
            RedirectTargetUrl = uriSettings.Value.RedirectTargetUrl,
            Rqp = GetToken(request.UserSessionId, request.Iss),
            Scope = Constants.Scope,
            ResponseType = Constants.ResponseType,
            Prompt = Constants.Prompt,
            Service = Constants.Service,
            CodeChallengeMethod = Constants.CodeChallengeMethod,
            CodeChallenge = codeChallenge,
            CodeVerifier = codeVerifier,
            RequestId = Guid.NewGuid().ToString()
        };
    }
}