using MhpdCommon.Constants.HttpClient;
using Microsoft.AspNetCore.Mvc;

namespace MhpdCommon.Models.MessageBodyModels;

public class CdaTokenRequestModel
{
    [FromQuery(Name = QueryParams.Cda.Token.GrantType)]
    public string? GrantType { get; set; }
    
    [FromQuery(Name = QueryParams.Cda.Token.Ticket)]
    public string? Ticket { get; set; }
    
    [FromQuery(Name = QueryParams.Cda.Token.ClaimToken)]
    public string? ClaimToken { get; set; }
    
    [FromQuery(Name = QueryParams.Cda.Token.ClaimTokenFormat)]
    public string? ClaimTokenFormat { get; set; }
    
    [FromQuery(Name = QueryParams.Cda.Token.Scope)]
    public string? Scope { get; set; }
    
    [FromQuery(Name = QueryParams.Cda.Token.ClientId)]
    public string? ClientId { get; set; }
    
    [FromQuery(Name = QueryParams.Cda.Token.ClientSecret)]
    public string? ClientSecret { get; set; }
    
    [FromQuery(Name = QueryParams.Cda.Token.Code)]
    public string? Code { get; set; }
    
    [FromQuery(Name = QueryParams.Cda.Token.RedirectUri)]
    public string? RedirectUri { get; set; }
    
    [FromQuery(Name = QueryParams.Cda.Token.CodeVerifier)]
    public string? CodeVerifier { get; set; }
}