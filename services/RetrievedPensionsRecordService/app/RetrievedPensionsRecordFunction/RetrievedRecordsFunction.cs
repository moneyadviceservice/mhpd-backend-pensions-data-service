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

        [Function("GetRetrievedRecords")]
        public async Task<IActionResult> GetAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "retrieved-pension-records")] HttpRequest req)
        {
            return await ProcessRetrievedRecordsAsync(req, _repository.GetRetrievedRecordsAsync);
        }

        [Function("DeleteRetrievedRecords")]
        public async Task<IActionResult> DeleteAsync([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "retrieved-pension-records")] HttpRequest req)
        {
            return await ProcessRetrievedRecordsAsync(req, _repository.DeleteRetrievedRecordsAsync);
        }

        private async Task<IActionResult> ProcessRetrievedRecordsAsync<T>(HttpRequest req, Func<string, Task<T>> processor)
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

            var records = await processor(pensionsRetrievalRecordId);

            _logger.LogResponse(records);

            return new OkObjectResult(records);
        }
    }
}
