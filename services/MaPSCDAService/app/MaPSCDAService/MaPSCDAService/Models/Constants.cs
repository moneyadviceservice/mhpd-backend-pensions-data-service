namespace MaPSCDAService.Models;

public static class Constants
{
    // Hardcoded values
    public const string NoRequestBody = "Request body not provided";
    public const string BadRequest = "Bad Request";
    public const string MissingOrInvalidIss = "Missing or invalid iss";
    public const string MissingOrInvalidUserSessionId = "Missing or invalid userSessionId";
    public const string MissingOrInvalidRedirectPurpose = "Missing or invalid redirectPurpose";
    public const string SampleRqpToken = "eyJ0eXAiOiJKV1QiLA0KICJhbGciOiJIUzI1NiJ9.eyJpc3MiOiJqb2UiLA0KICJleHAiOjEzMDA4MTkzODAsDQogImh0dHA6Ly9leGFtcGxlLmNvbS9pc19yb290Ijp0cnVlfQ.dBjftJeZ4CVP-mB92K27uhbUJU1p1r_wW1gFWFOEjXk";
    public const string Scope = "openid pdp";
    public const string ResponseType = "code";
    public const string Prompt = "login";
    public const string Service = "PDPPensionFinder";
    public const string CodeChallengeMethod = "S256";
    public const string RedirectPurpose = "FIND";
    
    // Test values
    public const string Kid = "ec1abf89-225b-49c2-ab87-1d425ac70f8d";
    public const string Audience = "https://pdp/ig/token";
    public const string Iss = "myapp.com";
    public const string UserSessionId123 = "mySessionId-123abcd";
    public const string UserSessionId = "ff2bba91-1867-4ff5-be4c-534ad527a59f";
    public const string InvalidIssUsersessionId = "Invalid input: Iss and UserSessionId are required.";
}
