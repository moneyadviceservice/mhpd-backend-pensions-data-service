using CDAServiceEmulator.Models.Token;

namespace CDAServiceEmulator.TokenValidation;

public class ScopeNotOwnerValidator(ILogger<ScopeNotOwnerValidator> logger) : ITokenRequestValidator
{
    public int Order => 4;

    public ValidationResult Validate(CdaTokenRequestModel request)
    {
        if (request.GrantType == TokenQueryParams.UmaGrantType && request.Scope != TokenQueryParams.Owner)
        {
            logger.LogError(TokenValidationMessages.ScopeNotOwner);
            return ValidationResult.Failure(TokenValidationMessages.ScopeNotOwner);
        }

        return ValidationResult.Success();
    }
}
