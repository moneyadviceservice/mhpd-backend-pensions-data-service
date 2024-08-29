using Microsoft.Extensions.FileSystemGlobbing.Internal;
using System.Text.RegularExpressions;

namespace TokenIntegrationService.Models
{
    public class TokenIntegrationRequestModel
    { 
        public string? Rqp { get; set; }

        public string? Ticket { get; set; }    

        public string? As_Uri{ get; set; }

        public bool Validate()
        {            
            return (string.IsNullOrEmpty(Rqp) || Regex.IsMatch(Rqp, "[@¬`!-#£$%^&*()<>?/|+=}{~:;]") || string.IsNullOrEmpty(Ticket) || Regex.IsMatch(Ticket, "[@¬`!-#£$%^&*()<>?/|+=}{~:;]") || string.IsNullOrEmpty(As_Uri) || Regex.IsMatch(As_Uri, "[@¬`!#£$%^&*()<>?|+=}{~;]")) ? false : true;
        }
    }
}