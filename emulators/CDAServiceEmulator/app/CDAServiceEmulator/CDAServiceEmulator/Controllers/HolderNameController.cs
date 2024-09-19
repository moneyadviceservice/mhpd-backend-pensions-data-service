using CDAServiceEmulator.Mocks;
using MhpdCommon.Utils;
using Microsoft.AspNetCore.Mvc;

namespace CDAServiceEmulator.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HolderNameController : ControllerBase
    {
        private readonly IIdValidator _idValidator;
        public HolderNameController(IIdValidator idValidator)
        {
            _idValidator = idValidator;
        }

        [HttpGet]
        [Route("/holdername-view-configurations")]
        public async Task<IActionResult> GetAsync([FromHeader(Name = "X-Request-ID")] string? requestId, [FromQuery] string? holdername_guid)
        {
            // Validate the input
            if (!_idValidator.IsValidGuid(requestId))
            {
                return BadRequest("Invalid X-Request-ID header");
            }

            // Scenario 1: No holdername_guid provided
            if (holdername_guid == null)
            {
                var allConfigurations = await HolderConfigurationMock.GetHolderConfiguration();
                return Ok(new { holder_view_configurations = allConfigurations });
            }

            // Scenario 3: Invalid holdername_guid format
            if (!_idValidator.IsValidGuid(holdername_guid))
            {
                return BadRequest("Invalid holdername_guid format");
            }

            // Filter based on holdername_guid
            var filteredConfigurations = HolderConfigurationMock.FilterConfigurations(holdername_guid);

            // Scenario 4: Unknown holdername_guid
            if (filteredConfigurations.Count == 0)
            {
                return NotFound("Unknown holdername_guid");
            }

            // Scenario 2: Known holdername_guid
            return Ok(new { holder_view_configurations = filteredConfigurations });
        }
    }
}
