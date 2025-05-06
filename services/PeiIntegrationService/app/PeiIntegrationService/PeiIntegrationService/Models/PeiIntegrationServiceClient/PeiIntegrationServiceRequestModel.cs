using MhpdCommon.Constants;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace PeiIntegrationService.Models.PeiIntegrationServiceClient;

public class PeiIntegrationServiceRequestModel
{
    [RegularExpression(ApiConstants.JwtPattern)]
    [FromHeader(Name = HeaderConstants.Rpt)]
    public string? Rpt { get; set; }

    [Required]
    [MinLength(1)]
    [FromHeader(Name = HeaderConstants.Iss)]
    public string Iss { get; set; } = string.Empty;

    [Required]
    [RegularExpression(ApiConstants.GuidPattern)]
    [FromHeader(Name = HeaderConstants.UserSessionId)]
    public string UserSessionId { get; set; } = string.Empty;

    [Required]
    [MinLength(1)]
    [FromHeader(Name = HeaderConstants.PeisId)]
    public string PeisId { get; set; } = string.Empty;

    [RegularExpression(ApiConstants.GuidPattern)]
    [FromHeader(Name = HeaderConstants.CorrelationId)]
    public string CorrelationId { get; set; } = string.Empty;
}
