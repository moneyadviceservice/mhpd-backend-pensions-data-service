namespace MhpdCommon.Models.MHPDModels;

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

    public enum AmountTypes
    {
        INC, INCL, INCN
    }

    public enum MatchType
    {
        POSS,  // Possible match
        DEFN,  // Definite match
        SYS,   // SYS match
        CONT,  // CONT match
        NEW    // NEW match
    }

    public enum IllustrationType
    {
        ERI, // Employer related investment pension
        AP,  // Accrued Pension
        UNDEFINED
    }
}

