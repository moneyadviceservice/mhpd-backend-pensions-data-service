namespace CDAServiceEmulator.TokenValidation;

public static class TokenValidationMessages
{
    // Bad request responses
    public const string InvalidGrantType = "Invalid grant_type";
    public const string MissingGrantType = "Missing grant_type";
    public const string InvalidTicketQuery = "Invalid ticket query";
    public const string InvalidTicketQueryFormat = "Invalid ticket query format, not in jwt format";
    public const string InvalidClaimToken = "Invalid claim token";
    public const string InvalidClaimTokenFormat = "Invalid claim token format";
    public const string InvalidScope = "Invalid scope";
    public const string InvalidXRequestId = "Invalid X-Request-ID";
    
    // Log error messages
    public const string ClaimTokenFormatNotDashboardRqp = "claim_token_format is not pension_dashboard_rqp";
    public const string ClaimTokenFormatNotPresent = "claim_token_format not provided in request";
    public const string ClaimTokenNotPresent = "claim_token not provided in request";
    public const string GrantTypeNotPresent = "grant_type not provided in request";
    public const string GrantTypeNotUmaTicket = "grant_type is not of uma-ticket type";
    public const string ScopeNotOwner = "Invalid scope, parameter not equal to owner";
    public const string ScopeNotPresent = "Scope not provided in request";
    public const string TicketNotPresent = "Ticket not provided in request";
    public const string TicketNotAJwt = "Ticket not a jwt";
    public const string ClaimTokenNotAJwt = "Claim token not a jwt";
}