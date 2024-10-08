namespace MhpdCommon.TokenValidation;

public static class TokenQueryParams
{
    public const string UmaGrantType = "urn:ietf:params:oauth:grant-type:uma-ticket";
    public const string AuthorizationCodeGrantType = "authorization_code";
    public const string PensionDashboardRqp = "pension_dashboard_rqp";
    public const string TokenTypeRpt = "pension_dashboard_rpt";
    public const string TokenTypeBearer = "bearer";
    public const string Owner = "owner";
    public const string ValidJwtToken =
        "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";
    public const string ValidCodeVerifier = "7189b64cc5f65b805baf201e384dc53ae7d18305d5ebb6170ad557b6";
    public const string ValidCode = "valid-code";
    public const string ValidClientId = "valid-client-id";
    public const string ValidClientSecret = "123e4567-e89b-12d3-a456-426614174000";
}