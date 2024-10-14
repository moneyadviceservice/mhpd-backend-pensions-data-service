using System.Net;
using MhpdCommon.Extensions;
using MhpdCommon.Models.Configuration;
using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.Models.RequestHeaderModel;
using MhpdCommon.TokenValidation;
using MhpdCommon.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PensionsDataService.HttpClients;
using PensionsDataService.Models;

namespace PensionsDataService.Controllers;

[Route("/")]   
[ApiController]
public class PensionsDataController(ILogger<PensionsDataController> logger,
    IIdValidator idValidator,
    PensionsDataRequestValidatorPipeline requestValidators,
    ITokenIntegrationServiceClient tokenIntegrationServiceClient,
    IOptions<CommonServiceBusConfiguration> serviceBusOptions,
    IMessagingService messagingService) : ControllerBase
{
    [HttpPost]
    [Route("pensions-data")]
    public async Task<IActionResult> PostPensionsDataAsync([FromBody] PensionsDataRequestModel request, [FromHeader] RequestHeaderModel requestHeader)
    {
        logger.LogRequest(requestHeader);
        
        if (string.IsNullOrEmpty(requestHeader.Iss))
        {
            return await Task.FromResult<IActionResult>(BadRequest(TokenValidationMessages.MissingIss));
        }
        
        if (string.IsNullOrEmpty(requestHeader.UserSessionId))
        {
            return await Task.FromResult<IActionResult>(BadRequest(TokenValidationMessages.MissingUserSessionId));
        }
        
        if (!idValidator.IsValidGuid(requestHeader.UserSessionId))
        {
            return await Task.FromResult<IActionResult>(BadRequest(TokenValidationMessages.InvalidUserSessionId));
        }
        
        var validationResult = requestValidators.Validate(request);
        if (!validationResult.IsValid)
        {
            logger.LogError("Error: {ErrorMessage}", validationResult.ErrorMessage);
            return await Task.FromResult<IActionResult>(BadRequest(validationResult.ErrorMessage));
        }
        
        // Get pei from the token integration service
        var result = await tokenIntegrationServiceClient
            .PostAsync(CreateCdaTokenServiceRequestModel(request), requestHeader);
        
        // Post a message to initiate a process to retrieve the Pensions Data for the userSessionId
        var message = CreateRequestPayload(result, requestHeader);
        logger.LogInformation("Post a message to initiate a process to retrieve the Pensions Data for the userSessionId {UserSessionId}",
            message.UserSessionId);
        await messagingService.SendMessageAsync(message, serviceBusOptions.Value.OutboundQueue!, Guid.NewGuid().ToString());

        var response = StatusCode((int)HttpStatusCode.NoContent);
        logger.LogResponse(response);
        
        return await Task.FromResult<IActionResult>(response);
    }
    
    private static PensionRetrievalPayload CreateRequestPayload(PeiRetrievalDetailsResponseModel response, RequestHeaderModel requestHeader)
    {
        return new PensionRetrievalPayload
        {
            Iss = requestHeader.Iss,
            PeisId = response.PeisId,
            UserSessionId = requestHeader.UserSessionId
        };
    }
    
    private static CdaTokenRequestModel CreateCdaTokenServiceRequestModel(PensionsDataRequestModel request)
    {
        return new CdaTokenRequestModel
        {
            GrantType = TokenQueryParams.AuthorizationCodeGrantType,
            ClientId = TokenQueryParams.ValidClientId,
            ClientSecret = TokenQueryParams.ValidClientSecret,
            Code = request.AuthorisationCode,
            RedirectUri = request.RedirectUri,
            CodeVerifier = request.CodeVerifier
        };
    }
}