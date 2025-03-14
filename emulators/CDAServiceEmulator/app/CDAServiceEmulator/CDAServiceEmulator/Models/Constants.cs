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
        public const string NoKidTokenCode = "9993";
        public const string UnknownKidTokenCode = "9994";
        public const string ExpiredTokenCode = "9995";
        public const string UnknownKeyTokenCode = "9996";
        public const string NullIdTokenCode = "9997";
        public const string InvalidIdTokenCode = "9998";
        public const string MissingPeisTokenCode = "9999";
    }
}
