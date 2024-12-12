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

        public static class Vault
        {
            public const string Uri = "KeyVault:VaultUri";
            public const string MtlsCertificate = "KeyVault:MtlsCertificate";
        }

        public static class Mtls
        {
            public const string InvalidCertificateMessage = "Invalid client certificate";
        }
    }
}
