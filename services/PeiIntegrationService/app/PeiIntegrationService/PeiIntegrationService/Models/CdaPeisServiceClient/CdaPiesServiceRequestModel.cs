namespace PeiIntegrationService.Models.CdaPeisServiceClient
{
    public class CdaPiesServiceRequestModel
    {
        public string? Rpt { get; set; }

        public string? PeisId { get; set; }

        public string CorrelationId { get; set; } = string.Empty;
    }
}
