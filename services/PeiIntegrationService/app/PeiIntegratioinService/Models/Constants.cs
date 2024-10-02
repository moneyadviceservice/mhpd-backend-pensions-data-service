namespace PeiIntegrationService.Models;

internal static class Constants
{
    internal static class HttpClients
    {
        internal const string CdaPiesService = "CdaPiesService";
        internal const string MapsCdaService = "MapsCdaService";
        internal const string TokenIntegrationService = "TokenIntegrationService";
    }

    internal static class HttpEndpoints
	{
        internal const string CdaPeisServiceEndpoint = "CdaPeisServiceEndpoint";
        internal const string MapsCdaServiceEndpoint = "MapsCdaServiceEndpoint";
        internal const string TokenIntegrationServiceEndpoint = "TokenIntegrationServiceEndpoint";
        internal const string CdaTokenServicesEndpoint = "CdaTokenServicesEndpoint";
    }

    internal static class Headers
    {
        internal const string AuthenticateType = "Bearer";
        internal const string AuthenticateTicket = "ticket=";
        internal const string AuthenticateUri = "as_uri=";
        internal const string RequestId = "X-Request-ID";
    }

    internal static class RequestRoutes
    {
        internal const string Rqp = "rqp";
        internal const string Rpt = "rpts";
    }
}
