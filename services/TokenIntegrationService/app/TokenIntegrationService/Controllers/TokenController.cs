using Microsoft.AspNetCore.Mvc;
using TokenIntegrationService.HttpClients;
using TokenIntegrationService.Models;

namespace TokenIntegrationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        private readonly ICDATokenService _iCDATokenService;

        private const string GrantType = "urn:ietf:params:oauth:grant-type:uma-ticket";
        private const string ClaimTokenFormat = "pension_dashboad_rqp";
        private const string Scope = "owner";
        private const string RequestId = "sdfasdfasdasdadsg";

        public TokenController(ICDATokenService iCDATokenService)
        {
            _iCDATokenService = iCDATokenService;
        }

        [HttpPost]
        [Route("/rpts")]
        public async Task<IActionResult> PostAsync([FromBody] TokenIntegrationRequestModel requestBody)
        {            
            if (!requestBody.Validate())
                return BadRequest("Bad Request");

            var request = CreateCDATokenServiceRequestModel(requestBody);
            
            var result = await _iCDATokenService.PostRpt(request);           
            
            return Ok(new TokenIntegrationResponseModel {  Rpt = result.AccessToken });             
        }

        private CDATokenRequestModel CreateCDATokenServiceRequestModel(TokenIntegrationRequestModel requestBody)
        {
            return new CDATokenRequestModel
            {
                GrantType = GrantType,
                ClaimToken = requestBody.Rqp,
                ClaimTokenFormat = ClaimTokenFormat,
                Scope = Scope,
                RequestId = RequestId,
                Ticket = requestBody.Ticket,
                Rqp = requestBody.Rqp,
                CdaTokenUrl = requestBody.As_Uri
            };
        }
    }
}
