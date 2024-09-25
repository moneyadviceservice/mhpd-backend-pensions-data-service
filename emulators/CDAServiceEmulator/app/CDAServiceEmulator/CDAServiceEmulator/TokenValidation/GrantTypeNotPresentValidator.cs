using CDAServiceEmulator.Models.Token;

namespace CDAServiceEmulator.TokenValidation;

public class GrantTypeNotPresentValidator(ILogger<GrantTypeNotPresentValidator> logger) : ITokenRequestValidator
{
    public int Order => 1;
    
    public ValidationResult Validate(CdaTokenRequestModel request)
    {
        if (string.IsNullOrEmpty(request.GrantType))
        {
            logger.LogError(TokenValidationMessages.GrantTypeNotPresent);
            return ValidationResult.Failure(TokenValidationMessages.MissingGrantType);
        }

        return ValidationResult.Success();
    }
}