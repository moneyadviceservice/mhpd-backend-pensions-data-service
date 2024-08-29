using Microsoft.AspNetCore.Mvc;

namespace CDATokenServices.Models
{
    public class CDATokenRequestModel
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
    }    
}
