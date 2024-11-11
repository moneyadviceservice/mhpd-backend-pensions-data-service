namespace PensionRequestFunction.Models.MapsRqpServiceClient
{
    public class MapsRqpServiceRequestModel
    {
        public string? Iss { get; set; }

        public string? UserSessionId { get; set; }

        public string CorrelationId { get; set; } = string.Empty;
    }
}
