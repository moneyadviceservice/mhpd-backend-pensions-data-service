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
        if (!TryValidateRequestHeader(requestHeader, out var validationMessage))
        {
            logger.LogError("Error: {ErrorMessage}", validationMessage);
            return await Task.FromResult<IActionResult>(BadRequest(validationMessage));
        }

        using var scope = logger.BeginCorrelationScope(requestHeader.CorrelationId!, $"{Constants.LogSource} GET");
        logger.LogRequest(requestHeader);

        // Get the pensions retrieval record associated with the passed userSessionId
        var retrievalRecordResult = await _retrievalRecordServiceClient.GetAsync(requestHeader);
        logger.LogResponse(retrievalRecordResult);

        var response = new PensionsDataResponseModel
        {
            PensionPolicies = null,
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
        if (!TryValidateRequests(request, requestHeader, out var validationMessage))
        {
            logger.LogError("Error: {ErrorMessage}", validationMessage);
            return await Task.FromResult<IActionResult>(BadRequest(validationMessage));
        }

        using var scope = logger.BeginCorrelationScope(requestHeader.CorrelationId!, $"{Constants.LogSource} POST");
        logger.LogRequest(requestHeader);
        logger.LogRequest(request);

        // Get pei from the token integration service
        var result = await _tokenIntegrationServiceClient
            .PostAsync(CreateCdaTokenServiceRequestModel(request, logger), requestHeader);
        
        // Post a message to initiate a process to retrieve the Pensions Data for the userSessionId
        var message = CreateRequestPayload(result, requestHeader);
        logger.LogInformation("Post a message to initiate a process to retrieve the Pensions Data for the userSessionId {UserSessionId}",
            message.UserSessionId);
        await messagingService.SendMessageAsync(message, serviceBusOptions.Value.OutboundQueue!, requestHeader.CorrelationId);

        var response = StatusCode((int)HttpStatusCode.NoContent);
        logger.LogResponse(response);
        
        return await Task.FromResult<IActionResult>(response);
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

    private static PensionRetrievalPayload CreateRequestPayload(PeiRetrievalDetailsResponseModel response, RequestHeaderModel requestHeader)
    {
        return new PensionRetrievalPayload
        {
            Iss = requestHeader.Iss,
            PeisId = response.PeisId,
            UserSessionId = requestHeader.UserSessionId
        };
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

            // If there is a matching record, set status to RETRIEVAL_COMPLETE. Otherwise, retain the existing retrievalStatus
            peiData.RetrievalStatus = matchingRecord != null ? RetrievalStatusConstants.RetrievalComplete : peiData.RetrievalStatus;
        }

        return peiDataList;
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
}