namespace PensionsRetrievalFunction.Models;

public static class Constants
{
    internal static class RetrievalStatus
    {
        internal const string New = "NEW";
        internal const string Requested = "RETRIEVAL_REQUESTED";
        internal const string Complete = "RETRIEVAL_COMPLETE";
        internal const string Failed = "RETRIEVAL_FAILED";
    }

    public static class ResponseType
    {
        public const string InvalidSessionId = "userSessionId missing or invalid";
    }
}
