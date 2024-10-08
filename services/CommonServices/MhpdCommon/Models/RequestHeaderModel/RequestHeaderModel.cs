using MhpdCommon.Constants;
using Microsoft.AspNetCore.Mvc;

namespace MhpdCommon.Models.RequestHeaderModel;

public class RequestHeaderModel
{
    [FromHeader(Name = HeaderConstants.RequestId)]
    public string? XRequestId { get; set; }
}