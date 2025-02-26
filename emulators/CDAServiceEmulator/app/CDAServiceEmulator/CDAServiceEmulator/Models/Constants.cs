namespace CDAServiceEmulator.Models;

public static class Constants
{
    public static class HolderNameConstants
    {
        public const string InvalidRequestId = "Invalid X-Request-ID header";
        public const string InvalidHolderNameId = "Invalid holdernamGuid format";
        public const string UnknownHolderNameId = "Unknown holdernameGuid";
    }

    public static class TokenConstants
    {
        public const string NoKidTokenCode = "0093";
        public const string UnknownKidTokenCode = "0094";
        public const string ExpiredTokenCode = "0095";
        public const string UnknownKeyTokenCode = "0096";
        public const string NullIdTokenCode = "0097";
        public const string InvalidIdTokenCode = "0098";
        public const string MissingPeisTokenCode = "0099";
    }
}
