using MhpdCommon.Constants;
using MhpdCommon.Constants.HttpClient;
using MhpdCommon.Models.MHPDModels;
using MhpdCommon.SharedHttpClient;
using MhpdCommon.Utils;
using Microsoft.AspNetCore.Mvc;

namespace PensionsDataService.Controllers;

[Route(StatusConstants.ServiceRoute)]
[ApiController]
public class StatusController(IServiceStatusProvider statusProvider, IServiceStatusClient statusClient) : ControllerBase
{
    [HttpGet]
    [Route(StatusConstants.Endpoint)]
    public async Task<IActionResult> Status()
    {
        var servicesToMonitor = new[]
        {
            HttpClientNames.MapsCdaService,
            HttpClientNames.PensionRetrievalService,
            HttpClientNames.RetrievedPensionsService,
            HttpClientNames.ViewDataIntegrationService,
            HttpClientNames.TokenIntegrationService,
            HttpClientNames.PeiIntegrationService,
        };

        var status = statusProvider.GetServiceStatus();

        var serviceStatuses = new List<ServiceStatus> { status };

        var statusTasks = servicesToMonitor.Select(service => statusClient.GetServiceStatusAsync(service));
        ServiceStatus[] downstreamStatusList = await Task.WhenAll(statusTasks);

        serviceStatuses.AddRange(downstreamStatusList);


        return Ok(serviceStatuses);
    }
}
