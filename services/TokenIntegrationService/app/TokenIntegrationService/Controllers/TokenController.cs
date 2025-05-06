using MhpdCommon.Constants;
using MhpdCommon.Extensions;
using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.Models.MHPDModels;
using MhpdCommon.Models.RequestHeaderModel;
using MhpdCommon.TokenValidation;
using MhpdCommon.Utils;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using TokenIntegrationService.HttpClients;
using TokenIntegrationService.Models;

namespace TokenIntegrationService.Controllers;

[Route("/")]
[ApiController]
public class TokenController(
    ICdaServiceClient iCdaServiceClient,
    ILogger<TokenController> logger,
    IIdValidator idValidator,
    TokenIntegrationRequestValidatorPipeline validatorPipeline,
    PensionsDataRequestValidatorPipeline cdaRequestValidatorPipeline,
    ITokenUtility tokenUtility,
    IJwtUtility jwtUtility)
    : ControllerBase
{
    [HttpPost]
    [Route("rpts")]
    [ProducesResponseType(typeof(CdaTokenResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PostAsync([FromBody] TokenClientRequestModel request, [FromHeader(Name = HeaderConstants.CorrelationId)] string? correlationId)
    {
        var requestHeader = new RequestHeaderModel
        {
            CorrelationId = correlationId
        };

        if (!TryValidateRequest(validatorPipeline, request, requestHeader, out var message))
        {
            logger.LogError("Error: {ErrorMessage}", message);
            return await Task.FromResult<IActionResult>(BadRequest(message));
        }

        using var scope = logger.BeginCorrelationScope(requestHeader.CorrelationId!, $"{Constants.LogSource} Rpts");
        logger.LogRequest(request);
        
        var cdaTokenRequestModelRequest = CreateCdaTokenServiceRequestModel(request);
        var response = await iCdaServiceClient.PostAsync(cdaTokenRequestModelRequest);

        if (!await ValidateTokenResponseAsync(response, false))
        {
            return await GetInternalServerErrorResponse();
        }
        
        logger.LogResponse(response);
        return Ok(response);
    }

    [HttpPost]
    [Route("pei-retrieval-details")]
    public async Task<IActionResult> PostPeiRetrievalDetailsAsync([FromQuery] PensionsDataRequestModel request, 
        [FromHeader(Name = HeaderConstants.CorrelationId)] string? correlationId)
    {
        var requestHeader = new RequestHeaderModel
        {
            CorrelationId = correlationId
        };

        if (!TryValidateRequest(cdaRequestValidatorPipeline, request, requestHeader, out var message))
        {
            logger.LogError("Error: {ErrorMessage}", message);
            return await Task.FromResult<IActionResult>(BadRequest(message));
        }

        using var scope = logger.BeginCorrelationScope(requestHeader.CorrelationId!, $"{Constants.LogSource} - Retrieval Details");
        logger.LogRequest(request);

        var cdaTokenRequestModelRequest = CreateCdaTokenServiceRequestModel(request);
        var result = await iCdaServiceClient.PostAsync(cdaTokenRequestModelRequest);
        
        var response = new PeiRetrievalDetailsResponseModel();
        
        // Check if IdToken is null and handle the error
        if (result.IdToken == null)
        {
            logger.LogError("IdToken is null in the response");
            return await GetInternalServerErrorResponse();
        }
        
        try
        {
            // Validate the Jwt token signature
            await jwtUtility.ValidateJwtTokenWithKidAsync(result.IdToken, logger);

            // Attempt to decode the IdToken
            var claims = tokenUtility.DecodeJwt(result.IdToken);

            // Ensure "peis_id" claim exists before assigning it
            if (claims.TryGetValue("peis_id", out var peisId))
            {
                response.PeisId = peisId;
            }
            else
            {
                logger.LogError("id_token missing peis_id");
                return await GetInternalServerErrorResponse();
            }
        } 
        catch (Exception ex)
        {
            logger.LogError(ex, "id_token signature invalid");
            return await GetInternalServerErrorResponse();
        }
    
        logger.LogResponse(response);
        return Ok(response);
    }

    private async Task<bool> ValidateTokenResponseAsync(CdaTokenResponseModel response, bool validateToken = true)
    {
        if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.Forbidden)
        {
            logger.LogError("Token request was not successful");
            return false;
        }

        if(response.StatusCode == HttpStatusCode.Forbidden && !ValidateRedirectResponse(response.UserRedirectDetails))
        {
            return false;
        }

        if (response.StatusCode == HttpStatusCode.OK)
        {
            // Check if AccessToken is null and handle the error
            if (string.IsNullOrEmpty(response.AccessToken))
            {
                logger.LogError("AccessToken is null in the response");
                return false;
            }

            // For AccessToken (RPT), introspection is not required, and it will not contain a kid. It functions as a pass-through token.
            if (!validateToken)
            {
                return true;
            }

            try
            {
                // Validate the Jwt token signature
                await jwtUtility.ValidateJwtTokenWithKidAsync(response.AccessToken!, logger);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "AccessToken signature invalid");
                return false;
            }
        }

        return true;
    }

    private bool ValidateRedirectResponse(ClaimsGatheringResponseModel? response)
    {
        if( response == null)
        {
            logger.LogError("No redirect details were returned for claim gathering");
            return false;
        }

        var result = response.IsValidClaimsGatheringResponse();

        if (!result.IsValid)
        {
            logger.LogError("Invalid ClaimsGatheringResponse: {ErrorMessage}", result.ErrorMessage);
            return false;
        }
        return true;
    }

    private bool TryValidateRequest<T>(IRequestValidator<T> validator, T request, RequestHeaderModel headerModel, out string? message)
    {
        var validation = validator.Validate(request);
        if (!validation.IsValid)
        {
            message = validation.ErrorMessage;
            return false;
        }

        if (string.IsNullOrEmpty(headerModel.CorrelationId))
        {
            headerModel.CorrelationId = Guid.NewGuid().ToString();
        }

        if (!idValidator.IsValidGuid(headerModel.CorrelationId))
        {
            message = Constants.InvalidCorrelationId;
            return false;
        }

        message = null;
        return true;
    }

    private static CdaTokenRequestModel CreateCdaTokenServiceRequestModel(TokenClientRequestModel requestBody)
    {
        return new CdaTokenRequestModel
        {
            GrantType = TokenQueryParams.UmaGrantType,
            ClaimToken = requestBody.Rqp,
            ClaimTokenFormat = TokenQueryParams.PensionDashboardRqp,
            Scope = TokenQueryParams.Owner,
            Ticket = requestBody.Ticket,
            Pct = requestBody.Pct,
            ClientId = requestBody.ClientId
        };
    }
    
    private static CdaTokenRequestModel CreateCdaTokenServiceRequestModel(PensionsDataRequestModel request)
    {
        return new CdaTokenRequestModel
        {
            GrantType = TokenQueryParams.AuthorizationCodeGrantType,
            ClientId = request.ClientId,
            ClientSecret = request.ClientSecret,
            Code = request.AuthorisationCode,
            CodeVerifier = request.CodeVerifier,
            RedirectUrl = request.RedirectUrl
        };
    }

    private Task<IActionResult> GetInternalServerErrorResponse()
    {
        return Task.FromResult<IActionResult>(StatusCode(500, "Internal server error"));
    }
}