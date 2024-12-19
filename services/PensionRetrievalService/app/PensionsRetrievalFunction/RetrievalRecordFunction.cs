using MhpdCommon.Constants;
using MhpdCommon.Extensions;
using MhpdCommon.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using PensionsRetrievalFunction.Models;
using PensionsRetrievalFunction.Repository;

namespace PensionsRetrievalFunction;

public class RetrievalRecordFunction(ILogger<RetrievalRecordFunction> logger, IPensionRetrievalRepository repository, IIdValidator validator)
{
    private readonly ILogger<RetrievalRecordFunction> _logger = logger;
    private readonly IPensionRetrievalRepository _repository = repository;
    private readonly IIdValidator _idValidator = validator;

    [Function("GetRetrievalRecords")]
    public async Task<IActionResult> GetAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "pensions-retrieval-records")] HttpRequest req)
    {
        return await ProcessRetrievalRecords(req, _repository.GetRetrievalRecordAsync);
    }

    [Function("DeleteRetrievalRecords")]
    public async Task<IActionResult> DeleteAsync([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "pensions-retrieval-records")] HttpRequest req)
    {
        return await ProcessRetrievalRecords(req, _repository.DeleteRetrievalRecordsAsync);
    }

    private async Task<IActionResult> ProcessRetrievalRecords<T>(HttpRequest req, Func<string, Task<T>> processor)
    {
        var correlationId = req.Headers[HeaderConstants.CorrelationId].ToString();

        if (string.IsNullOrEmpty(correlationId))
        {
            correlationId = Guid.NewGuid().ToString();
        }

        if (!_idValidator.IsValidGuid(correlationId))
        {
            return new BadRequestObjectResult(Constants.ResponseType.InvalidCorrelationId);
        }

        var userSessionId = req.Headers[HeaderConstants.UserSessionId].ToString();

        using var scope = _logger.BeginCorrelationScope(correlationId, Constants.LogSource.Http);

        _logger.LogRequest($"User session Id: {userSessionId}");

        if (!_idValidator.IsValidGuid(userSessionId))
        {
            _logger.LogError("Unable to service request for sessionId [{sessionId}]: {reason}", userSessionId, Constants.ResponseType.InvalidSessionId);
            return new BadRequestObjectResult(Constants.ResponseType.InvalidSessionId);
        }

        var record = await processor(userSessionId);
        _logger.LogResponse(record);

        return EqualityComparer<T>.Default.Equals(record, default) ? new OkResult() : new OkObjectResult(record);
    }
}
