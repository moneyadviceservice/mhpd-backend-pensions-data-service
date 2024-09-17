using Microsoft.AspNetCore.Mvc;

namespace CDAServiceEmulator.Models.Peis;

public class RequestHeaderModel
{
    [FromHeader(Name = "X-Request-ID")]
    public string? XRequestId { get; set; }
}