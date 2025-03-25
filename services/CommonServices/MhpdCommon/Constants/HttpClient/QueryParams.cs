namespace MhpdCommon.Constants.HttpClient;

public static class QueryParams
{
    public static class Cda
    {
        public static class Token
        {
            public const string GrantType = "grant_type";

            // When grant_type is authorization_code, expect these query params
            public const string ClientId = "client_id";
            public const string ClientSecret = "client_secret";
            public const string CodeVerifier = "code_verifier";
            public const string Code = "code";
            public const string AuthorisationCode = "authorisationCode";
            public const string RedirectUri = "redirect_uri";

            // When grant_type is uma-ticket, expect these query params
            public const string ClaimToken = "claim_token";
            public const string ClaimTokenFormat = "claim_token_format";
            public const string Scope = "scope";
            public const string Ticket = "ticket";

            public const string AsUri = "as_uri";
            public const string PeisId = "peis_id";
            
            public const string Kid = "kid";
            
            public const string Pct = "pct";
        }

        public static class HolderName
        {
            public const string Guid = "holdernameGuid";
        }

        public static class ClaimsGathering
        {
            public const string State = "state";
            public const string ClaimsRedirectUri = "claims_redirect_uri";
            public const string RqpToken = "rqp_token";
            public const string RequestId = "request_id";
        }
    }

    public const string RetrievalRecordId = "pensionsRetrievalRecordId";
}