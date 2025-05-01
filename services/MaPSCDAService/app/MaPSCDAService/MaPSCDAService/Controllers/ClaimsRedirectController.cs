using MaPSCDAService.Models;
using MhpdCommon.Extensions;
using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.Models.MHPDModels;
using MhpdCommon.Models.RequestHeaderModel;
using MhpdCommon.Repository;
using MhpdCommon.SharedHttpClient;
using MhpdCommon.TokenValidation;
using MhpdCommon.Utils;
using Microsoft.AspNetCore.Mvc;

namespace MaPSCDAService.Controllers;

[Route("/")]
[ApiController]
public class ClaimsRedirectController(ILogger<ClaimsRedirectController> logger, 
    IIdValidator validator,
    ITokenUtility tokenUtility,
    IPeiServiceClient peiServiceClient,
    ITokenIntegrationServiceClient tokenIntegrationServiceClient,
    ICosmosDbRepository<UserSessionData> sessionDataRepository) : ControllerBase
{
    [HttpGet]
    [Route("claims-gathering-redirect")]
    public async Task<IActionResult> GetRedirectAsync([FromHeader] RequestHeaderModel request)
    {
        logger.LogRequest(request);

        if (!TryValidateRequestHeader(request, out var validationMessage))
        {
            logger.LogError("Error: {ErrorMessage}", validationMessage);
            return BadRequest(validationMessage);
        }

        var rqp = tokenUtility.GenerateJwt(new CustomClaimDataModel
        {
            Subject = request.UserSessionId + "@" + request.Iss,
            Issuer = request.Iss
        });

        var sessionData = await sessionDataRepository.GetByIdAsync(request.UserSessionId!, request.UserSessionId!);
        if(sessionData == null)
        {
            return GetServerErrorResponse(Constants.NoSessionDataFound);
        }
        
        if(!validator.IsValidPeisId(sessionData.PeisId))
        {
            return GetServerErrorResponse(Constants.MissingOrInvalidRedirectPeisId);
        }

        var peiResponse = await GetPeiDataAsync(request, sessionData);
        if(peiResponse.ResponseMessage!.ResponseStatusCode != System.Net.HttpStatusCode.Unauthorized)
        {
            return GetServerErrorResponse(Constants.UnableToTriggerRedirect);
        }

        var tokenResponse = await GetAccessToken(peiResponse, rqp, request.CorrelationId!, sessionData.ClientId);
        if (tokenResponse.StatusCode != System.Net.HttpStatusCode.Forbidden || !ValidateRedirectResponse(tokenResponse.UserRedirectDetails))
        {
            return GetServerErrorResponse(Constants.UnableToTriggerRedirect);
        }

        var response = new ClaimsRedirectResponseModel
        {
            Rqp = rqp,
            RequestId = Guid.NewGuid().ToString(),
            ClaimsRedirectUrl = tokenResponse.UserRedirectDetails!.RedirectUser,
            Ticket = tokenResponse.UserRedirectDetails.Ticket
        };

        logger.LogResponse(response);

        return Ok(response);
    }

    private bool ValidateRedirectResponse(ClaimsGatheringResponseModel? response)
    {
        if (response == null)
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

    private async Task<CdaPeisServiceResponseModel> GetPeiDataAsync(RequestHeaderModel request, UserSessionData sessionData)
    {
        return await peiServiceClient.GetPeiDataAsync(new PeiRequestModel
        {
            UserSessionId = request.UserSessionId!,
            Iss = request.Iss!,
            PeisId = sessionData.PeisId!,
            CorrelationId = request.CorrelationId!
        });
    }

    private async Task<CdaTokenResponseModel> GetAccessToken(CdaPeisServiceResponseModel peiResponse, string rqp, string correlationId, string clientId)
    {
        return await tokenIntegrationServiceClient.PostAccessTokenAsync(new TokenClientRequestModel
        {
            Rqp = rqp,
            AsUri = peiResponse.ResponseMessage!.AsUri,
            Ticket = peiResponse.ResponseMessage.Ticket,
            ClientId = clientId
        }, correlationId);
    }

    private bool TryValidateRequestHeader(RequestHeaderModel requestHeader, out string? message)
    {
        if (!validator.IsValidGuid(requestHeader.UserSessionId))
        {
            message = TokenValidationMessages.InvalidUserSessionId;
            return false;
        }

        if (string.IsNullOrEmpty(requestHeader.CorrelationId))
        {
            requestHeader.CorrelationId = Guid.NewGuid().ToString();
        }

        if (!validator.IsValidGuid(requestHeader.CorrelationId))
        {
            message = Constants.InvalidCorrelationId;
            return false;
        }

        message = null;
        return true;
    }

    private ObjectResult GetServerErrorResponse(string message)
    {
        return StatusCode(500, message);
    }
}
