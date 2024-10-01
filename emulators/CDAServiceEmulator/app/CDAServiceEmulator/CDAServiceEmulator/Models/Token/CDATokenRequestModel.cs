using Microsoft.AspNetCore.Mvc;

namespace CDAServiceEmulator.Models.Token;

public class CdaTokenRequestModel
{
    [FromQuery(Name = "grant_type")]
    public string? GrantType { get; set; }
    
    [FromQuery(Name = "ticket")]
    public string? Ticket { get; set; }
    
    [FromQuery(Name = "claim_token")]
    public string? ClaimToken { get; set; }
    
    [FromQuery(Name = "claim_token_format")]
    public string? ClaimTokenFormat { get; set; }
    
    [FromQuery(Name = "scope")]
    public string? Scope { get; set; }
    
    [FromQuery(Name = "client_id")]
    public string? ClientId { get; set; }
    
    [FromQuery(Name = "client_secret")]
    public string? ClientSecret { get; set; }
    
    [FromQuery(Name = "code")]
    public string? Code { get; set; }
    
    [FromQuery(Name = "redirect_uri")]
    public string? RedirectUri { get; set; }
    
    [FromQuery(Name = "code_verifier")]
    public string? CodeVerifier { get; set; }
}