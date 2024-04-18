namespace PeiIntegrationService.Models
{
    public class CDAServiceRequestModel
    {
        public string? CdaUserGuid { get; set; }
        
        public string? Issuer { get; set; }
        
        public string? UserSessionId { get; set; }

        public string? Authorization { get; set; }

        public string? CdaServiceUrl { get; set; }

        public string? RequestId { get; set; }

        public required string Scope { get; set; }

        public bool Validate()
        {
            if (string.IsNullOrEmpty(CdaUserGuid) || string.IsNullOrEmpty(Issuer) || string.IsNullOrEmpty(UserSessionId)) return false;

            return true;
        }
    }
}
