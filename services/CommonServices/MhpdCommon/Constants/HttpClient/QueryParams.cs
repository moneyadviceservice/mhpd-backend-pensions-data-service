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
        }

        public static class HolderName
        {
            public const string Guid = "holdername_guid";
        }
    }

    public const string RetrievalRecordId = "pensionsRetrievalRecordId";
}