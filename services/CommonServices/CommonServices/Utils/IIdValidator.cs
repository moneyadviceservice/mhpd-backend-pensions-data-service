namespace MAPS.Core.Interfaces
{
    public interface IIdValidator
    {
        bool IsValidGuid(string? guid);
        bool IsValidPeI(string? pei);
    }
}
