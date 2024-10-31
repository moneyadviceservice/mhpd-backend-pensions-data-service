using MhpdCommon.Constants;
using MhpdCommon.Extensions;
using MhpdCommon.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using RetrievedPensionsRecordFunction.Models;
using RetrievedPensionsRecordFunction.Repository;

namespace RetrievedPensionsRecordFunction
{
    public class RetrievedRecordsFunction(ILogger<RetrievedRecordsFunction> logger, IPensionRecordRepository repository, IIdValidator validator)
    {
        private readonly ILogger<RetrievedRecordsFunction> _logger = logger;
        private readonly IPensionRecordRepository _repository = repository;
        private readonly IIdValidator _idValidator = validator;

        [Function("RetrievedRecordsFunction")]
        public async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "retrieved-pension-records")] HttpRequest req)
        {
            var correlationId = req.Headers[HeaderConstants.CorrelationId].ToString();

            if (string.IsNullOrEmpty(correlationId))
            {
                correlationId = Guid.NewGuid().ToString();
            }

            if (!_idValidator.IsValidGuid(correlationId))
            {
                return new BadRequestObjectResult(Constants.InvalidCorrelationId);
            }

            using var scope = _logger.BeginCorrelationScope(correlationId, Constants.HttpLogSource);

            var pensionsRetrievalRecordId = req.Query[Constants.RetrievedRecordQuery].ToString();

            _logger.LogRequest($"Pension retrieval record Id: {pensionsRetrievalRecordId}");

            if (!_idValidator.IsValidGuid(pensionsRetrievalRecordId))
            {
                _logger.LogError("Unable to service request for pensionsRetrievalRecordId [{retrievalId}]: {reason}", pensionsRetrievalRecordId, Constants.InvalidRecordId);
                return new BadRequestObjectResult(Constants.InvalidRecordId);
            }

            var records = await _repository.GetRetrievedRecordsAsync(pensionsRetrievalRecordId);

            _logger.LogResponse(records);

            return new OkObjectResult(records);
        }
    }
}
