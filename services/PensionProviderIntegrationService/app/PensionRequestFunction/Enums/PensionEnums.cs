namespace PensionRequestFunction.Enums;

public static class PensionEnums
{
    public enum PensionOrigin
    {
        A, PC, PM, PT, WC, WM, WT
    }

    public enum PensionStatus
    {
        A, I, IPPF, IWU
    }

    public enum MatchType
    {
        POSS, // Possible match
        DEFN  // Definite match
    }
}

