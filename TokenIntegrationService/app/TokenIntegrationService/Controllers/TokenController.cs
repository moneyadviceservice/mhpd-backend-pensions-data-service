using Microsoft.AspNetCore.Mvc;
using TokenIntegrationService.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TokenIntegrationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TokenController : ControllerBase
    {       
        // POST api/<TokenController>
        [HttpPost]
        [Route("/rpt")]
        
        public async Task<IActionResult> PostAsync([FromBody] TokenIntegrationRequestModel requestBody)
        {            
            if (ValidateQuery(requestBody, out var message) == false)
                return BadRequest(message);         
            return Ok(CreateResponse());
        }       
        private TokenIntegrationResponseModel CreateResponse()
        {
            return new TokenIntegrationResponseModel
            {                
                Rpt = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJsb2dnZWRJbkFzIjoiYWRtaW4iLCJpYXQiOjE0MjI3Nzk2Mzh9.gzSraSYS8EXBxLN_oWnFSRgCzcmJmMjLiuyu5CSpyHI"
            };
        }

        private bool ValidateQuery(TokenIntegrationRequestModel requestBody, out string message)
        {
            message = string.Empty; 
            
            if (string.IsNullOrEmpty(requestBody.Rqp))
            {
                message = BadRequestModel.InvalidRequest;
                return false;
            }
            if (string.IsNullOrEmpty(requestBody.Ticket))
            {
                message = BadRequestModel.InvalidRequest;
                return false;
            }
            if (string.IsNullOrEmpty(requestBody.As_Uri))
            {
                message = BadRequestModel.InvalidRequest;
                return false;
            }
            return true;
        }      


    }
}
