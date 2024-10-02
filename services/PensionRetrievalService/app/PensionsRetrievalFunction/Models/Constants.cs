namespace PensionsRetrievalFunction.Models;

internal static class Constants
{
    internal const string PeiDataEndpoint = "peis";

    internal static class RetrievalStatus
    {
        internal const string New = "NEW";
        internal const string Requested = "RETRIEVAL_REQUESTED";
        internal const string Complete = "RETRIEVAL_COMPLETE";
        internal const string Failed = "RETRIEVAL_FAILED";
    }
}
