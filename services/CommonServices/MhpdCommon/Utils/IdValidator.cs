using System.Drawing;
using System.Text.RegularExpressions;

namespace MhpdCommon.Utils;

public class IdValidator : IIdValidator
{
    private const int Length = 73;
    private const string Pattern = "^(?:(?:[0-9a-fA-F]){8}-(?:[0-9a-fA-F]){4}-(?:[0-9a-fA-F]){4}-(?:[0-9a-fA-F]){4}-(?:[0-9a-fA-F]){12}\\}{0,1}?:(?:[0-9a-fA-F]){8}-(?:[0-9a-fA-F]){4}-(?:[0-9a-fA-F]){4}-(?:[0-9a-fA-F]){4}-(?:[0-9a-fA-F]){12}\\}{0,1})$";

    public bool IsValidGuid(string? guid)
    {
        return Guid.TryParse(guid, out var id) && id != Guid.Empty;
    }

    public bool IsValidPeI(string? pei)
    {
        return !string.IsNullOrEmpty(pei) && pei.Length == Length && Regex.IsMatch(pei, Pattern);
    }

    public bool TryExtractPei(string? pei, out string holderNameId, out string externalAssetId)
    {
        holderNameId = string.Empty;
        externalAssetId = string.Empty;

        if(!IsValidPeI(pei)) return false;

        holderNameId = pei!.Split(":")[0];
        externalAssetId = pei!.Split(":")[1];

        return true;
    }
    
    // Method to check if a string matches the pattern
    public static bool IsValidString(string input)
    {
        // Regex pattern to match printable ASCII characters (space to tilde)
        const string pattern = @"^[\x20-\x7E]+$";

        // Return true if input matches the pattern
        return Regex.IsMatch(input, pattern, RegexOptions.None, TimeSpan.FromMilliseconds(500));
    }
}
