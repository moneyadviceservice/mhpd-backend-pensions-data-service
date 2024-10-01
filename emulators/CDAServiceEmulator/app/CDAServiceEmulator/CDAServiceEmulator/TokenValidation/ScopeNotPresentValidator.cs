using CDAServiceEmulator.Models.Token;

namespace CDAServiceEmulator.TokenValidation;

public class ScopeNotPresentValidator(ILogger<ScopeNotPresentValidator> logger) : ITokenRequestValidator
{
    public int Order => 3;
    
    public string GrantType => TokenQueryParams.UmaGrantType;

    public ValidationResult Validate(CdaTokenRequestModel request)
    {
        if (request.GrantType == TokenQueryParams.UmaGrantType && string.IsNullOrEmpty(request.Scope))
        {
            logger.LogError(TokenValidationMessages.ScopeNotPresent);
            return ValidationResult.Failure(TokenValidationMessages.InvalidScope);
        }

        return ValidationResult.Success();
    }
}