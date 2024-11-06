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
    IConfiguration configuration,
    IRqpTokenManager tokenManager,
    IIdValidator idValidator) : ControllerBase
{
    private readonly IConfiguration _configuration = configuration;
    private readonly ILogger<MapsCdaServiceController> _logger = logger;
    private readonly IPkceGenerator _pkceGenerator = pkceGenerator;
    private readonly string? _redirectTargetUrl = uriSettings.Value.RedirectTargetUrl;
    private readonly IRqpTokenManager _tokenManager = tokenManager;
    private readonly IIdValidator _idValidator = idValidator;

    [Route("rqp")]
    [HttpPost]
    public IActionResult PostRqp([FromBody] RPQRequestModel rqpquery, [FromHeader] RequestHeaderModel headerModel)
    {
        if (!TryValidateRqpRequest(rqpquery, headerModel, out var message))
        {
            return BadRequest(message);
        }

        using var scope = _logger.BeginCorrelationScope(headerModel.CorrelationId!, $"{Constants.LogSource} Rqp");
        _logger.LogRequest(rqpquery);

        if (!GetSecret(out _))
        {
            _logger.LogCritical(Constants.NoAccessToVault);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        var rqpsToken = _tokenManager.GenerateToken(rqpquery.UserSessionId!, rqpquery.Iss!);

        var response = new RQPResponseModel { Rqp = rqpsToken };
        _logger.LogResponse(response);

        return Ok(response);
    }

    [Route("redirect_details")]
    [HttpPost]
    public IActionResult RedirectDetails([FromBody] RedirectRequestPayload requestPayload, [FromHeader] RequestHeaderModel headerModel)
    {
        if (!TryValidateRedirectRequest(requestPayload, headerModel, out var message))
        {
            return BadRequest(message);
        }

        using var scope = _logger.BeginCorrelationScope(headerModel.CorrelationId!, $"{ Constants.LogSource} Redirect");
        _logger.LogRequest(requestPayload);

        if (!GetSecret(out _))
        {
            _logger.LogCritical(Constants.NoAccessToVault);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        var fetchRqp = _tokenManager.GenerateToken(requestPayload.UserSessionId!, requestPayload.Iss!);

        var response = CreateRedirectResponse(fetchRqp);

        _logger.LogResponse(response);

        return Ok(response);
    }

    private bool TryValidateRqpRequest(RPQRequestModel request, RequestHeaderModel headerModel, out string? message)
    {
        if (string.IsNullOrWhiteSpace(request.Iss))
        {
            message = Constants.MissingOrInvalidIss;
            return false;
        }

        if (!_idValidator.IsValidGuid(request.UserSessionId))
        {
            message = Constants.MissingOrInvalidUserSessionId;
            return false;
        }

        if (string.IsNullOrEmpty(headerModel.CorrelationId))
        {
            headerModel.CorrelationId = Guid.NewGuid().ToString();
        }

        if (!_idValidator.IsValidGuid(headerModel.CorrelationId))
        {
            message = Constants.InvalidCorrelationId;
            return false;
        }

        message = null;
        return true;
    }

    private bool TryValidateRedirectRequest(RedirectRequestPayload request, RequestHeaderModel headerModel, out string? message)
    {
        if (string.IsNullOrWhiteSpace(request.Iss))
        {
            message = Constants.MissingOrInvalidIss;
            return false;
        }

        if (string.IsNullOrWhiteSpace(request.RedirectPurpose))
        {
            message = Constants.MissingOrInvalidRedirectPurpose;
            return false;
        }

        if (!_idValidator.IsValidGuid(request.UserSessionId))
        {
            message = Constants.MissingOrInvalidUserSessionId;
            return false;
        }

        if (string.IsNullOrEmpty(headerModel.CorrelationId))
        {
            headerModel.CorrelationId = Guid.NewGuid().ToString();
        }

        if (!_idValidator.IsValidGuid(headerModel.CorrelationId))
        {
            message = Constants.InvalidCorrelationId;
            return false;
        }

        message = null;
        return true;
    }

    private bool GetSecret(out KeyVaultSecrets secrets)
    {
        var kId = _configuration[Constants.Kid];
        var aud = _configuration[Constants.Audience];

        secrets = new KeyVaultSecrets { Kid = kId, Audience = aud };

        if (string.IsNullOrEmpty(kId) || string.IsNullOrEmpty(aud))
            return false;

        return true;
    }

    private RedirectResponseModel CreateRedirectResponse(string fetchRqp)
    {            
        var (codeChallenge, codeVerifier) = _pkceGenerator.GeneratePkce();
        return new RedirectResponseModel
        {                
            RedirectTargetUrl = _redirectTargetUrl,
            Rqp = fetchRqp,
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