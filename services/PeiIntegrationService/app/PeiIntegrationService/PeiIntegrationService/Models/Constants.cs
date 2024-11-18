namespace PeiIntegrationService.Models
{
    public static class Constants
    {
        public const string LogSource = "Pei Integration Service";

        public static class ResponseType
        {
            public const string Iss = "Missing or invalid iss";
            public const string PeisId = "Missing or invalid peisId";
            public const string UserSessionId = "Missing or invalid userSessionId";
            public const string CorrelationId = "Invalid mhpdCorrelationId";
        }
    }
}
