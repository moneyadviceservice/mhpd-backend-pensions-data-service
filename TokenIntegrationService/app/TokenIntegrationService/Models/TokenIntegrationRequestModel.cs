//using DocumentFormat.OpenXml.Drawing;
//using DocumentFormat.OpenXml.Drawing.Charts;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.Net.Sockets;
using System.Runtime.Intrinsics.X86;

namespace TokenIntegrationService.Models
{
    public class TokenIntegrationRequestModel
    {               
        public string? Ticket { get; set; }      
        public string? Rqp { get; set; }   
        public string? AsUri{ get; set; }
    }
}
