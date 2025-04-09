namespace TokenIntegrationService.Models
{
    public static class Constants
    {
        public const string LogSource = "Token Integration Service";
        public const string InvalidCorrelationId = "Invalid mhpdCorrelationId";
        public const string TokenSerialisationError = "Unable to read from Cda Token response";
        public const string TokenServiceResponseError = "Unable to get token or redirect details";
    }
}
