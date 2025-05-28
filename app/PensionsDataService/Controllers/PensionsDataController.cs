using System.Text.Json;
using MhpdCommon.Constants;
using MhpdCommon.Extensions;
using MhpdCommon.Models.Configuration;
using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.Models.MHPDModels;
using MhpdCommon.Models.RequestHeaderModel;
using MhpdCommon.Repository;
using MhpdCommon.SharedHttpClient;
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
    IOptions<PeiOrchestrationSettings> peiRetrievalOptions,
    ICosmosDbRepository<UserSessionData> userSessionDataRepository)
    : ControllerBase
{
    private readonly ITokenIntegrationServiceClient _tokenIntegrationServiceClientRpts = serviceClients.TokenIntegrationServiceClient;
    private readonly IMapsCdaServiceClient _mapsCdaServiceClient = serviceClients.MapsCdaServiceClient;
    private readonly IRetrievalRecordServiceClient _retrievalRecordServiceClient = serviceClients.RetrievalRecordServiceClient;
    private readonly IRetrievedPensionsRecordClient _retrievedPensionsRecordClient = serviceClients.RetrievedPensionsRecordClient;
    private readonly IOptions<CommonServiceBusConfiguration> _serviceBusOptions = serviceClients.ServiceBusOptions;
    private readonly IMessagingService _messagingService = serviceClients.MessagingService;
    private readonly int _predictedTotalDataRetrievalTime = peiRetrievalOptions.Value.TotalPensionRetrievalDuration;

    private const string ErrorCode = "errorCode";
    private const string ExternalPensionPolicyId = "externalPensionPolicyId";
    private const string ErrorMessage = "Error: {ErrorMessage}";
    private const string AsUri = "https://example.com"; //NOSONAR

    private static readonly IEnumerable<string> CompletedStates =
    [
        PensionProviderConstants.RetrievalStatus.RetrievalComplete,
        PensionProviderConstants.RetrievalStatus.RetrievalFailed
    ];
    
    [HttpGet]
    [Route("pensions-data")]
    public async Task<IActionResult> GetPensionsDataAsync([FromHeader(Name = HeaderConstants.UserSessionId)] string? userSessionId, 
        [FromHeader(Name = HeaderConstants.CorrelationId)] string? correlationId)
    {
        var requestHeader = new RequestHeaderModel
        {
            UserSessionId = userSessionId,
            CorrelationId = correlationId
        };

        if (!TryValidateRequestHeader(requestHeader, out var validationMessage))
        {
            logger.LogError(ErrorMessage, validationMessage);
            return await Task.FromResult<IActionResult>(BadRequest(validationMessage));
        }

        using var scope = logger.BeginCorrelationScope(requestHeader.CorrelationId!, $"{Constants.LogSource} GET");
        logger.LogRequest(requestHeader);

        // Get the pensions retrieval record associated with the passed userSessionId
        var retrievalRecordResult = await _retrievalRecordServiceClient.GetAsync(requestHeader);
        logger.LogResponse(retrievalRecordResult);

        if (string.IsNullOrEmpty(retrievalRecordResult.Id))
        {
            return new JsonResult(null) { StatusCode = StatusCodes.Status200OK };
        }

        var response = new PensionsDataResponseModel
        {
            PensionPolicies = [],
            PeiInformation = new PeiInformation
            {
                PeiRetrievalComplete = retrievalRecordResult.PeiRetrievalComplete,
                PeiData = retrievalRecordResult.PeiData
            },
            PensionsDataRetrievalComplete = retrievalRecordResult.PeiRetrievalComplete,
            PredictedTotalDataRetrievalTime = _predictedTotalDataRetrievalTime
        };
        
        // Check pensions-retrieval-records response has any PeiData items with retrievalStatus = RETRIEVAL_REQUESTED
        if (retrievalRecordResult.PeiData.Exists(s => s.RetrievalStatus == PensionProviderConstants.RetrievalStatus.RetrievalRequested))
        {
            logger.LogRequest(retrievalRecordResult.Id);
            
            // Call the GET retrieved-pension-records endpoint to retrieve the retrieved pension records associated
            // with the Pensions Retrieval Record returned in the GET pensions-retrieval-records response.
            var retrievedRecordResult = await _retrievedPensionsRecordClient.GetAsync(new PensionsRetrievalRecordIdModel
            {
                PensionsRetrievalRecordId = retrievalRecordResult.Id
            }, requestHeader);
            
            logger.LogResponse(retrievedRecordResult);

            if (retrievedRecordResult.Count > 0)
            {
                // Mash results before sending
                response.PensionPolicies = GetMashedData(retrievedRecordResult);

                // Update retrieval status
                response.PeiInformation.PeiData = UpdateRetrievalStatus(retrievalRecordResult.PeiData, retrievedRecordResult);
            }
        }
        
        response.PensionsDataRetrievalComplete = IsPensionsDataRetrievalComplete(
            retrievalRecordResult.PeiRetrievalComplete,
            retrievalRecordResult.PeiData
        );

        response.PredictedRemainingDataRetrievalTime = GetRemainingRetrievalTime(retrievalRecordResult, _predictedTotalDataRetrievalTime);
        
        logger.LogResponse(response);

        return Ok(response);
    }

    [HttpPost]
    [Route("pensions-data")]
    public async Task<IActionResult> PostPensionsDataAsync([FromBody] PensionsDataRequestModel request, [FromHeader] RequestHeaderModel requestHeader)
    {
        if (!TryValidateRequests(request, requestHeader, out var validationMessage))
        {
            logger.LogError(ErrorMessage, validationMessage);
            return await Task.FromResult<IActionResult>(BadRequest(validationMessage));
        }

        using var scope = logger.BeginCorrelationScope(requestHeader.CorrelationId!, $"{Constants.LogSource} POST");
        logger.LogRequest(requestHeader);
        logger.LogRequest(request);

        // Get pei from the token integration service
        var result = await _tokenIntegrationServiceClientRpts
            .PostIdTokenAsync(CreateCdaTokenServiceRequestModel(request, logger), requestHeader.CorrelationId);
        
        logger.LogResponse(result);

        var userSessionId = requestHeader.UserSessionId!;

        if (!IsValidUserSessionData(userSessionId, result.PeisId))
        {
            return await Task.FromResult<IActionResult>(StatusCode(500, "Internal server error"));
        }
        
        // Add the idToken to the userSessionData container against the userSessionId for downstream use
        await userSessionDataRepository.InsertItemAsync(new UserSessionData
        {
            Id = userSessionId,
            UserSessionId = userSessionId,
            PeisId = result.PeisId!,
            ClientId = request.ClientId!,
        }, userSessionId);
        
        logger.LogInformation("UserSessionData created for userSessionId {UserSessionId} with PeisId {PeisId}", userSessionId, result.PeisId);

        var response = Accepted();
        logger.LogResponse(response);
        
        return await Task.FromResult<IActionResult>(response);
    }

    // This endpoint is called once the claims gathering process has been completed
    [HttpPost]
    [Route("pensions-data-retrieval")]
    public async Task<IActionResult> PostPensionsDataRetrievalAsync([FromBody] PensionsDataRetrievalRequest request,
        [FromHeader] RequestHeaderModel requestHeader)
    {
        if (!TryValidateDataRetrievalRequests(request, requestHeader, out var validationMessage))
        {
            logger.LogError(ErrorMessage, validationMessage);
            return BadRequest(validationMessage);
        }

        using var scope = logger.BeginCorrelationScope(requestHeader.CorrelationId!, $"{Constants.LogSource} POST PensionsDataRetrieval");
        logger.LogRequest(requestHeader);
        logger.LogRequest(request);

        var userSessionId = requestHeader.UserSessionId!;

        var rqpResponse = await GetRqpFromMapsCdaService(requestHeader, userSessionId);
        if (rqpResponse is null || string.IsNullOrEmpty(rqpResponse.Rqp))
        {
            return InternalServerErrorResult();
        }

        var tokenResult = await GetAccessToken(request, rqpResponse.Rqp, requestHeader, userSessionId);
        if (tokenResult is null)
        {
            return InternalServerErrorResult();
        }

        var peisId = await StoreSessionData(userSessionId, tokenResult);
        if (string.IsNullOrEmpty(peisId))
        {
            return InternalServerErrorResult();
        }

        await PostPensionDataRetrievalMessage(peisId, requestHeader, userSessionId);

        var response = Accepted(new { predictedTotalDataRetrievalTime = _predictedTotalDataRetrievalTime });
        logger.LogResponse(response);
        return response;
    }
    
    [HttpDelete]
    [Route("pensions-data")]
    public async Task<IActionResult> DeletePensionsDataAsync([FromHeader] RequestHeaderModel requestHeader)
    {
        if (!TryValidateRequestHeader(requestHeader, out var validationMessage))
        {
            logger.LogError(ErrorMessage, validationMessage);
            return await Task.FromResult<IActionResult>(BadRequest(validationMessage));
        }

        using var scope = logger.BeginCorrelationScope(requestHeader.CorrelationId!, $"{Constants.LogSource} DELETE");
        logger.LogRequest(requestHeader);

        // Get the pensions retrieval record associated with the passed userSessionId
        var retrievalRecordResult = await _retrievalRecordServiceClient.GetAsync(requestHeader);
        logger.LogResponse(retrievalRecordResult);

        if (!string.IsNullOrEmpty(retrievalRecordResult.Id))
        {
            var retrievalCount = await _retrievalRecordServiceClient.DeleteAsync(requestHeader);
            var retrievedCount = await _retrievedPensionsRecordClient.DeleteAsync(retrievalRecordResult.Id, requestHeader.CorrelationId!);

            logger.LogWarning("Delete request removed {RetrievalCount} pension retrieval records and {RetrievedCount} retrieved pension records", retrievalCount, retrievedCount);
        }

        return new NoContentResult();
    }

    private static int GetRemainingRetrievalTime(PensionsRetrievalRecord retrievalRecord, int totalEstimatedDuration)
    {
        if (retrievalRecord.PeiRetrievalComplete)
        {
            return 0;
        }

        var estimatedCompletionTime = retrievalRecord.JobStartTimestamp.AddSeconds(totalEstimatedDuration);
        var remainingDuration = (estimatedCompletionTime - DateTime.UtcNow).TotalSeconds;

        return Math.Max((int)remainingDuration, 1);
    }
    
    private bool TryValidateRequests(PensionsDataRequestModel request, RequestHeaderModel requestHeader, out string? message)
    {
        if (!TryValidateRequestHeader(requestHeader, out message))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(requestHeader.Iss))
        {
            message = TokenValidationMessages.MissingIss;
            return false;
        }

        var validationResult = requestValidators.Validate(request);
        if (!validationResult.IsValid)
        {
            message = validationResult.ErrorMessage;
            return false;
        }

        message = null;
        return true;
    }
    
    private bool TryValidateDataRetrievalRequests(PensionsDataRetrievalRequest request, RequestHeaderModel requestHeader, out string? message)
    {
        if (!TryValidateRequestHeader(requestHeader, out message))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(requestHeader.Iss))
        {
            message = TokenValidationMessages.MissingIss;
            return false;
        }
        
        if (string.IsNullOrWhiteSpace(request.Ticket))
        {
            message = TokenValidationMessages.InvalidPermissionTicket;
            return false;
        }
        
        if (string.IsNullOrWhiteSpace(request.ClientId))
        {
            message = TokenValidationMessages.InvalidClientIdFormat;
            return false;
        }
        
        if (!JweValidator.IsJweFormatValid(request.Ticket))
        {
            message = TokenValidationMessages.InvalidJweTicketQueryFormat;
            return false;
        }
        
        message = null;
        return true;
    }

    private bool TryValidateRequestHeader(RequestHeaderModel requestHeader, out string? message)
    {
        if (!idValidator.IsValidGuid(requestHeader.UserSessionId))
        {
            message = TokenValidationMessages.InvalidUserSessionId;
            return false;
        }

        if (string.IsNullOrEmpty(requestHeader.CorrelationId))
        {
            requestHeader.CorrelationId = Guid.NewGuid().ToString();
        }

        if (!idValidator.IsValidGuid(requestHeader.CorrelationId))
        {
            message = Constants.InvalidCorrelationId;
            return false;
        }

        message = null;
        return true;
    }

    private static TokenClientRequestModel CreateCdaTokenServiceRptsRequestModel(string ticket, 
        string rqp,
        ILogger<PensionsDataController> logger, string? asUri, string clientId)
    {
        var request = new TokenClientRequestModel
        {
            Ticket = ticket,
            Rqp = rqp,
            AsUri = asUri,
            ClientId = clientId
        };
        
        logger.LogRequest(request);

        return request;
    }

    private static PensionsDataRequestModel CreateCdaTokenServiceRequestModel(PensionsDataRequestModel request, ILogger<PensionsDataController> logger)
    {
        var requestModel = new PensionsDataRequestModel
        {
            ClientId = request.ClientId,
            ClientSecret = request.ClientSecret,
            AuthorisationCode = request.AuthorisationCode,
            RedirectUrl = request.RedirectUrl,
            CodeVerifier = request.CodeVerifier
        };
        
        logger.LogRequest(requestModel);

        return requestModel;
    }
    
    private static List<PensionPolicy> GetMashedData(List<RetrievedPensionRecord> retrievedRecordResult)
    {
        var policies = new List<PensionPolicy>();
        var mappedData = GetMappedRetrievalResults(retrievedRecordResult);

        foreach (var policy in mappedData.Item1)
        {
            if (policy.Value is JsonElement jsonElement)
            {
                policies.Add(new PensionPolicy
                {
                    PensionArrangements = new List<JsonElement> { jsonElement } // Add as a single-item list
                });
            }
        }

        policies.AddRange(mappedData.Item2.Select(kvp => new PensionPolicy { PensionArrangements = kvp.Value }));

        return policies;
    }
    
    private static List<PeiDataModel> UpdateRetrievalStatus(List<PeiDataModel> peiDataList, List<RetrievedPensionRecord> retrievedPensionsRecords)
    {
        foreach (var peiData in peiDataList)
        {
            // Check if there is a matching pei in the retrievedPensionsRecords
            var matchingRecord = retrievedPensionsRecords.Find(record => record.Pei == peiData.Pei);

            // If there is a matching record, set the retrieved status. Otherwise, retain the existing retrievalStatus
            peiData.RetrievalStatus = GetPeiStatus(matchingRecord, peiData.RetrievalStatus);
        }

        return peiDataList;
    }

    private static string? GetPeiStatus(RetrievedPensionRecord? record, string? retrievalStatus)
    {
        if (record != null && record.RetrievalResult is JsonElement { ValueKind: JsonValueKind.Object } retrievalResult && 
            retrievalResult.TryGetProperty(ErrorCode, out _))
        {
            return PensionProviderConstants.RetrievalStatus.RetrievalFailed;
        }

        if (record != null && record.RetrievalResult is JsonElement { ValueKind: JsonValueKind.Array })
        {
            return PensionProviderConstants.RetrievalStatus.RetrievalComplete;
        }

        return retrievalStatus;
    }
    
    private static bool IsPensionsDataRetrievalComplete(bool peiRetrievalComplete, List<PeiDataModel> peiData)
    {
        // Return true if PeiRetrievalComplete is true and either no PeiData or all PeiData have a status indicating retrieval was completed
        if (peiRetrievalComplete)
        {
            return peiData.Count == 0 || peiData.TrueForAll(p => CompletedStates.Contains(p.RetrievalStatus));
        }
    
        // Return false if PensionsDataRetrievalComplete is false
        return false;
    }

    private static Tuple<Dictionary<int, dynamic>, Dictionary<string, List<JsonElement>>> GetMappedRetrievalResults(List<RetrievedPensionRecord> retrievedRecordResults)
    { 
        var policyList = new Dictionary<int, dynamic>();
        var linkedExternalPolicies = new Dictionary<string, List<JsonElement>>();

        var pensionPolicyId = 1;
        foreach (var record in retrievedRecordResults)
        {
            ProcessRetrievalResults(record, ref policyList, ref linkedExternalPolicies, pensionPolicyId);
            pensionPolicyId++;
        }

        return new Tuple<Dictionary<int, dynamic>, Dictionary<string, List<JsonElement>>>(policyList, linkedExternalPolicies);
    }

    private static int GetPolicyKey(string policyId, Dictionary<int, dynamic> policyList)
    {
       return policyList.FirstOrDefault(s =>
       {
           if (s.Value is string strValue)
           {
               return strValue == policyId;
           }

           return false;
       }).Key;
    }

    private static void ProcessRetrievalResults(RetrievedPensionRecord record,
        ref Dictionary<int, dynamic> policyList,
        ref Dictionary<string, List<JsonElement>> linkedExternalPolicies,
        int pensionPolicyId)
    {
        if (record.RetrievalResult is JsonElement { ValueKind: JsonValueKind.Array } retrievalResult)
        {
            foreach (var item in retrievalResult.EnumerateArray())
            {
                // Check if it has an externalPensionPolicyId
                if (item.TryGetProperty(ExternalPensionPolicyId, out var policyId))
                {
                    ProcessRecord(policyId.ToString(), item, ref linkedExternalPolicies, ref policyList, ref pensionPolicyId);
                }
                else
                {
                    if (!policyList.TryAdd(pensionPolicyId, item))
                    {
                        var key = policyList.First(s => s.Key == pensionPolicyId).Value.ToString();
                        string result = key as string ?? Convert.ToString(key);
                        linkedExternalPolicies[result].Add(item);
                    }
                }
            }
        }
    }
    
    private static PensionRetrievalPayload CreateRequestPayload(string peisId, RequestHeaderModel requestHeader)
    {
        return new PensionRetrievalPayload
        {
            PeisId = peisId,
            Iss = requestHeader.Iss,
            UserSessionId = requestHeader.UserSessionId
        };
    }

    private static void ProcessRecord(string policyId,
        JsonElement item,
        ref Dictionary<string, List<JsonElement>> linkedExternalPolicies, 
        ref Dictionary<int, dynamic> policyList, 
        ref int pensionPolicyId)
    {
        // Add it to the linkedExternalPolicies
        if (linkedExternalPolicies.TryGetValue(policyId, out _))
        {
            linkedExternalPolicies[policyId].Add(item);

            // Get pensionPolicyList key
            pensionPolicyId = GetPolicyKey(policyId, policyList);
        }
        else
        {
            linkedExternalPolicies.Add(policyId, [item]);
            policyList.Add(pensionPolicyId, policyId);
        }
    }

    private bool IsValidUserSessionData(string userSessionId, string? peisId)
    {
        if (string.IsNullOrEmpty(userSessionId))
        {
            logger.LogError("UserSessionId is missing");
            return false;
        }
        
        if (string.IsNullOrEmpty(peisId))
        {
            logger.LogError("PeisId is missing");
            return false;
        }

        return true;
    }
    
    private async Task<MapsRqpServiceResponseModel?> GetRqpFromMapsCdaService(RequestHeaderModel requestHeader, string userSessionId)
    {
        var rqpResponse = await _mapsCdaServiceClient.GetRqp(requestHeader);
        if (string.IsNullOrEmpty(rqpResponse.Rqp) || !JwtValidator.IsJwtFormatValid(rqpResponse.Rqp))
        {
            logger.LogError("Invalid RQP for Id {UserSessionId}", userSessionId);
            return null;
        }
        return rqpResponse;
    }

    private async Task<CdaTokenResponseModel?> GetAccessToken(
        PensionsDataRetrievalRequest request,
        string rqp,
        RequestHeaderModel requestHeader,
        string userSessionId)
    {
        var tokenResult = await _tokenIntegrationServiceClientRpts.PostAccessTokenAsync(
            CreateCdaTokenServiceRptsRequestModel(request.Ticket, rqp, logger, AsUri, request.ClientId), requestHeader.CorrelationId);

        if (!IsValidToken(tokenResult.Pct) || !IsValidToken(tokenResult.AccessToken))
        {
            logger.LogError("Invalid token(s) for Id {UserSessionId}", userSessionId);
            return null;
        }
        return tokenResult;
    }

    private async Task<string?> StoreSessionData(string userSessionId, CdaTokenResponseModel tokenResult)
    {
        var userSessionData = await userSessionDataRepository.GetByIdAsync(userSessionId, userSessionId);
        if (userSessionData == null) return null;

        var clientId = userSessionData.ClientId;
        var peisId = userSessionData.PeisId;
        if (string.IsNullOrEmpty(peisId))
        {
            logger.LogError("Invalid peisId for UserSessionData Id {UserSessionId}", userSessionId);
            return null;
        }

        await userSessionDataRepository.InsertItemAsync(new UserSessionData
        {
            Id = userSessionId,
            UserSessionId = userSessionId,
            AccessToken = tokenResult.AccessToken!,
            PeisId = peisId,
            Pct = tokenResult.Pct,
            ClientId = clientId
        }, userSessionId);

        return peisId;
    }

    private async Task PostPensionDataRetrievalMessage(string peisId, RequestHeaderModel requestHeader, string userSessionId)
    {
        var message = CreateRequestPayload(peisId, requestHeader);
        logger.LogInformation("Posting message to initiate Pensions Data retrieval for UserSessionId {UserSessionId}", userSessionId);
        await _messagingService.SendMessageAsync(message, _serviceBusOptions.Value.OutboundQueue!, requestHeader.CorrelationId);
    }

    private static bool IsValidToken(string? token) => !string.IsNullOrEmpty(token);

    private ObjectResult InternalServerErrorResult() => StatusCode(500, "Internal server error");
}