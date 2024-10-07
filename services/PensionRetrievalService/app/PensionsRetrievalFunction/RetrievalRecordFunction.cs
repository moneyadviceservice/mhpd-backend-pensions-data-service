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

    [Function("RetrievalRecordFunction")]
    public async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "pensions-retrieval-records")] HttpRequest req)
    {
        var userSessionId = req.Headers[HeaderConstants.UserSessionId].ToString();

        _logger.LogRequest($"User session Id: {userSessionId}");

        if (!_idValidator.IsValidGuid(userSessionId))
        {
            _logger.LogError("Unable to service request for sessionId [{sessionId}]: {reason}", userSessionId, Constants.ResponseType.InvalidSessionId);
            return new BadRequestObjectResult(Constants.ResponseType.InvalidSessionId);
        }

        var record = await _repository.GetRetrievalRecordAsync(userSessionId);

        _logger.LogResponse(record);

        return record != null ? new OkObjectResult(record) : new OkResult();
    }
}
