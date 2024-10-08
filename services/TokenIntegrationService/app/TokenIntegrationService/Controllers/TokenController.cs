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
    TokenIntegrationRequestValidatorPipeline validatorPipeline)
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
            
        var result = await iCdaServiceClient.PostRptAsync(cdaTokenRequestModelRequest, requestHeader);
        var response = new TokenIntegrationResponseModel { Rpt = result.AccessToken };
        
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