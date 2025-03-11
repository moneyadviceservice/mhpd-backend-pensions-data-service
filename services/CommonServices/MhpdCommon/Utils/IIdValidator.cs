namespace MhpdCommon.Utils;

public interface IIdValidator
{
    bool IsValidGuid(string? guid);

    bool IsValidPeI(string? pei);
    
    bool IsValidPeisId(string? id);

    bool TryExtractPei(string? pei, out string holderNameId, out string externalAssetId);
}
