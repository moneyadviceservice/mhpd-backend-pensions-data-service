namespace PensionRequestFunction.Constants;

public static class StatusConstants
{
    public const string ViewDataNotFound = "Unable obtain pension view data content for pei: {0} with correlationId: {1}";
    public const string InvalidCorrelationId = "Missing or Invalid correlationId: {0}";
    public const string ViewDataNotFoundForPei = "Unable obtain pension view data content for pei: {pei} with correlationId: {correlationId}";
    public const string PayloadInvalid = "Invalid pension details request payload";
    public const string InvalidPei = "Pei value is missing or invalid";
    public const string FetchingRpt = "Fetching rpt to access view data for request with correlationId {correlationId}";
    public const string NoViewDataUrl = "No view data Url was returned from PDP for this Pei: {0}";
}
