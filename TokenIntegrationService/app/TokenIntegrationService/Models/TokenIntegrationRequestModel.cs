namespace TokenIntegrationService.Models
{
    public class TokenIntegrationRequestModel
    {
        public string? Rqp { get; set; }

        public string? Ticket { get; set; }    

        public string? As_Uri{ get; set; }

        public bool Validate()
        {
            return (string.IsNullOrEmpty(Rqp) || string.IsNullOrEmpty(Ticket) || string.IsNullOrEmpty(As_Uri)) ? false : true;
         
        }
    }
}