using CDATokenServices.Models;
using Microsoft.AspNetCore.Mvc;

namespace CDAServiceEmulator.Controllers
{
    [Route("api/[controller]")]   
    [ApiController]
    public class CDATokenController : ControllerBase
    {
        [Route("/token")]     
        [HttpPost]      

        public async Task<IActionResult> PostAsync([FromQuery] CDATokenRequestModel query)            
        {
           
            if (ValidateQuery(query, out var message) == false)
                return BadRequest(message);

            if (!ValidateHeaders())
            {
                return Unauthorized("Unauthorized");
            }

            return Ok(CreateResponse());

        }

        private CDATokenResponseModel CreateResponse ()
        {
            return new CDATokenResponseModel
            {
                AccessToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJsb2dnZWRJbkFzIjoiYWRtaW4iLCJpYXQiOjE0MjI3Nzk2Mzh9.gzSraSYS8EXBxLN_oWnFSRgCzcmJmMjLiuyu5CSpyHI",
                TokenType = "pension_dashboard_rpt",
                Upgraded = false,
                Pct = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJsb2dnZWRJbkFzIjoiYWRtaW4iLCJpYXQiOjE0MjI3Nzk2Mzh9.gzSraSYS8EXBxLN_oWnFSRgCzcmJmMjLiuyu5CSpyHI"
            };
        }

        private bool ValidateQuery(CDATokenRequestModel query, out string message)
        {
            message = string.Empty;

            if (!GrantTypeEnum.Validate(query.GrantType!, out var badMessage))
            {
                message = badMessage;
                return false;
            }

            if (!ScopeEnum.Validate(query.Scope!, out badMessage))
            {
                message = badMessage;
                return false;
            }

            if (!ClaimTokenFormatEnum.Validate(query.ClaimTokenFormat!, out badMessage))
            {
                message = badMessage;
                return false;
            }
            if (string.IsNullOrEmpty(query.Ticket))
            {
                message = BadRequestModel.InvalidRequest;
                return false;
            }          
            if (string.IsNullOrEmpty(query.ClaimToken))
            {
                message = BadRequestModel.InvalidRequest;
                return false;
            }

            return true;
        }

        private bool ValidateHeaders()
        {
            string headerValue = Request.Headers["X-Request-ID"];
            if (string.IsNullOrEmpty(headerValue))
            {
                return false;
            }
            return true;
        }      
      

    }
}
