using MhpdCommon.Constants;
using MhpdCommon.Constants.HttpClient;
using MhpdCommon.CustomExceptions;
using MhpdCommon.Models.RequestHeaderModel;
using Microsoft.AspNetCore.Mvc;

namespace PensionsDataService.HttpClients;

public class RetrievalRecordFunctionClient : IRetrievalRecordFunctionClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<RetrievalRecordFunctionClient> _logger;
    
    public RetrievalRecordFunctionClient(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<RetrievalRecordFunctionClient> logger)
    {
        _logger = logger;
        
        _httpClient = httpClientFactory.CreateClient(HttpClientNames.RetrievalRecordFunction);
        
        // Set the base address for the client
        var endpoint = configuration[HttpClientUrlVariables.RetrievalRecordFunctionUrl];
        if (string.IsNullOrEmpty(endpoint))
        {
            throw new InvalidOperationException("RetrievalRecordFunctionClient endpoint is not configured.");
        }
        _httpClient.BaseAddress = new Uri(endpoint);
    }
    
    public async Task<IActionResult> GetAsync(RequestHeaderModel requestHeader)
    {
        try
        {
            // Add request ID header
            _httpClient.DefaultRequestHeaders.Add(HeaderConstants.UserSessionId, requestHeader.UserSessionId);
            
            // Send the request to the constructed endpoint
            var response = await _httpClient.GetAsync(HttpEndpoints.PensionsRetrievalRecords);

            // Check if the response is successful
            response.EnsureSuccessStatusCode();
            
            return new OkResult();
        }
        catch (HttpRequestException ex)
        {
            // Log the error and throw a more specific exception
            _logger.LogError(ex, "An HTTP request error occurred while calling the retrieval record endpoint");
            throw new ServiceCommunicationException("Error communicating with retrieval record endpoint", ex);
        }
        catch (InvalidOperationException ex)
        {
            // Log and handle specific invalid operation errors with contextual information
            _logger.LogError(ex, "Invalid operation: {Message}", ex.Message);
            throw new InvalidOperationException("An invalid operation occurred during retrieval record function communication", ex);
        }
        catch (Exception ex)
        {
            // Log any other exceptions with context, but do not throw a generic Exception
            _logger.LogError(ex, "An unexpected error occurred in get pensions-retrieval-records");
            throw new ServiceCommunicationException("An unexpected error occurred during retrieval record function communication", ex);
        }
        
    }
}