using MhpdCommon.Constants.HttpClient;
using Microsoft.AspNetCore.Mvc;

namespace MhpdCommon.Models.MessageBodyModels;

public class CdaTokenRequestModel
{
    [FromQuery(Name = QueryParams.GrantType)]
    public string? GrantType { get; set; }
    
    [FromQuery(Name = QueryParams.Ticket)]
    public string? Ticket { get; set; }
    
    [FromQuery(Name = QueryParams.ClaimToken)]
    public string? ClaimToken { get; set; }
    
    [FromQuery(Name = QueryParams.ClaimTokenFormat)]
    public string? ClaimTokenFormat { get; set; }
    
    [FromQuery(Name = QueryParams.Scope)]
    public string? Scope { get; set; }
    
    [FromQuery(Name = QueryParams.ClientId)]
    public string? ClientId { get; set; }
    
    [FromQuery(Name = QueryParams.ClientSecret)]
    public string? ClientSecret { get; set; }
    
    [FromQuery(Name = QueryParams.Code)]
    public string? Code { get; set; }
    
    [FromQuery(Name = QueryParams.RedirectUri)]
    public string? RedirectUri { get; set; }
    
    [FromQuery(Name = QueryParams.CodeVerifier)]
    public string? CodeVerifier { get; set; }
}