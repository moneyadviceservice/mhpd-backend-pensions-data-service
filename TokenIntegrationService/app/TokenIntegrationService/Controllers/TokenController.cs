using Microsoft.AspNetCore.Mvc;
using TokenIntegrationService.HttpClients;
using TokenIntegrationService.Models;
using static System.Net.WebRequestMethods;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TokenIntegrationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        private readonly ICDAToken _iCDAToken;
        private CDATokenRequestModel? _cdaTokenRequestModel;

        public TokenController(ICDAToken iCDAToken)
        {
            _iCDAToken = iCDAToken;
        }

        // POST api/<TokenController>
        [HttpPost]
        [Route("/rpt")]
        
        public async Task<IActionResult> PostAsync([FromBody] TokenIntegrationRequestModel requestBody)
        {
            var request = CreateCDATokenRequestModel(requestBody);
            if (ValidateQuery(requestBody, out var message) == false)
                return BadRequest(message);
            var result = await _iCDAToken.PostRpts(request);
            return Ok(result!);
            //return Ok(CreateResponse());
        }       
        private TokenIntegrationResponseModel CreateResponse()
        {
            return new TokenIntegrationResponseModel
            {  
                //rpt = access_token from CDA Token service
                Rpt = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJsb2dnZWRJbkFzIjoiYWRtaW4iLCJpYXQiOjE0MjI3Nzk2Mzh9.gzSraSYS8EXBxLN_oWnFSRgCzcmJmMjLiuyu5CSpyHI"
            };
        }

        private bool ValidateQuery(TokenIntegrationRequestModel requestBody, out string message)
        {
            message = string.Empty;            

            
            if (string.IsNullOrEmpty(requestBody.Ticket))
            {
                message = BadRequestModel.InvalidRequest;
                return false;
            }
            if (string.IsNullOrEmpty(requestBody.Rqp))
            {
                message = BadRequestModel.InvalidRequest;
                return false;
            }
            if (string.IsNullOrEmpty(requestBody.AsUri))
            {
                message = BadRequestModel.InvalidRequest;
                return false;
            }
            return true;
        }      
        private CDATokenRequestModel CreateCDATokenRequestModel(TokenIntegrationRequestModel requestBody)
        {
            
            _cdaTokenRequestModel = new CDATokenRequestModel
            {
                GrantType = "urn:ietf:params:oauth:grant-type:uma-ticket",
                ClaimToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c",
                ClaimTokenFormat = "pension_dashboad_rqp",
                Scope = "owner",
                RequestId = "sdfasdfasdasdadsg",
                Ticket = requestBody.Ticket,
                Rqp = requestBody.Rqp,                
                CdaTokenUrl = requestBody.AsUri                
            };

            return _cdaTokenRequestModel;
        }


    }
}
