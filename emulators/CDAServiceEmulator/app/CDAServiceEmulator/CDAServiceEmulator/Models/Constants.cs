namespace CDAServiceEmulator.Models;

public static class Constants
{
    public static class HolderNameConstants
    {
        public const string InvalidRequestId = "Invalid X-Request-ID header";
        public const string InvalidHolderNameId = "Invalid holdername_guid format";
        public const string UnknownHolderNameId = "Unknown holdername_guid";
    }

    public static class TokenConstants
    {
        public const string NullIdTokenCode = "0097";
        public const string InvalidIdTokenCode = "0098";
        public const string MissingPeisTokenCode = "0099";
    }
}
