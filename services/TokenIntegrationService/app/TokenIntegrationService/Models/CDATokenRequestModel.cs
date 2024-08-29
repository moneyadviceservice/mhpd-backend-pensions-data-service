namespace TokenIntegrationService.Models
{
    public class CDATokenRequestModel
    {       
        public string? GrantType { get; set; }      
        
        public string? Ticket { get; set; }
        
        public string? ClaimToken { get; set; }       
        
        public string? ClaimTokenFormat { get; set; }
        
        public string? Scope { get; set; }
        
        public string? RequestId { get; set; }
        
        public string? Rqp { get; set; }
        
        public string? CdaTokenUrl { get; set; }       
    }
}
