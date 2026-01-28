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
using PensionsDataService.Extensions;
using PensionsDataService.HttpClients;
using PensionsDataService.Models;
using PensionsDataService.Utilities;
using System.Text.Json;
using System.Text.Json.Nodes;
using static MhpdCommon.ViewData.EvaluationConstants;

namespace PensionsDataService.Controllers;

[Route("/")]   
[ApiController]
public class PensionsDataController(
    ILogger<PensionsDataController> logger,
    IIdValidator idValidator,
    PensionsDataRequestValidatorPipeline requestValidators,
    PensionServiceClients serviceClients,
    IOptions<PeiOrchestrationSettings> peiRetrievalOptions,
    ICosmosDbRepository<UserSessionData> userSessionDataRepository,
    PensionServiceUtilities serviceUtilities)
    : ControllerBase
{
    private readonly ITokenIntegrationServiceClient _tokenIntegrationServiceClientRpts = serviceClients.TokenIntegrationServiceClient;
    private readonly IMapsCdaServiceClient _mapsCdaServiceClient = serviceClients.MapsCdaServiceClient;
    private readonly IRetrievalRecordServiceClient _retrievalRecordServiceClient = serviceClients.RetrievalRecordServiceClient;
    private readonly IRetrievedPensionsRecordClient _retrievedPensionsRecordClient = serviceClients.RetrievedPensionsRecordClient;
    private readonly IOptions<CommonServiceBusConfiguration> _serviceBusOptions = serviceClients.ServiceBusOptions;
    private readonly IMessagingService _messagingService = serviceClients.MessagingService;
    private readonly int _predictedTotalDataRetrievalTime = peiRetrievalOptions.Value.TotalPensionRetrievalDuration;
    private const string ErrorMessage = "Error: {ErrorMessage}";
    private const string AsUri = "https://example.com"; //NOSONAR

    private static readonly IEnumerable<string> ExcludedCategories =
    [
        Category.Error,
        Category.Unsupported
    ];

    [HttpGet]
    [Route("pensions-status")]
    public async Task<IActionResult> GetPensionsStatusAsync([FromHeader(Name = HeaderConstants.UserSessionId)] string? userSessionId,
        [FromHeader(Name = HeaderConstants.CorrelationId)] string? correlationId)
    {
        var (requestedPension, earlyResponse) = await TryGetRequestedPensionAsync(
        $"{Constants.LogSource} Status - GET", userSessionId, correlationId);

        if (earlyResponse != null)
        {
            return earlyResponse;
        }

        var retrievedPeis = await _retrievedPensionsRecordClient.GetRetrievedPeisAsync(new RequestHeaderModel
        {
            UserSessionId = userSessionId,
            CorrelationId = correlationId
        });

        var isComplete = IsPensionsDataRetrievalComplete(
            requestedPension.PeiRetrievalComplete,
            requestedPension.PeiData,
            retrievedPeis
        );

        var response = new PensionsStatusResponseModel
        {
            PensionsDataRetrievalComplete = isComplete,
            PredictedTotalDataRetrievalTime = _predictedTotalDataRetrievalTime,
            PredictedRemainingDataRetrievalTime = GetRemainingRetrievalTime(requestedPension, _predictedTotalDataRetrievalTime)
        };

        logger.LogResponseSent(response);

        return Ok(response);
    }

    [HttpGet]
    [Route("pensions-summary")]
    public async Task<IActionResult> GetPensionSummaryAsync([FromHeader(Name = HeaderConstants.UserSessionId)] string? userSessionId,
        [FromHeader(Name = HeaderConstants.CorrelationId)] string? correlationId)
    {
        var (requestedPension, earlyResponse) = await TryGetRequestedPensionAsync(
        $"{Constants.LogSource} Summary - GET", userSessionId, correlationId);

        if (earlyResponse != null)
        {
            return earlyResponse;
        }

        var retrievedPensions = await GetRetrievedPensionsAsync(userSessionId!, correlationId!);

        var isComplete = IsPensionsDataRetrievalComplete(
            requestedPension.PeiRetrievalComplete,
            requestedPension.PeiData,
            retrievedPensions.Select(r => r.Pei!)
        );

        var response = new PensionSummary
        {
            IsPensionRetrievalComplete = isComplete
        };

        foreach (var peiData in requestedPension.PeiData)
        {
            // Check if there is a matching pei in the retrievedPensionsRecords
            var matchingRecord = retrievedPensions.Find(record => record.Pei == peiData.Pei);

            if (!ExcludedCategories.Contains(matchingRecord?.Category))
            {
                response.TotalPensionsFound++;
            }

            response.Pensions.Add(new PensionItem
            {
                Pei = peiData.Pei,
                RetrievalStatus = GetPeiStatus(matchingRecord),
                SchemeName = !string.IsNullOrEmpty(matchingRecord?.SchemeName) ? matchingRecord.SchemeName : peiData.Description ?? Constants.Unknown,
                AdministratorName = matchingRecord?.Administrator ?? Constants.Unknown,
                HasIncome = bool.TryParse(matchingRecord?.HasIncome, out var hasIncome) && hasIncome,
                PensionType = matchingRecord?.PensionType ?? Constants.Unknown,
                Category = matchingRecord?.Category ?? Category.None
            });
        }

        logger.LogResponseSent(response);

        return Ok(response);
    }

    [HttpGet]
    [Route("pensions/{category}")]
    public async Task<IActionResult> GetPensionsByCategoryAsync([FromRoute] string category, [FromHeader(Name = HeaderConstants.UserSessionId)] string? userSessionId,
        [FromHeader(Name = HeaderConstants.CorrelationId)] string? correlationId)
    {
        var (requestedPension, earlyResponse) = await TryGetRequestedPensionAsync(
        $"{Constants.LogSource} By Category - GET", userSessionId, correlationId);

        if (earlyResponse != null)
        {
            return earlyResponse;
        }

        var retrievedPensions = await GetRetrievedPensionsAsync(userSessionId!, correlationId!);

        var isComplete = IsPensionsDataRetrievalComplete(
            requestedPension.PeiRetrievalComplete,
            requestedPension.PeiData,
            retrievedPensions.Select(r => r.Pei!)
        );

        var response = new PensionData
        {
            IsPensionRetrievalComplete = isComplete
        };

        var enrichedPensions = retrievedPensions.EnrichLinkedPensions().EnrichCardData(serviceUtilities.CardDataRuleEngine);

        foreach (var pension in enrichedPensions) 
        {
            if(category != Category.Contact &&
                pension.Category == Category.Contact)
            {
                ++response.TotalContactPensions;
            }

            if (pension.Category == category)
            {
                response.Arrangements.Add(pension.RetrievalResult);
            }
        }

        response.EnrichSummaryData(enrichedPensions, category, serviceUtilities.SummaryDataRuleEngine);

        logger.LogResponseSent(response);

        return Ok(response);
    }

    [HttpGet]
    [Route("pensions-timeline")]
    public async Task<IActionResult> GetPensionTimelineAsync([FromHeader(Name = HeaderConstants.UserSessionId)] string? userSessionId,
        [FromHeader(Name = HeaderConstants.CorrelationId)] string? correlationId)
    {
        var (requestedPension, earlyResponse) = await TryGetRequestedPensionAsync(
        $"{Constants.LogSource} Summary - GET", userSessionId, correlationId);

        if (earlyResponse != null)
        {
            return earlyResponse;
        }

        var retrievedPensions = await GetRetrievedPensionsAsync(userSessionId!, correlationId!);

        var isComplete = IsPensionsDataRetrievalComplete(
            requestedPension.PeiRetrievalComplete,
            requestedPension.PeiData,
            retrievedPensions.Select(r => r.Pei!)
        );

        var timeSeries = serviceUtilities.TimelineSeriesBuilder.Build(retrievedPensions.Where(pension => pension.Category == Category.Confirmed));

        timeSeries.IsPensionRetrievalComplete = isComplete;

        logger.LogResponseSent(timeSeries);

        return Ok(timeSeries);
    }

    [HttpGet]
    [Route("pension-detail/{id}")]
    public async Task<IActionResult> GetPensionDetailAsync([FromRoute] string id, [FromHeader(Name = HeaderConstants.UserSessionId)] string? userSessionId,
        [FromHeader(Name = HeaderConstants.CorrelationId)] string? correlationId)
    {
        var (_, earlyResponse) = await TryGetRequestedPensionAsync(
        $"{Constants.LogSource} Detail - GET", userSessionId, correlationId);

        if (earlyResponse != null)
        {
            return earlyResponse;
        }

        var retrievedPensions = await GetRetrievedPensionsAsync(userSessionId!, correlationId!, id);

        if (retrievedPensions.Count == 0)
        {
            return NotFound($"No pension found with id {id}");
        }

        var enrichedPensions = retrievedPensions.EnrichLinkedPensions();
        var pensionDetail = enrichedPensions.Where(pension => pension.AssetId == id).Select(pension =>
        {
            JsonNode node = JsonNode.Parse(pension.RetrievalResult!.GetRawText());
            var enrichedJson = node.EnrichDetailData(serviceUtilities.DetailDataRuleEngine).ToJsonString();

            return JsonDocument.Parse(enrichedJson).RootElement;
        });

        logger.LogResponseSent(pensionDetail);
        return Ok(pensionDetail);
    }

    [HttpGet]
    [Route("pensions/analytics")]
    public async Task<IActionResult> GetPensionsAnalyticsAsync([FromHeader(Name = HeaderConstants.UserSessionId)] string? userSessionId,
        [FromHeader(Name = HeaderConstants.CorrelationId)] string? correlationId)
    {
        var (requestedPension, earlyResponse) = await TryGetRequestedPensionAsync(
        $"{Constants.LogSource} Analytics - GET", userSessionId, correlationId);

        if (earlyResponse != null)
        {
            return earlyResponse;
        }

        var retrievedPensions = await GetRetrievedPensionsAsync(userSessionId!, correlationId!);

        var isComplete = IsPensionsDataRetrievalComplete(
            requestedPension.PeiRetrievalComplete,
            requestedPension.PeiData,
            retrievedPensions.Select(r => r.Pei!)
        );

        if(!isComplete)
        {
            logger.LogWarning("Pension analytics data requested before retrieval was complete for UserSessionId {UserSessionId}", userSessionId);
            return BadRequest("Pension data retrieval is not complete");
        }

        var response = new AnalyticsData();

        try
        {
            response.SplitPensionsByStatus(retrievedPensions, requestedPension.PeiData);
            var json = JsonSerializer.Serialize(response);
            var anonymizedJson = serviceUtilities.Anonymizer.Anonymize(json);
            response = JsonSerializer.Deserialize<AnalyticsData>(anonymizedJson);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error anonymizing pensions data for UserSessionId {UserSessionId}", userSessionId);
            response = null;
        }

        if (response is null)
        {
            return StatusCode(500, "Unable to collect pension retrieval analytics data");
        }

        logger.LogResponseSent(response);

        return Ok(response);
    }


    [HttpPost]
    [Route("pensions-data")]
    public async Task<IActionResult> PostPensionsDataAsync([FromBody] PensionsDataRequestModel request, [FromHeader] RequestHeaderModel requestHeader)
    {
        if (!TryValidateRequests(request, requestHeader, out var validationMessage))
        {
            logger.LogError(ErrorMessage, validationMessage);
            return BadRequest(validationMessage);
        }

        using var scope = logger.BeginCorrelationScope(requestHeader.CorrelationId!, $"{Constants.LogSource} POST");
        logger.LogRequestReceived(requestHeader);
        logger.LogRequestReceived(request);

        // Get pei from the token integration service
        var result = await _tokenIntegrationServiceClientRpts
            .PostIdTokenAsync(CreateCdaTokenServiceRequestModel(request), requestHeader.CorrelationId);

        var userSessionId = requestHeader.UserSessionId!;

        if (!IsValidUserSessionData(userSessionId, result.PeisId))
        {
            return StatusCode(500, "Internal server error");
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
        logger.LogResponseSent(response);
        
        return response;
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
        logger.LogRequestReceived(requestHeader);
        logger.LogRequestReceived(request);

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

        var response = Accepted(new
        {
            predictedTotalDataRetrievalTime = _predictedTotalDataRetrievalTime,
            pensionRetrievalStartTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        });

        logger.LogResponseSent(response);
        return response;
    }
    
    [HttpDelete]
    [Route("pensions-data")]
    public async Task<IActionResult> DeletePensionsDataAsync([FromHeader(Name = HeaderConstants.UserSessionId)] string? userSessionId,
        [FromHeader(Name = HeaderConstants.CorrelationId)] string? correlationId)
    {
        // Get the pensions retrieval record associated with the passed userSessionId
        var (_, earlyResponse) = await TryGetRequestedPensionAsync(
        $"{Constants.LogSource} - {Constants.HttpDelete}", userSessionId, correlationId);

        if (earlyResponse != null && earlyResponse is not JsonResult)
        {
            return earlyResponse;
        }

        var retrievalCount = await _retrievalRecordServiceClient.DeleteAsync(userSessionId!, correlationId!);
        var retrievedCount = await _retrievedPensionsRecordClient.DeleteAsync(userSessionId!, correlationId!);

        logger.LogWarning("Delete request removed {RetrievalCount} pension retrieval records and {RetrievedCount} retrieved pension records", retrievalCount, retrievedCount);

        if (await userSessionDataRepository.DeleteByIdAsync(userSessionId!, userSessionId!))
        {
            logger.LogInformation("User session data deleted for UserSessionId {UserSessionId}", userSessionId);
        }
        else
        {
            logger.LogWarning("Unable to delete session data for UserSessionId {UserSessionId}", userSessionId);
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
        string rqp, string? asUri, string clientId)
    {
        var request = new TokenClientRequestModel
        {
            Ticket = ticket,
            Rqp = rqp,
            AsUri = asUri,
            ClientId = clientId
        };

        return request;
    }

    private static PensionsDataRequestModel CreateCdaTokenServiceRequestModel(PensionsDataRequestModel request)
    {
        var requestModel = new PensionsDataRequestModel
        {
            ClientId = request.ClientId,
            ClientSecret = request.ClientSecret,
            AuthorisationCode = request.AuthorisationCode,
            RedirectUrl = request.RedirectUrl,
            CodeVerifier = request.CodeVerifier
        };

        return requestModel;
    }

    private static string GetPeiStatus(RetrievedPensionRecord? record)
    {
        if (record == null)
        {
            return PensionProviderConstants.RetrievalStatus.RetrievalRequested;
        }

        if (record.RetrievalResult is JsonElement { ValueKind: JsonValueKind.Object } result && result.TryGetProperty(PensionConstants.ErrorCode, out _))
        {
            return PensionProviderConstants.RetrievalStatus.RetrievalFailed;
        }

        return PensionProviderConstants.RetrievalStatus.RetrievalComplete;
    }

    private static bool IsPensionsDataRetrievalComplete(bool peiRetrievalComplete, List<PeiDataModel> peiData, IEnumerable<string> retrievedPeis)
    {
        // Return true if PeiRetrievalComplete is true and either no PeiData or all Peis have been retrieved
        if (peiRetrievalComplete)
        {
            return peiData.Count == 0 || peiData.TrueForAll(p => retrievedPeis.Contains(p.Pei));
        }

        // Return false if PensionsDataRetrievalComplete is false
        return false;
    }

    private async Task<(PensionsRetrievalRecord requestedPension, IActionResult? earlyResponse)>
    TryGetRequestedPensionAsync(string endpoint, string? userSessionId, string? correlationId)
    {
        var requestHeader = new RequestHeaderModel
        {
            UserSessionId = userSessionId,
            CorrelationId = correlationId
        };

        if (!TryValidateRequestHeader(requestHeader, out var validationMessage))
        {
            logger.LogError(ErrorMessage, validationMessage);
            return (null!, BadRequest(validationMessage));
        }

        using var scope = logger.BeginCorrelationScope(requestHeader.CorrelationId!, $"{Constants.LogSource} {endpoint}");
        logger.LogRequestReceived(requestHeader);

        // Get the pensions retrieval record associated with the passed userSessionId
        var retrievalRecordResult = await _retrievalRecordServiceClient.GetAsync(requestHeader);

        if (string.IsNullOrEmpty(retrievalRecordResult.Id))
        {
            // No session data found, return an empty response
            return (null!, new JsonResult(null) { StatusCode = StatusCodes.Status200OK });
        }

        return (retrievalRecordResult, null);
    }

    private async Task<List<RetrievedPensionRecord>> GetRetrievedPensionsAsync(string userSessionId, string correlationId, string? assetId = null)
    {
        var requestHeader = new RequestHeaderModel
        {
            UserSessionId = userSessionId,
            CorrelationId = correlationId
        };

        var pensions = await _retrievedPensionsRecordClient.GetRetrievedPensionsAsync(
            new RetrievedPensionsRequest
            {
                AssetId = assetId
            }, requestHeader);

        return pensions;
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
            CreateCdaTokenServiceRptsRequestModel(request.Ticket, rqp, AsUri, request.ClientId), requestHeader.CorrelationId);

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
        logger.LogWarning("Posting message to initiate Pensions Data retrieval for UserSessionId {UserSessionId}", userSessionId);
        await _messagingService.SendMessageAsync(message, _serviceBusOptions.Value.OutboundQueue!, requestHeader.CorrelationId);
    }

    private static bool IsValidToken(string? token) => !string.IsNullOrEmpty(token);

    private ObjectResult InternalServerErrorResult() => StatusCode(500, "Internal server error");
}