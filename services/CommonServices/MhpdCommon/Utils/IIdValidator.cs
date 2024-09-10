namespace MhpdCommon.Utils;

public interface IIdValidator
{
    bool IsValidGuid(string? guid);
    bool IsValidPeI(string? pei);
}
