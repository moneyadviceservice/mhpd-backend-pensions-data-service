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

        public MaPSCDAServiceController(IOptions<UriSettings> uriSettings, ILogger<MaPSCDAServiceController> logger, IPkceGenerator pkceGenerator, IConfiguration configuration)
        {
            _configuration = configuration;
            _logger = logger;
            _pkceGenerator = pkceGenerator;
            _redirectTargetUrl = uriSettings.Value.RedirectTargetUrl; 
        }

        [Route("/rqp")]
        [HttpPost]
        public async Task<IActionResult> PostAsync([FromBody] RPQRequestModel rqpquery)
        {
            if (!rqpquery.Validate(rqpquery))
                return BadRequest(Constants.BadRequest);

            if (!GetSecret(out KeyVaultSecrets secrets))
                return StatusCode(StatusCodes.Status500InternalServerError);

            string rqpsToken = GenerateRqpToken(rqpquery.UserSessionId!, rqpquery.Iss!, secrets);

            return Ok(new RQPResponseModel { Rqp = rqpsToken });
        }

        private string GenerateRqpToken(string userSessionId, string issuer, KeyVaultSecrets secrets)
        {
            RSA256TokenUtils.RQPTokenManager _tokenManager = new RSA256TokenUtils.RQPTokenManager(userSessionId, issuer, secrets);
            return _tokenManager.GenerateToken();
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

            var response = CreateRedirectResponse();

            _logger.LogResponse(response);

            return Ok(response);
        }

        private RedirectResponseModel CreateRedirectResponse()
        {
            var (codeChallenge, codeVerifier) = _pkceGenerator.GeneratePkce();
            return new RedirectResponseModel
            {
                // Replaced the constant with the private field storing the environment variable value.
                RedirectTargetUrl = _redirectTargetUrl,
                Rqp = Constants.SampleRqpToken,
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