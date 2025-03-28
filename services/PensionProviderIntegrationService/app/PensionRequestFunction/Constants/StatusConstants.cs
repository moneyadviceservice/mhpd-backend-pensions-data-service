namespace PensionRequestFunction.Constants;

public static class StatusConstants
{
    public const string ViewDataNotFound = "Unable obtain pension view data content for pei: {0} with correlationId: {1}";
    public const string UserSessionDataNotFound = "Unable obtain user session data for id: {0} with correlationId: {1}";
    public const string InvalidCorrelationId = "Missing or Invalid correlationId: {0}";
    public const string ViewDataNotFoundForPei = "Unable obtain pension view data content for pei: {pei} with correlationId: {correlationId}";
    public const string PayloadInvalid = "Invalid pension details request payload";
    public const string InvalidPei = "Pei value is missing or invalid";
    public const string FetchingRpt = "Fetching rpt to access view data for request with correlationId {correlationId}";
    public const string FetchingRqp = "Fetching RQP to access view data for request with correlationId {correlationId}";
    public const string NoViewDataUrl = "No view data Url was returned from PDP for this pei: {0}";
    public const string NoPctFound = "No PCT was returned on the user session data for this userSessionId: {0}";
    public const string MalformedArrangement = "A valid arrangement could not be formed for the pei: {0} using view data {1}";
    public const string InvalidExternalAssetId = "Invalid externalAssetId. It must be a valid GUID";
}
