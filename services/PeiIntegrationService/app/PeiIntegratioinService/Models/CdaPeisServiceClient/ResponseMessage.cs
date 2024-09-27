using System.Net;

namespace PeiIntegrationService.Models.CdaPiesService
{
    public class ResponseMessage
    {
        public HttpStatusCode ResponseStatusCode { get; set; }

        public string? WWWAuthenticateResponseHeader { get; set; }
    }
}
