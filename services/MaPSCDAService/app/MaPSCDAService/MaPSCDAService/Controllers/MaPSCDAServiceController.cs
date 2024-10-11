using MaPSCDAService.Models;
using MaPSCDAService.Utils;
using MhpdCommon.Models.MHPDModels;
using MhpdCommon.Models.MessageBodyModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using MhpdCommon.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.DataProtection;
using System.Globalization;

namespace MaPSCDAService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MaPSCDAServiceController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<MaPSCDAServiceController> _logger;
        private readonly IPkceGenerator _pkceGenerator;
        private readonly string? _redirectTargetUrl;
        private readonly IRqpTokenManager _tokenManager;

        public MaPSCDAServiceController
            (IOptions<UriSettings> uriSettings, 
            ILogger<MaPSCDAServiceController> logger, 
            IPkceGenerator pkceGenerator, 
            IConfiguration configuration,
            IRqpTokenManager tokenManager)
        {
            _configuration = configuration;
            _logger = logger;
            _pkceGenerator = pkceGenerator;
            _redirectTargetUrl = uriSettings.Value.RedirectTargetUrl; 
            _tokenManager = tokenManager;
        }

        [Route("/rqp")]
        [HttpPost]
        public async Task<IActionResult> PostAsync([FromBody] RPQRequestModel rqpquery)
        {
            if (!rqpquery.Validate(rqpquery))
                return BadRequest(Constants.BadRequest);

            if (!GetSecret(out KeyVaultSecrets secrets))
                return StatusCode(StatusCodes.Status500InternalServerError);
            if (string.IsNullOrEmpty(rqpquery.Iss))
                return BadRequest("Issuer is required.");
            if (string.IsNullOrEmpty(rqpquery.UserSessionId))
                return BadRequest("UserSessionID is required.");
            var rqpsToken = _tokenManager.GenerateToken(rqpquery.UserSessionId, rqpquery.Iss);

            return Ok(new RQPResponseModel { Rqp = rqpsToken });
        }

        private bool GetSecret(out KeyVaultSecrets secrets)
        {
            var kId = _configuration["Kid"];
            var aud = _configuration["Audience"];

            secrets = new KeyVaultSecrets { Kid = kId, Audience = aud };

            if (string.IsNullOrEmpty(kId) || string.IsNullOrEmpty(aud))
                return false;

            return true;
        }

        [Route("/redirect_details")]
        [HttpPost]
        public IActionResult RedirectDetails([FromBody] RedirectRequestPayload requestPayload)
        {
            _logger.LogRequest(requestPayload);

            if (requestPayload == null)
            {
                return BadRequest(new { message = Constants.NoRequestBody });
            }

            if (string.IsNullOrEmpty(requestPayload.Iss))
            {
                _logger.LogError(Constants.MissingOrInvalidIss);
                return BadRequest(Constants.MissingOrInvalidIss);
            }

            if (string.IsNullOrEmpty(requestPayload.UserSessionId))
            {
                _logger.LogError(Constants.MissingOrInvalidUserSessionId);
                return BadRequest(Constants.MissingOrInvalidUserSessionId);
            }

            if (string.IsNullOrEmpty(requestPayload.RedirectPurpose))
            {
                _logger.LogError(Constants.MissingOrInvalidRedirectPurpose);
                return BadRequest(Constants.MissingOrInvalidRedirectPurpose);
            }
            
            
            if (!GetSecret(out KeyVaultSecrets secrets))
            {
                _logger.LogError("Failed to retrieve secrets. Kid or Audience is null.");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            var fetchRqp = _tokenManager.GenerateToken(requestPayload.UserSessionId, requestPayload.Iss);

            var response = CreateRedirectResponse(fetchRqp);

            _logger.LogResponse(response);

            return Ok(response);
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
}