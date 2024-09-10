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
}
