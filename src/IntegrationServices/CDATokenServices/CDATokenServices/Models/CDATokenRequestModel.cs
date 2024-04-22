using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Drawing.Charts;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.Net.Sockets;
using System.Runtime.Intrinsics.X86;

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
