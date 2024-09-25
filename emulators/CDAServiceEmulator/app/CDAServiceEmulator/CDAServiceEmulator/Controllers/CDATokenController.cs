using CDAServiceEmulator.Models.Peis;
using CDAServiceEmulator.Models.Token;
using CDAServiceEmulator.TokenValidation;
using MhpdCommon.Utils;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace CDAServiceEmulator.Controllers;

[Route("api/[controller]")]   
[ApiController]
public class CdaTokenController(
    ILogger<CdaTokenController> logger,
    IIdValidator idValidator,
    TokenRequestValidatorPipeline tokenRequestValidators)
    : ControllerBase
{
    [Route("token")]     
    [HttpPost]      

    public Task<IActionResult> GenerateTokenAsync([FromQuery] CdaTokenRequestModel request, [FromHeader]RequestHeaderModel requestHeader)
    {
        LogInfoWithJsonObject("Request received: ", request);
            
        if (string.IsNullOrEmpty(requestHeader.XRequestId) || !idValidator.IsValidGuid(requestHeader.XRequestId))
        {
            return Task.FromResult<IActionResult>(BadRequest(TokenValidationMessages.InvalidXRequestId));
        }
            
        var validationResult = tokenRequestValidators.Validate(request);
    
        if (!validationResult.IsValid)
        {
            LogError(validationResult.ErrorMessage);
            return Task.FromResult<IActionResult>(BadRequest(validationResult.ErrorMessage));
        }

        return Task.FromResult<IActionResult>(Ok(CreateResponse()));
    }

    private static CdaTokenResponseModel CreateResponse()
    {
        return new CdaTokenResponseModel
        {
            AccessToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJsb2dnZWRJbkFzIjoiYWRtaW4iLCJpYXQiOjE0MjI3Nzk2Mzh9.gzSraSYS8EXBxLN_oWnFSRgCzcmJmMjLiuyu5CSpyHI",
            TokenType = "pension_dashboard_rpt",
            Upgraded = false,
            Pct = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJsb2dnZWRJbkFzIjoiYWRtaW4iLCJpYXQiOjE0MjI3Nzk2Mzh9.gzSraSYS8EXBxLN_oWnFSRgCzcmJmMjLiuyu5CSpyHI"
        };
    }

    private void LogInfoWithJsonObject<T>(string type, T data)
    {
        logger.LogInformation("{Type} {Data}", type, JsonConvert.SerializeObject(data));
    }

    private void LogError(string errorMessage)
    {
        logger.LogError("Error: {ErrorMessage}", errorMessage);
    }
}