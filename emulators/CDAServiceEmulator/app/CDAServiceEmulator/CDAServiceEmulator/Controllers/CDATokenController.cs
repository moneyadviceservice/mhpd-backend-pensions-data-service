using CDAServiceEmulator.CosmosRepository;
using CDAServiceEmulator.Models;
using CDAServiceEmulator.Models.Token;
using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.Models.RequestHeaderModel;
using MhpdCommon.TokenValidation;
using MhpdCommon.Utils;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace CDAServiceEmulator.Controllers;

[Route("/")]   
[ApiController]
public class CdaTokenController(
    ILogger<CdaTokenController> logger,
    IIdValidator idValidator,
    TokenRequestValidatorPipeline tokenRequestValidators, 
    TokenEmulatorPiesIdScenarioModelsRepository tokenEmulatorPiesIdScenarioModelRepository,
    ITokenUtility tokenUtility)
    : ControllerBase
{
    [HttpPost]
    [Route("token")]
    public async Task<IActionResult> GenerateTokenAsync([FromQuery] CdaTokenRequestModel request, [FromHeader] RequestHeaderModel requestHeader)
    {
        LogInfoWithJsonObject("Request received: ", request);
            
        if (string.IsNullOrEmpty(requestHeader.XRequestId) || !idValidator.IsValidGuid(requestHeader.XRequestId))
        {
            return await Task.FromResult<IActionResult>(BadRequest(TokenValidationMessages.InvalidXRequestId));
        }
        
        var validationResult = tokenRequestValidators.Validate(request);
    
        if (!validationResult.IsValid)
        {
            LogError(validationResult.ErrorMessage);
            return await Task.FromResult<IActionResult>(BadRequest(validationResult.ErrorMessage));
        }

        CdaTokenResponseModel response;
        
        // Validate code when grant_type is authorization_code
        if (request is { Code: not null, GrantType: TokenQueryParams.AuthorizationCodeGrantType })
        {
            var data = await tokenEmulatorPiesIdScenarioModelRepository.GetByIdAsync(request.Code, request.Code);
            if (data != null && !string.IsNullOrEmpty(data.PeisIdStartCode))
            {
               response = CreateResponse(true, data.PeisIdStartCode);
            }
            else
            {
                return BadRequest(TokenValidationMessages.UnknownAuthorizationCode);
            }
        }
        else
        {
            response = CreateResponse();
        }

        LogInfoWithJsonObject("Response: ", response);
        
        return await Task.FromResult<IActionResult>(Ok(response));
    }

    private CdaTokenResponseModel CreateResponse(bool isAuthorizationCodeGrantType = false, string peisStartCode = "")
    {
        return new CdaTokenResponseModel
        {
            AccessToken = TokenQueryParams.ValidJwtToken,
            TokenType = isAuthorizationCodeGrantType ? TokenQueryParams.TokenTypeBearer : TokenQueryParams.TokenTypeRpt,
            Upgraded = false,
            IdToken = isAuthorizationCodeGrantType ? GetIdToken(peisStartCode) : null,
            Pct = !isAuthorizationCodeGrantType ? TokenQueryParams.ValidJwtToken : null
        };
    }

    private string? GetIdToken(string peisStartCode)
    {
        if (peisStartCode == Constants.TokenConstants.NullIdTokenCode) return null;
        if (peisStartCode == Constants.TokenConstants.InvalidIdTokenCode) return "ThisStringIsNotAValidJwtToken";
        if (peisStartCode == Constants.TokenConstants.MissingPeisTokenCode) return tokenUtility.GenerateJwt(null); //return a Jwt with no PeisId claim

        return tokenUtility.GenerateJwt(peisStartCode);
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