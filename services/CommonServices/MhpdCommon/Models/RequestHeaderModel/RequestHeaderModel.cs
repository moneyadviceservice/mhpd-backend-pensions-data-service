using MhpdCommon.Constants;
using Microsoft.AspNetCore.Mvc;

namespace MhpdCommon.Models.RequestHeaderModel;

public class RequestHeaderModel
{
    [FromHeader(Name = HeaderConstants.RequestId)]
    public string? XRequestId { get; set; }
    
    [FromHeader(Name = HeaderConstants.UserSessionId)]
    public string? UserSessionId { get; set; }
    
    [FromHeader(Name = HeaderConstants.Iss)]
    public string? Iss { get; set; }
}