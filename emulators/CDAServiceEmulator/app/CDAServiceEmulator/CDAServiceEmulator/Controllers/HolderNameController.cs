using CDAServiceEmulator.CosmosRepository;
using CDAServiceEmulator.Models;
using CDAServiceEmulator.Models.HolderConfiguration;
using MhpdCommon.Constants;
using MhpdCommon.Models.RequestHeaderModel;
using MhpdCommon.Utils;
using Microsoft.AspNetCore.Mvc;

namespace CDAServiceEmulator.Controllers;

[Route("/")]
[ApiController]
public class HolderNameController(IIdValidator idValidator, IHolderNameViewDataRepository<HolderNameConfigurationModel> viewDataRepository) : ControllerBase
{
    [HttpGet]
    [Route("holdername-view-configurations")]
    public async Task<IActionResult> GetAsync([FromHeader] RequestHeaderModel headerModel, [FromQuery] string? holdername_guid)
    {
        if (!idValidator.IsValidGuid(headerModel.XRequestId))
        {
            return BadRequest(Constants.HolderNameConstants.InvalidRequestId);
        }

        if (holdername_guid == null)
        {
            var allConfigurations = await viewDataRepository.GetHolderNameConfigurationsAsync();
            return Ok(new HolderNameViewDataResponse { Configurations = allConfigurations });
        }

        if (!idValidator.IsValidGuid(holdername_guid))
        {
            return BadRequest(Constants.HolderNameConstants.InvalidHolderNameId);
        }

        var filteredConfigurations = await viewDataRepository.GetByIdAsync(holdername_guid, holdername_guid);

        if (filteredConfigurations == null)
        {
            return NotFound(Constants.HolderNameConstants.UnknownHolderNameId);
        }

        return Ok(new HolderNameViewDataResponse { Configurations = [filteredConfigurations] });
    }
}