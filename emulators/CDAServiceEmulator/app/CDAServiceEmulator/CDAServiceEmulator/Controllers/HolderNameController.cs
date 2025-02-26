using CDAServiceEmulator.CosmosRepository;
using CDAServiceEmulator.Models;
using MhpdCommon.Constants;
using MhpdCommon.Constants.HttpClient;
using MhpdCommon.Models.MHPDModels;
using MhpdCommon.Utils;
using Microsoft.AspNetCore.Mvc;

namespace CDAServiceEmulator.Controllers;

[Route("/")]
[ApiController]
public class HolderNameController(IIdValidator idValidator, IHolderNameViewDataRepository<HolderNameConfigurationModel> viewDataRepository) : ControllerBase
{
    [HttpGet]
    [Route("holdername-view-configurations")]
    public async Task<IActionResult> GetAsync([FromQuery(Name = QueryParams.Cda.HolderName.Guid)] string holderNameGuid,
        [FromHeader(Name = HeaderConstants.RequestId)] string? xRequestId)
    {
        if (!idValidator.IsValidGuid(xRequestId))
        {
            return BadRequest(Constants.HolderNameConstants.InvalidRequestId);
        }
        
        if (!idValidator.IsValidGuid(holderNameGuid))
        {
            return BadRequest(Constants.HolderNameConstants.InvalidHolderNameId);
        }

        var filteredConfigurations = await viewDataRepository.GetByIdStreamAsync(holderNameGuid, holderNameGuid);

        if (filteredConfigurations == null)
        {
            return NotFound(Constants.HolderNameConstants.UnknownHolderNameId);
        }

        return Ok(new HolderNameViewDataResponse { Configurations = [filteredConfigurations] });
    }
}