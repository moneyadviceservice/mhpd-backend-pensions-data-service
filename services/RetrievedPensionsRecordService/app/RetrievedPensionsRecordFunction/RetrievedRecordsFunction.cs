using System.Net;
using MhpdCommon.Constants;
using MhpdCommon.Extensions;
using MhpdCommon.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using RetrievedPensionsRecordFunction.Models;
using RetrievedPensionsRecordFunction.Repository;

namespace RetrievedPensionsRecordFunction
{
    public class RetrievedRecordsFunction(ILogger<RetrievedRecordsFunction> logger, IPensionRecordRepository repository, IIdValidator validator)
    {
        [Function("GetRetrievedRecords")]
        [OpenApiOperation(operationId: "get-retrieved-pensions-records",
            Summary = "Get Retrieved Pension Records",
            Description = "Get the retrieved retrieved-pensions-records that contains pensions information has been retrieved from the PDP Ecosystem for peis.")]
        [OpenApiParameter(
            "pensionsRetrievalRecordId",
            In = ParameterLocation.Query, 
            Description = "The id of the pensions retrieval record that the retrieved pension record is associated with.",
            Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), 
            Description = "The array of Retrieved Pension Records that match the provided query parameters")]
        [OpenApiResponseWithoutBody(HttpStatusCode.BadRequest, Description = "BadRequest")]
        [OpenApiResponseWithoutBody(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [OpenApiResponseWithoutBody(HttpStatusCode.Forbidden, Description = "Forbidden")]
        [OpenApiResponseWithoutBody(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [OpenApiResponseWithoutBody(HttpStatusCode.BadGateway, Description = "BadGateway")]
        [OpenApiResponseWithoutBody(HttpStatusCode.ServiceUnavailable, Description = "Service Unavailable")]
        [OpenApiResponseWithoutBody(HttpStatusCode.GatewayTimeout, Description = "Gateway Timeout")]
        public async Task<IActionResult> GetAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "retrieved-pension-records")] HttpRequest req)
        {
            return await ProcessRetrievedRecordsAsync(req, repository.GetRetrievedRecordsAsync);
        }

        [Function("DeleteRetrievedRecords")]
        [OpenApiOperation(operationId: "delete-pensions-retrieved-records-id",
            Summary = "Delete Pensions Retrieved Record",
            Description = "Deletes the given pension retrieved record id.")]
        [OpenApiParameter(
            "id",
            In = ParameterLocation.Path, 
            Description = "The id of pension retrieved record to be deleted",
            Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.NoContent, Description = "No Content")]
        [OpenApiResponseWithoutBody(HttpStatusCode.NotFound, Description = "Not Found")]
        public async Task<IActionResult> DeleteAsync([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "retrieved-pension-records")] HttpRequest req)
        {
            return await ProcessRetrievedRecordsAsync(req, repository.DeleteRetrievedRecordsAsync);
        }

        private async Task<IActionResult> ProcessRetrievedRecordsAsync<T>(HttpRequest req, Func<string, Task<T>> processor)
        {
            var correlationId = req.Headers[HeaderConstants.CorrelationId].ToString();

            if (string.IsNullOrEmpty(correlationId))
            {
                correlationId = Guid.NewGuid().ToString();
            }

            if (!validator.IsValidGuid(correlationId))
            {
                return new BadRequestObjectResult(Constants.InvalidCorrelationId);
            }

            using var scope = logger.BeginCorrelationScope(correlationId, Constants.HttpLogSource);

            var pensionsRetrievalRecordId = req.Query[Constants.RetrievedRecordQuery].ToString();

            logger.LogRequest($"Pension retrieval record Id: {pensionsRetrievalRecordId}");

            if (!validator.IsValidGuid(pensionsRetrievalRecordId))
            {
                logger.LogError("Unable to service request for pensionsRetrievalRecordId [{retrievalId}]: {reason}", pensionsRetrievalRecordId, Constants.InvalidRecordId);
                return new BadRequestObjectResult(Constants.InvalidRecordId);
            }

            var records = await processor(pensionsRetrievalRecordId);

            logger.LogResponse(records);

            return new OkObjectResult(records);
        }
    }
}
