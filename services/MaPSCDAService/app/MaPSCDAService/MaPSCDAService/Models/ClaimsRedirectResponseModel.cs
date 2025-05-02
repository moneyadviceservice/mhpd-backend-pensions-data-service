using MhpdCommon.Constants;
using System.ComponentModel.DataAnnotations;

namespace MaPSCDAService.Models;

public class ClaimsRedirectResponseModel
{
    [RegularExpression(ApiConstants.UrlPattern)]
    public string ClaimsRedirectUrl { get; set; } = string.Empty;

    [RegularExpression(ApiConstants.JwtPattern)]
    public string Rqp { get; set; } = string.Empty;

    [RegularExpression(ApiConstants.JwePattern)]
    public string Ticket { get; set; } = string.Empty;

    [RegularExpression(ApiConstants.GuidPattern)]
    public string RequestId { get; set; } = string.Empty;
}
