using MhpdCommon.Extensions;
using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.Models.RequestHeaderModel;
using MhpdCommon.TokenValidation;
using MhpdCommon.Utils;
using Microsoft.AspNetCore.Mvc;
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
    TokenRequestValidatorPipeline cdaRequestValidatorPipeline,
    ITokenUtility tokenUtility)
    : ControllerBase
{
    [HttpPost]
    [Route("rpts")]
    public async Task<IActionResult> PostAsync([FromBody] TokenIntegrationRequestModel request, [FromHeader] RequestHeaderModel requestHeader)
    {            
        logger.LogRequest(request);
            
        if (string.IsNullOrEmpty(requestHeader.XRequestId) || !idValidator.IsValidGuid(requestHeader.XRequestId))
        {
            return await Task.FromResult<IActionResult>(BadRequest(TokenValidationMessages.InvalidXRequestId));
        }

        var validation = validatorPipeline.Validate(request);
        if (!validation.IsValid)
        {
            logger.LogError("Error: {ErrorMessage}", validation.ErrorMessage);
            return await Task.FromResult<IActionResult>(BadRequest(validation.ErrorMessage));
        }
            
        var cdaTokenRequestModelRequest = CreateCdaTokenServiceRequestModel(request);
        var result = await iCdaServiceClient.PostAsync(cdaTokenRequestModelRequest, requestHeader);

        var response = new TokenIntegrationResponseModel
        {
            Rpt = result.AccessToken
        };
        
        logger.LogResponse(response);
        Console.WriteLine(response);
            
        return Ok(response);             
    }

    [HttpPost]
    [Route("pei_retrieval_details")]
    public async Task<IActionResult> PostPeiRetrievalDetailsAsync([FromQuery] CdaTokenRequestModel request, [FromHeader] RequestHeaderModel requestHeader)
    {
        logger.LogRequest(request);
        
        if (string.IsNullOrEmpty(requestHeader.XRequestId) || !idValidator.IsValidGuid(requestHeader.XRequestId))
        {
            requestHeader.XRequestId = Guid.NewGuid().ToString();
        }

        var validation = cdaRequestValidatorPipeline.Validate(request);
        if (!validation.IsValid)
        {
            logger.LogError("Error: {ErrorMessage}", validation.ErrorMessage);
            return await Task.FromResult<IActionResult>(BadRequest(validation.ErrorMessage));
        }
        
        var result = await iCdaServiceClient.PostAsync(request, requestHeader);
        var internalServerErrorResponse = Task.FromResult<IActionResult>(StatusCode(500, "Internal server error"));
        
        var response = new PeiRetrievalDetailsResponseModel();
        
        // Check if IdToken is null and handle the error
        if (result.IdToken == null)
        {
            logger.LogError("IdToken is null in the response");
            return await internalServerErrorResponse;
        }

        try
        {
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
                return await internalServerErrorResponse;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "id_token signature invalid");
            return await internalServerErrorResponse;
        }
        
        logger.LogResponse(response);
        return Ok(response);
    }

    private static CdaTokenRequestModel CreateCdaTokenServiceRequestModel(TokenIntegrationRequestModel requestBody)
    {
        return new CdaTokenRequestModel
        {
            GrantType = TokenQueryParams.UmaGrantType,
            ClaimToken = requestBody.Rqp,
            ClaimTokenFormat = TokenQueryParams.PensionDashboardRqp,
            Scope = TokenQueryParams.Owner,
            Ticket = requestBody.Ticket,
        };
    }
}