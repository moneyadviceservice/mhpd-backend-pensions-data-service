using System.Net;
using System.Text.Json;
using MhpdCommon.Extensions;
using MhpdCommon.Models.Configuration;
using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.Models.MHPDModels;
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
public class PensionsDataController(
    ILogger<PensionsDataController> logger,
    IIdValidator idValidator,
    PensionsDataRequestValidatorPipeline requestValidators,
    PensionServiceClients serviceClients,
    IOptions<CommonServiceBusConfiguration> serviceBusOptions,
    IMessagingService messagingService)
    : ControllerBase
{
    private readonly ITokenIntegrationServiceClient _tokenIntegrationServiceClient = serviceClients.TokenIntegrationServiceClient;
    private readonly IRetrievalRecordServiceClient _retrievalRecordServiceClient = serviceClients.RetrievalRecordServiceClient;
    private readonly IRetrievedPensionsRecordClient _retrievedPensionsRecordClient = serviceClients.RetrievedPensionsRecordClient;

    private const string ExternalPensionPolicyId = "externalPensionPolicyId";
    
    [HttpGet]
    [Route("pensions-data")]
    public async Task<IActionResult> GetPensionsDataAsync([FromHeader] RequestHeaderModel requestHeader)
    {
        logger.LogRequest(requestHeader);
        
        if (string.IsNullOrEmpty(requestHeader.UserSessionId))
        {
            return await Task.FromResult<IActionResult>(BadRequest(TokenValidationMessages.MissingUserSessionId));
        }
        
        if (!idValidator.IsValidGuid(requestHeader.UserSessionId))
        {
            return await Task.FromResult<IActionResult>(BadRequest(TokenValidationMessages.InvalidUserSessionId));
        }

        // Get the pensions retrieval record associated with the passed userSessionId
        var retrievalRecordResult = await _retrievalRecordServiceClient.GetAsync(requestHeader);
        logger.LogResponse(retrievalRecordResult);

        var response = new PensionsDataResponseModel
        {
            PensionPolicies = new List<PensionPolicy>(),
            PeiInformation = new PeiInformation
            {
                PeiRetrievalComplete = retrievalRecordResult.PeiRetrievalComplete,
                PeiData = retrievalRecordResult.PeiData
            },
            PensionsDataRetrievalComplete = retrievalRecordResult.PeiRetrievalComplete
        };
        
        // Check pensions-retrieval-records response has any PeiData items with retrievalStatus = RETRIEVAL_REQUESTED
        if (retrievalRecordResult.PeiData.Exists(s => s.RetrievalStatus == RetrievalStatusConstants.RetrievalRequested)) // Move this to the MHPD common services
        {
            logger.LogRequest(retrievalRecordResult.Id);
            
            // Call the GET retrieved-pension-records endpoint to retrieve the retrieved pension records associated
            // with the Pensions Retrieval Record returned in the GET pensions-retrieval-records response.
            var retrievedRecordResult = await _retrievedPensionsRecordClient.GetAsync(new PensionsRetrievalRecordIdModel
            {
                PensionsRetrievalRecordId = retrievalRecordResult.Id
            });
            
            logger.LogResponse(retrievedRecordResult);

            if (retrievedRecordResult.Count > 0)
            {
                // Mash results before sending
                response.PensionPolicies = GetMashedData(retrievedRecordResult);

                // Update retrieval status
                response.PeiInformation.PeiData = UpdateRetrievalStatus(retrievalRecordResult.PeiData);
            }
        }
        
        response.PensionsDataRetrievalComplete = response.PensionsDataRetrievalComplete = IsPensionsDataRetrievalComplete(
            retrievalRecordResult.PeiRetrievalComplete,
            retrievalRecordResult.PeiData
        );
        
        logger.LogResponse(response);

        return Ok(response);
    }

    [HttpPost]
    [Route("pensions-data")]
    public async Task<IActionResult> PostPensionsDataAsync([FromBody] PensionsDataRequestModel request, [FromHeader] RequestHeaderModel requestHeader)
    {
        logger.LogRequest(requestHeader);
        logger.LogRequest(request);
        
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
        var result = await _tokenIntegrationServiceClient
            .PostAsync(CreateCdaTokenServiceRequestModel(request, logger), requestHeader);
        
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
    
    private static CdaTokenRequestModel CreateCdaTokenServiceRequestModel(PensionsDataRequestModel request, ILogger<PensionsDataController> logger)
    {
        var cdaTokenRequest = new CdaTokenRequestModel
        {
            GrantType = TokenQueryParams.AuthorizationCodeGrantType,
            ClientId = TokenQueryParams.ValidClientId,
            ClientSecret = TokenQueryParams.ValidClientSecret,
            Code = request.AuthorisationCode,
            RedirectUri = request.RedirectUri,
            CodeVerifier = request.CodeVerifier
        };
        
        // Log cdaTokenServiceRequest
        logger.LogRequest(cdaTokenRequest);

        return cdaTokenRequest;
    }
    
    private static List<PensionPolicy>? GetMashedData(List<RetrievedPensionRecord> retrievedRecordResult)
    {
        var groupedRecords = retrievedRecordResult
            .Where(record => record.RetrievalResult?.ValueKind == JsonValueKind.Array) // Filter for records with array RetrievalResult
            .SelectMany<RetrievedPensionRecord, JsonElement>(record => record.RetrievalResult?.EnumerateArray()!) // Flatten the arrays into a single sequence
            .GroupBy(item =>
            {
                // Safely attempt to get the property
                if (item.TryGetProperty(ExternalPensionPolicyId, out var externalPolicyId))
                {
                    return externalPolicyId.GetString();
                }
                return null;
            })
            .ToList();

        return groupedRecords.Select(group => new PensionPolicy { PensionArrangements = group.ToList() }).ToList();
    }
    
    private static List<PeiDataModel> UpdateRetrievalStatus(List<PeiDataModel> data)
    {
        foreach (var peiData in data.Where(peiData => 
                     data.Exists(p => p.Pei == peiData.Pei && 
                                      p.RetrievalStatus == RetrievalStatusConstants.RetrievalRequested)))
        {
            peiData.RetrievalStatus = RetrievalStatusConstants.RetrievalComplete;
        }

        return data;
    }
    
    private static bool IsPensionsDataRetrievalComplete(bool peiRetrievalComplete, List<PeiDataModel> peiData)
    {
        // Return true if PeiRetrievalComplete is true and either no PeiData or all PeiData have status "RETRIEVAL_COMPLETE"
        if (peiRetrievalComplete)
        {
            return peiData.Count == 0 || peiData.TrueForAll(p => p.RetrievalStatus == RetrievalStatusConstants.RetrievalComplete);
        }
    
        // Return false if PensionsDataRetrievalComplete is false
        return false;
    }
}