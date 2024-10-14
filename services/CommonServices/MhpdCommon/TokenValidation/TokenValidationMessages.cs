namespace MhpdCommon.TokenValidation;

public static class TokenValidationMessages
{
    // Bad request responses
    public const string InvalidRequest = "Invalid request";
    public const string InvalidGrantType = "Invalid grant_type";
    public const string MissingGrantType = "Missing grant_type";
    public const string MissingAuthorisationCode = "Missing authorisation code";
    public const string InvalidTicketQuery = "Invalid ticket query";
    public const string InvalidTicketQueryFormat = "Invalid ticket query format, not in jwt format";
    public const string InvalidRqpFormat = "Invalid rqp format, not in jwt format";
    public const string InvalidClaimToken = "Invalid claim token";
    public const string InvalidClaimTokenFormat = "Invalid claim token format";
    public const string InvalidScope = "Invalid scope";
    public const string InvalidXRequestId = "Invalid X-Request-ID";
    public const string InvalidClientIdFormat = "Invalid client_id format";
    public const string InvalidCodeFormat = "Invalid code format";
    public const string InvalidAuthorisationCodeFormat = "Invalid authorisation code format";
    public const string InvalidClientSecretFormat = "Invalid client_secret";
    public const string InvalidRedirectUri = "Invalid rediect_uri";
    public const string InvalidCodeVerifierFormat = "Invalid code_verifier";
    public const string UnknownAuthorizationCode = "Unknown authorization code";
    public const string UnsupportedGrantType = "Unsupported grant_type";
    public const string InvalidCodeVerifier = "Invalid code_verifier";
    public const string InvalidRqp = "Invalid rqp";
    public const string InvalidAsUri = "Invalid as_uri";
    public const string InvalidIss = "Invalid iss";
    public const string MissingIss = "Missing iss";
    public const string MissingUserSessionId = "Missing userSessionId";
    public const string InvalidUserSessionId = "Invalid userSessionId";
    
    // Log error messages
    public const string ClaimTokenFormatNotDashboardRqp = "claim_token_format is not pension_dashboard_rqp";
    public const string ClaimTokenFormatNotPresent = "claim_token_format not provided in request";
    public const string ClaimTokenNotPresent = "claim_token not provided in request";
    public const string GrantTypeNotPresent = "grant_type not provided in request";
    public const string ScopeNotOwner = "Invalid scope, parameter not equal to owner";
    public const string ScopeNotPresent = "Scope not provided in request";
    public const string TicketNotPresent = "Ticket not provided in request";
    public const string TicketNotAJwt = "Ticket not a jwt";
    public const string RqpNotAJwt = "Rqp not a jwt";
    public const string ClaimTokenNotAJwt = "Claim token not a jwt";
    public const string ClientIdNotPresent = "client_id not provided in request";
    public const string ClientIdInvalidFormat = "client_id format is incorrect";
    public const string ClientSecretNotAGuid = "client_id format is incorrect";
    public const string ClientSecretNotPresent = "client_secret not provided in request";
    public const string CodeNotPresent = "Code not provided in request";
    public const string AuthorisationCodeNotPresent = "Authorisation code not provided in request";
    public const string AuthorisationCodeInvalidFormat = "Authorisation code format is incorrect";
    public const string CodeInvalidFormat = "code format is incorrect";
    public const string CodeVerifierNotPresent = "Code_verifier not provided in request";
    public const string RedirectUriNotPresent = "redirect_uri not provided in request";
    public const string RedirectUriNotValidFormat = "redirect_uri incorrect format";
    public const string AsUriNotValidFormat = "as_uri incorrect format";
    public const string RqpNotPresent = "Rqp not provided in request";
    public const string AsUriNotPresent = "as_uri not provided in request";
}