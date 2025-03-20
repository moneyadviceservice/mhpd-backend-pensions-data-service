using CDAServiceEmulator.CosmosRepository;
using CDAServiceEmulator.Models;
using MhpdCommon.Constants;
using MhpdCommon.Constants.HttpClient;
using MhpdCommon.Models.Configuration;
using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.Models.MHPDModels;
using MhpdCommon.Models.MHPDModels.JwkUri;
using MhpdCommon.TokenValidation;
using MhpdCommon.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;
using CdaTokenResponseModel = CDAServiceEmulator.Models.Token.CdaTokenResponseModel;

namespace CDAServiceEmulator.Controllers;

[Route("/")]   
[ApiController]
public class CdaTokenController(
    ILogger<CdaTokenController> logger,
    IIdValidator idValidator,
    TokenRequestValidatorPipeline tokenRequestValidators, 
    TokenEmulatorPiesIdScenarioModelsRepository tokenEmulatorPiesIdScenarioModelRepository,
    ITokenUtility tokenUtility,
    IOptions<CommonHttpConfiguration> options)
    : ControllerBase
{
    private readonly CommonHttpConfiguration httpConfiguration = options.Value;

    [HttpPost]
    [Route("token")]
    public async Task<IActionResult> GenerateTokenAsync([FromForm] CdaTokenRequestModel request,
        [FromHeader(Name = HeaderConstants.RequestId)] string? xRequestId)
    {
        LogInfoWithJsonObject("Request received: ", request);
        
        if (string.IsNullOrEmpty(xRequestId) || !idValidator.IsValidGuid(xRequestId))
        {
            return await Task.FromResult<IActionResult>(BadRequest(TokenValidationMessages.InvalidXRequestId));
        }
        
        var validationResult = tokenRequestValidators.Validate(request);
    
        if (!validationResult.IsValid)
        {
            LogError(validationResult.ErrorMessage);
            return await Task.FromResult<IActionResult>(BadRequest(validationResult.ErrorMessage));
        }

        // Validate code when grant_type is authorization_code
        if (request is { Code: not null, GrantType: TokenQueryParams.AuthorizationCodeGrantType })
        {
            return await ProcessIdToken(request.Code);
        }
        else
        {
            return ProcessAccessToken(request.Ticket);
        }
    }

    private async Task<IActionResult> ProcessIdToken(string code)
    {
        var data = await tokenEmulatorPiesIdScenarioModelRepository.GetByIdAsync(code, code);
        if (data != null && !string.IsNullOrEmpty(data.PeisIdStartCode))
        {
            var response = CreateResponse(true, data.PeisIdStartCode);

            LogInfoWithJsonObject("Response: ", response);
            return Ok(response);
        }
        else
        {
            LogError(TokenValidationMessages.UnknownAuthorizationCode);
            return BadRequest(TokenValidationMessages.UnknownAuthorizationCode);
        }
    }

    private IActionResult ProcessAccessToken(string? ticket)
    {
        if (string.Equals(ticket, SecurityConstants.Jwe.AuthorizedPermissionTicket))
        {
            var response = CreateResponse();

            LogInfoWithJsonObject("Response: ", response);
            return Ok(response);
        }

        if (string.Equals(ticket, SecurityConstants.Jwe.AuthorizationRequiredPermissionTicket))
        {
            var redirectResponse = new ClaimsGatheringResponseModel
            {
                Ticket = SecurityConstants.Jwe.ClaimsRequiredPermissionTicket,
                ErrorDescription = TokenValidationMessages.AdditionalClaimsMessage,
                Error = TokenValidationMessages.AdditionalClaimsReason,
                RedirectUser = UrlHelper.ConstructPath(httpConfiguration.CdaServiceUrl, HttpEndpoints.External.ClaimsGathering)
            };

            LogError(TokenValidationMessages.AdditionalClaimsMessage);
            return StatusCode(403, redirectResponse);
        }

        return BadRequest(TokenValidationMessages.InvalidTicketQuery);
    }

    private CdaTokenResponseModel CreateResponse(bool isAuthorizationCodeGrantType = false, string peisStartCode = "")
    {
        return new CdaTokenResponseModel
        {
            AccessToken = isAuthorizationCodeGrantType ? string.Empty : tokenUtility.GenerateJwt(new CustomClaimDataModel()),
            TokenType = isAuthorizationCodeGrantType ? TokenQueryParams.TokenTypeBearer : TokenQueryParams.TokenTypeRpt,
            Upgraded = false,
            IdToken = isAuthorizationCodeGrantType ? GetIdToken(peisStartCode) : null,
            Pct = !isAuthorizationCodeGrantType ? TokenQueryParams.ValidJwtToken : null
        };
    }

    private string? GetIdToken(string peisStartCode)
    {
        return peisStartCode switch
        {
            Constants.TokenConstants.NullIdTokenCode => null,
            Constants.TokenConstants.InvalidIdTokenCode => "ThisStringIsNotAValidJwtToken",
            Constants.TokenConstants.MissingPeisTokenCode => tokenUtility.GenerateJwt(new CustomClaimDataModel()),
            Constants.TokenConstants.NoKidTokenCode => JwkConstants.TestTokenNoKid,
            Constants.TokenConstants.UnknownKidTokenCode => JwkConstants.TestTokenUnknownKid,
            Constants.TokenConstants.ExpiredTokenCode => JwkConstants.TestTokenExpired,
            Constants.TokenConstants.UnknownKeyTokenCode => JwkConstants.TestTokenUnknownKey,
            _ => tokenUtility.GenerateJwt(new CustomClaimDataModel 
                { 
                    Name = QueryParams.Cda.Token.PeisId,
                    Data = peisStartCode[..4] + BitConverter.ToString(
                        SHA1.HashData(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()))
                    ).Replace("-", "").ToLower()[4..]
                })
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