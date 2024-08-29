using MaPSCDAService.Models;
using MaPSCDAService.Utils;
using Microsoft.AspNetCore.Mvc;

namespace MaPSCDAService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MaPSCDAServiceController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        
        public MaPSCDAServiceController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [Route("/rqp")]
        [HttpPost]
        public async Task<IActionResult> PostAsync([FromBody] RPQRequestModel rqpquery)
        {
            if (!rqpquery.Validate(rqpquery))
               return BadRequest("Bad Request");

            if (!GetSecret(out KeyVaultSecrets secrets))
                return StatusCode(StatusCodes.Status500InternalServerError);

            string rqpsToken = GenerateRqpToken(rqpquery.UserSessionId!, rqpquery.Iss!, secrets);

            return Ok(new RQPResponseModel { Rqp = rqpsToken });
        }

        private string GenerateRqpToken (string userSessionId, string issuer, KeyVaultSecrets secrets)
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
    }
}
