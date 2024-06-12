namespace PeiIntegrationService.Models.CdaPiesService
{
    public class CdaPiesServiceRequestModel
    {
        public string? Rpt { get; set; }

        public string? PeisId { get; set; }

        public string? RequestId { get; set; }

        public bool Validate()
        {
            Guid.TryParse(PeisId, out var peisIdGuid);
            if (peisIdGuid == Guid.Empty || PeisId!.ToString().Length != 36)
            {
                return false;
            }
            
            Guid.TryParse(RequestId, out var requestIdGuid);
            if (requestIdGuid == Guid.Empty || RequestId!.ToString().Length != 36)
            {
                return false;
            }

            return true;
        }
   }
}
