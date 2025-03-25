using MhpdCommon.Constants;
using Microsoft.AspNetCore.Mvc;

namespace PeiIntegrationService.Models.PeiIntegrationServiceClient;

public class PeiIntegrationServiceRequestModel
{
    [FromHeader(Name = HeaderConstants.Rpt)]
    public string? Rpt { get; set; }

    [FromHeader(Name = HeaderConstants.Iss)]
    public string? Iss { get; set; }

    [FromHeader(Name = HeaderConstants.UserSessionId)]
    public string? UserSessionId { get; set; }

    [FromHeader(Name = HeaderConstants.PeisId)]
    public string? PeisId { get; set; }

    [FromHeader(Name = HeaderConstants.CorrelationId)]
    public string CorrelationId { get; set; } = string.Empty;
}
