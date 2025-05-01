using MhpdCommon.Constants;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace MhpdCommon.Models.RequestHeaderModel;

public class RequestHeaderModel
{
    [Required]
    [RegularExpression(ApiConstants.GuidPattern)]
    [FromHeader(Name = HeaderConstants.UserSessionId)]
    public string? UserSessionId { get; set; }

    [Required]
    [MinLength(1)]
    [MaxLength(200)]
    [FromHeader(Name = HeaderConstants.Iss)]
    public string? Iss { get; set; }

    [StringLength(36)]
    [RegularExpression(ApiConstants.GuidPattern)]
    [FromHeader(Name = HeaderConstants.CorrelationId)]
    public string? CorrelationId { get; set; }
}