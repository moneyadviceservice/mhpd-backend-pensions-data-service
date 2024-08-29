using CDAServiceEmulator.Mocks;
using Microsoft.AspNetCore.Mvc;

namespace CDAServiceEmulator.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HolderNameController : ControllerBase
    {
        private const int PeiLength = 73;

        public HolderNameController()
        {
        }

        [HttpGet]
        [Route("/holdername-configurations")]
        public async Task<IActionResult> GetAsync([FromQuery] string? pei)
        {
            if (Validate(pei!) == false)
            {
                return BadRequest("Bad Request");
            }

            return Ok(await HolderConfigurationMock.GetHolderConfiguration());
        }

        private bool Validate(string pei)
        {
            Request.Headers.TryGetValue("X-Request-ID", out var xRequestId);

            if (string.IsNullOrEmpty(xRequestId))
                return false;

            if (!(string.IsNullOrEmpty(pei)))
            {
                if (pei.Length != PeiLength)
                    return false;
            }

            return true;
        }
    }
}
