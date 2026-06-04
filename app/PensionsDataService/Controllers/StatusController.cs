using MhpdCommon.Constants.HttpClient;
using MhpdCommon.Models.MHPDModels;
using MhpdCommon.SharedHttpClient;
using MhpdCommon.Utils;
using Microsoft.AspNetCore.Mvc;

namespace PensionsDataService.Controllers;

public class StatusController(IServiceStatusProvider statusProvider, IServiceStatusClient statusClient) : ControllerBase
{
    public async Task<IActionResult> Status()
    {
        var servicesToMonitor = new[]
        {
            HttpClientNames.PensionRetrievalService
        };

        var status = statusProvider.GetServiceStatus();

        var serviceStatuses = new List<ServiceStatus> { status };

        var statusTasks = servicesToMonitor.Select(service => statusClient.GetServiceStatusAsync(service));
        ServiceStatus[] downstreamStatusList = await Task.WhenAll(statusTasks);

        serviceStatuses.AddRange(downstreamStatusList);


        return Ok(serviceStatuses);
    }
}
