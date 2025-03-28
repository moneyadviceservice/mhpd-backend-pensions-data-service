namespace MhpdCommon.Constants.HttpClient;

public static class HttpEndpoints
{
    public static class Internal
    {
        public const string PeiRetrievalDetails = "pei-retrieval-details";
        public const string PensionsRetrievalRecords = "pensions-retrieval-records";
        public const string RetrievedPensionRecords = "retrieved-pension-records";
        public const string PensionDataRetrieval = "pension-data-retrieval";
        public const string RedirectDetails = "redirect-details";
        public const string ClaimsGatheringRedirect = "claims-gathering-redirect";
        public const string IntegrationPeis = "peis";
        public const string Rqp = "rqp";
        public const string Rpts = "rpts";
        public const string ClaimsGathering = "claims-gathering-redirect";
    }

    public static class External
    {
        public const string HolderNameViewConfigurations = "holdername-view-configurations";
        public const string CdaTokenServiceEndpoint = "token";
        public const string CdaPeis = "peis/{0}";
        public const string PdpViewData = "{0}?scope={1}";
        public const string JwkUri = "jwk_uri";
        public const string ClaimsGathering = "claims_gathering";
    }
}