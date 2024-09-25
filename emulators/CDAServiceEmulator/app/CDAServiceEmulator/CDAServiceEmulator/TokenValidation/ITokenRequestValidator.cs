using CDAServiceEmulator.Models.Token;

namespace CDAServiceEmulator.TokenValidation;

public interface ITokenRequestValidator
{
    ValidationResult Validate(CdaTokenRequestModel request);
    int Order { get; }
}