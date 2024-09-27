using Microsoft.AspNetCore.Mvc;

namespace PeiIntegrationService.Models.PeiIntegrationService;

public class PeiIntegrationServiceRequestModel
{
    [FromHeader(Name = "rpt")]
    public string? Rpt { get; set; }

    [FromHeader(Name = "iss")]
    public string? Iss { get; set; }

    [FromHeader(Name = "userSessionId")]
    public string? UserSessionId { get; set; }

    [FromHeader(Name = "peisId")]
    public string? PeisId { get; set; }
}
