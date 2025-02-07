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
    [Route("rqp")]
    [HttpPost]
    [ProducesResponseType(typeof(RedirectRequestPayload), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public IActionResult PostRqp([FromBody] RedirectRequestPayload request, [FromHeader] RequestHeaderModel headerModel)
    {
        if (!TryValidateRqpRequest(request, headerModel, out var message))
        {
            logger.LogWarning("Invalid request: {Message}", message);
            return BadRequest(message);
        }

        using var scope = logger.BeginCorrelationScope(headerModel.CorrelationId!, $"{Constants.LogSource} Rqp");
        logger.LogRequest(request);
        
        var response = new RqpResponseModel { Rqp = GetToken(request) };
        logger.LogResponse(response);

        return Ok(response);
    }

    [Route("redirect-details")]
    [HttpPost]
    [ProducesResponseType(typeof(RedirectRequestPayload), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public IActionResult RedirectDetails([FromBody] RedirectRequestPayload request, [FromHeader] RequestHeaderModel headerModel)
    {
        if (!TryValidateRqpRequest(request, headerModel, out var message, true))
        {
            logger.LogWarning("Invalid request: {Message}", message);
            return BadRequest(message);
        }

        using var scope = logger.BeginCorrelationScope(headerModel.CorrelationId!, $"{ Constants.LogSource} Redirect");
        logger.LogRequest(request);
        
        var response = CreateRedirectResponse(request);
        logger.LogResponse(response);

        return Ok(response);
    }

    private bool TryValidateRqpRequest(RedirectRequestPayload request, RequestHeaderModel headerModel,
        out string? message, bool redirectPurposeCheck = false)
    {
        if (redirectPurposeCheck && string.IsNullOrWhiteSpace(request.RedirectPurpose))
        {
            message = Constants.MissingOrInvalidRedirectPurpose;
            return false;
        }
        
        if (string.IsNullOrWhiteSpace(request.Iss))
        {
            message = Constants.MissingOrInvalidIss;
            return false;
        }

        if (!idValidator.IsValidGuid(request.UserSessionId))
        {
            message = Constants.MissingOrInvalidUserSessionId;
            return false;
        }

        if (string.IsNullOrEmpty(headerModel.CorrelationId))
        {
            headerModel.CorrelationId = Guid.NewGuid().ToString();
        }

        if (!idValidator.IsValidGuid(headerModel.CorrelationId))
        {
            message = Constants.InvalidCorrelationId;
            return false;
        }

        message = null;
        return true;
    }

    private string GetToken(RedirectRequestPayload request)
    {
        return tokenUtility.GenerateJwt(new CustomClaimDataModel
        {
            Subject = request.UserSessionId + "@" + request.Iss,
            Issuer = request.Iss
        });
    }
    
    private RedirectResponseModel CreateRedirectResponse(RedirectRequestPayload request)
    {            
        var pkce = pkceGenerator.GeneratePkce();
        return new RedirectResponseModel
        {
            RedirectTargetUrl = uriSettings.Value.RedirectTargetUrl,
            Rqp = GetToken(request),
            Scope = Constants.Scope,
            ResponseType = Constants.ResponseType,
            Prompt = Constants.Prompt,
            Service = Constants.Service,
            CodeChallengeMethod = Constants.CodeChallengeMethod,
            CodeChallenge = pkce.codeChallenge,
            CodeVerifier = pkce.codeVerifier,
            RequestId = Guid.NewGuid().ToString()
        };
    }
}