namespace MhpdCommon.Constants
{
    public static class SecurityConstants
    {
        public static class Identity
        {
            public const string ClientId = "AzureAd:ClientId";
            public const string ClientSecret = "AzureAd:ClientSecret";
            public const string TenantId = "AzureAd:TenantId";
        }

        public static class Mtls
        {
            public const string VaultUri = "Mtls:VaultUri";
            public const string ClientCertificate = "Mtls:ClientCertificate";
            public const string InvalidCertificateMessage = "Invalid client certificate";
            public const string EnforceClientCertificate = "Mtls:EnforceClientCertificate";
        }
    }
}
