using System.Net.Http.Headers;
using CDAServiceEmulator.Configuration;
using CDAServiceEmulator.CosmosRepository;
using CDAServiceEmulator.Models.Peis;
using MhpdCommon.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace CDAServiceEmulator.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PeisController : ControllerBase
{
    private readonly CdaPeisEmulatorScenarioModelRepository _cdaPeisEmulatorScenarioModelRepository;
    private readonly CdaPeisEmulatorTestInstanceDataRepository _cdaPeisEmulatorTestInstanceDataRepository;
    private readonly MhpdCosmosConfiguration _cosmosConfigOptions;
    private const string BadRequestResponse = "Bad Request";
    private readonly IIdValidator _idValidator;

    public PeisController(CdaPeisEmulatorScenarioModelRepository cdaPeisEmulatorScenarioModelRepository,
        CdaPeisEmulatorTestInstanceDataRepository cdaPeisEmulatorTestInstanceDataRepository,
        IOptions<MhpdCosmosConfiguration> cosmosConfigOptions, IIdValidator idValidator)
    {
        _cdaPeisEmulatorScenarioModelRepository = cdaPeisEmulatorScenarioModelRepository;
        _cdaPeisEmulatorTestInstanceDataRepository = cdaPeisEmulatorTestInstanceDataRepository;
        _cosmosConfigOptions = cosmosConfigOptions.Value;
        _idValidator = idValidator;
    }

    [HttpGet]
    [Route("/peis/{peis_id}")]
    public async Task<IActionResult> GetAsync([FromRoute] string peis_id, [FromHeader] RequestHeaderModel requestHeader)
    {
        if (!ValidateAuthHeader())
        {
            return Unauthorized("Unauthorized");
        }
        
        if (string.IsNullOrEmpty(requestHeader.XRequestId) || !_idValidator.IsValidGuid(requestHeader.XRequestId))
        {
            return BadRequest(BadRequestResponse);
        }
        
        if (!_idValidator.IsValidGuid(peis_id))
        {
            return BadRequest(BadRequestResponse);
        }
        
        // Extract first 4 characters from the Peis_id
        var peisStartCode = GetPeisStartCode(peis_id);
        
        var scenarioModelData = await _cdaPeisEmulatorScenarioModelRepository.GetByIdAsync(peisStartCode, _cosmosConfigOptions.CdaPeisEmulatorScenarioModelContainerPartitionKey);

        if (scenarioModelData?.DataPoints == null)
        {
            return BadRequest(BadRequestResponse);
        }

        var result = scenarioModelData.DataPoints?[0].ResponsePayload;
        
        // Check if the record exists in the cdaPeisEmulatorTestInstanceData container
        var testInstanceData = await _cdaPeisEmulatorTestInstanceDataRepository.GetByIdAsync(peisStartCode, _cosmosConfigOptions.CdaPeisEmulatorTestInstanceDataContainerPartitionKey);
        if (testInstanceData == null)
        {
            await _cdaPeisEmulatorTestInstanceDataRepository.InsertItemAsync(new CdaPeisEmulatorTestInstanceDataModel
            {
                Id = peis_id,
                PeisId = peis_id,
                InitialCallTimestamp = DateTimeOffset.UtcNow
            }, _cosmosConfigOptions.CdaPeisEmulatorTestInstanceDataContainerPartitionKey);
        }
        else
        {
            // Get the time since initial call
            var timeSince = (DateTimeOffset.UtcNow - testInstanceData.InitialCallTimestamp).Seconds;

            if (scenarioModelData.DataPoints != null)
            {
                var allAvailableAt = scenarioModelData.DataPoints.Select(s => s.AvailableAt).ToList();
                var closestAvailableAt = FindClosestAvailableTime(allAvailableAt, timeSince);
            
                // Find the closest response payload
                result = FindResponsePayload(scenarioModelData, closestAvailableAt);
            }

            if (result == null)
            {
                return BadRequest(BadRequestResponse);
            }
        }
        
        return Ok(result);
    }
    
    public static int FindClosestAvailableTime(List<int> availableTimes, int timeSince)
    {
        return availableTimes.Where(n => n <= timeSince).MaxBy(n => n);
    }
    
    public static ResponsePayload? FindResponsePayload(CdaPeisEmulatorScenarioModel scenarioModelData, int closestAvailableTime)
    {
        return scenarioModelData.DataPoints?.Find(s => s.AvailableAt == closestAvailableTime)?.ResponsePayload;
    }

    private static string GetPeisStartCode(string peisId)
    {
        return peisId.Substring(0, 4);
    }

    private bool ValidateAuthHeader()
    {
        var accessToken = Request.Headers[HeaderNames.Authorization];
        var parameter = string.Empty;

        if (AuthenticationHeaderValue.TryParse(accessToken, out var headerValue))
        {
            var scheme = headerValue.Scheme;
            if (scheme != "Bearer")
            {
                return false;
            }

            parameter = headerValue.Parameter;
        }

        if (string.IsNullOrEmpty(parameter))
        {
            var headers = Response.Headers;
            headers.Append("WWW-Authenticate", "realm=\"PensionDashboard\", " +
                                               "as_uri=\"https://as.pdp.com\", " +
                                               "ticket=\"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.cThIIoDvwdueQB468K5xDc5633seEFoqwxjF_xSJyQQ\"");
            return false;
        }

        return true;
    }
}