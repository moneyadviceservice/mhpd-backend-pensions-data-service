using MhpdCommon.Constants.HttpClient;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace MhpdCommon.Models.MessageBodyModels;

public class CdaTokenRequestModel
{
    [FromForm(Name = QueryParams.Cda.Token.GrantType)]
    public string? GrantType { get; set; }

    [FromForm(Name = QueryParams.Cda.Token.Ticket)]
    public string? Ticket { get; set; }

    [FromForm(Name = QueryParams.Cda.Token.ClaimToken)]
    public string? ClaimToken { get; set; }

    [FromForm(Name = QueryParams.Cda.Token.ClaimTokenFormat)]
    public string? ClaimTokenFormat { get; set; }

    [FromForm(Name = QueryParams.Cda.Token.Scope)]
    public string? Scope { get; set; }

    [FromForm(Name = QueryParams.Cda.Token.ClientId)]
    public string? ClientId { get; set; }

    [FromForm(Name = QueryParams.Cda.Token.ClientSecret)]
    public string? ClientSecret { get; set; }

    [FromForm(Name = QueryParams.Cda.Token.Code)]
    public string? Code { get; set; }

    [FromForm(Name = QueryParams.Cda.Token.RedirectUri)]
    public string? RedirectUrl { get; set; }

    [FromForm(Name = QueryParams.Cda.Token.CodeVerifier)]
    public string? CodeVerifier { get; set; }
    
    [FromForm(Name = QueryParams.Cda.Token.Pct)]
    public string? Pct { get; set; }
}