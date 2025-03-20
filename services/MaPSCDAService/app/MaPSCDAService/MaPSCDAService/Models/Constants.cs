namespace MaPSCDAService.Models;

public static class Constants
{
    public const string NoRequestBody = "Request body not provided";
    public const string NoAccessToVault = "Unable to access secrets in the key vault";
    public const string MissingOrInvalidIss = "Missing or invalid iss";
    public const string MissingOrInvalidUserSessionId = "Missing or invalid userSessionId";
    public const string MissingOrInvalidRedirectPurpose = "Missing or invalid redirectPurpose";
    public const string InvalidCorrelationId = "Invalid mhpdCorrelationId";
    public const string NoSessionDataFound = "No user session data found";
    public const string UnableToTriggerRedirect = "Unable to trigger claim gathering flow";
    public const string SampleRqpToken = "eyJ0eXAiOiJKV1QiLA0KICJhbGciOiJIUzI1NiJ9.eyJpc3MiOiJqb2UiLA0KICJleHAiOjEzMDA4MTkzODAsDQogImh0dHA6Ly9leGFtcGxlLmNvbS9pc19yb290Ijp0cnVlfQ.dBjftJeZ4CVP-mB92K27uhbUJU1p1r_wW1gFWFOEjXk";
    public const string Scope = "openid pdp";
    public const string ResponseType = "code";
    public const string Prompt = "login";
    public const string Service = "PDPPensionFinder";
    public const string CodeChallengeMethod = "S256";
    public const string RedirectPurpose = "FIND";
    public const string Kid = "Kid";
    public const string Audience = "Audience";
    public const string LogSource = "MapsCDA Service";
}
