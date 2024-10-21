namespace MhpdCommon.Constants.HttpClient;

public static class HttpEndpoints
{
    public static class Internal
    {
        public const string PeiRetrievalDetails = "pei_retrieval_details";
        public const string PensionsRetrievalRecords = "pensions-retrieval-records";
        public const string RetrievedPensionRecords = "retrieved-pension-records";
        public const string RedirectDetails = "redirect_details";
        public const string IntegrationPeis = "peis";
        public const string Rqp = "rqp";
        public const string Rpts = "rpts";
    }

    public static class External
    {
        public const string HolderNameViewConfigurations = "holdername-view-configurations";
        public const string CdaTokenServiceEndpoint = "token";
        public const string CdaPeis = "peis/{0}";
        public const string PdpViewData = "view-data/{0}";
    }
}