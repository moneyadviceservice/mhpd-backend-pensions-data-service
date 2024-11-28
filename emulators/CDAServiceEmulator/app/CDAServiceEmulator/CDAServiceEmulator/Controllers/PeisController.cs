using System.Net.Http.Headers;
using CDAServiceEmulator.CosmosRepository;
using CDAServiceEmulator.Models.Peis;
using MhpdCommon.Constants;
using MhpdCommon.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace CDAServiceEmulator.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PeisController(
    CdaPeisEmulatorScenarioModelRepository cdaPeisEmulatorScenarioModelRepository,
    CdaPeisEmulatorTestInstanceDataRepository cdaPeisEmulatorTestInstanceDataRepository,
    IIdValidator idValidator)
    : ControllerBase
{
    private const string BadRequestPeisIdInvalidResponse = "Invalid peis_id";
    private const string BadRequestXRequestIdInvalidResponse = "Invalid X-Request-Id";
    private const string BadRequestUnknownTestScenarioResponse = "Unknown test scenario";
    private const string UnauthorisedResponse = "Unauthorized";

    [HttpGet]
    [Route("/peis/{peis_id}")]
    public async Task<IActionResult> GetAsync([FromRoute] string peis_id,
        [FromHeader(Name = HeaderConstants.RequestId)] string? xRequestId)
    {
        if (!ValidateAuthHeader())
        {
            return Unauthorized(UnauthorisedResponse);
        }
        
        if (string.IsNullOrEmpty(xRequestId) || !idValidator.IsValidGuid(xRequestId))
        {
            return BadRequest(BadRequestXRequestIdInvalidResponse);
        }
        
        if (string.IsNullOrEmpty(peis_id) || !idValidator.IsValidGuid(peis_id))
        {
            return BadRequest(BadRequestPeisIdInvalidResponse);
        }
        
        // Extract first 4 characters from the Peis_id
        var peisStartCode = GetPeisStartCode(peis_id);
        
        var scenarioModelData = await cdaPeisEmulatorScenarioModelRepository.GetByIdAsync(peisStartCode, peisStartCode);

        if (scenarioModelData?.DataPoints == null)
        {
            return BadRequest(BadRequestUnknownTestScenarioResponse);
        }

        var result = scenarioModelData.DataPoints?[0].ResponsePayload;
        
        // Check if the record exists in the cdaPeisEmulatorTestInstanceData container
        var testInstanceData = await cdaPeisEmulatorTestInstanceDataRepository.GetByIdAsync(peisStartCode, peis_id);
        if (testInstanceData == null)
        {
            await cdaPeisEmulatorTestInstanceDataRepository.InsertItemAsync(new CdaPeisEmulatorTestInstanceDataModel
            {
                Id = peisStartCode,
                PeisId = peis_id,
                InitialCallTimestamp = DateTimeOffset.UtcNow
            }, peis_id);
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
                return BadRequest();
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