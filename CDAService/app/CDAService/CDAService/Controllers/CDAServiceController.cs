using CDAService.Models;
using Microsoft.AspNetCore.Mvc;
using static CDAService.Utils.RSA256TokenUtils;

namespace CDAService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CDAServiceController : ControllerBase
    { 
        [Route("/rqp")]
        [HttpPost]
        public async Task<IActionResult> PostAsync([FromBody] RPQRequestModel rqpquery)
        {
            if (!rqpquery.Validate(rqpquery))
               return BadRequest("Bad Request");

            string rqpsToken = GenerateRqpToken(rqpquery.Iss!, rqpquery.UserSessionId!);
           
            return Ok(new RQPResponseModel { Rqp = rqpsToken });
        }

        private string GenerateRqpToken (string iss,  string userSessionId)
        {
            RQPTokenManager _tokenManager = new RQPTokenManager(iss, userSessionId!);

            return _tokenManager.GenerateToken();
        }
    }
}
