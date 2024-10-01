using CDAServiceEmulator.Models.Token;

namespace CDAServiceEmulator.TokenValidation;

public class UnsupportedGrantTypeValidation(ILogger<UnsupportedGrantTypeValidation> logger) : ITokenRequestValidator
{
    public int Order => 2;

    public string GrantType => string.Empty;

    public ValidationResult Validate(CdaTokenRequestModel request)
    {
        if (request.GrantType is not (TokenQueryParams.UmaGrantType or TokenQueryParams.AuthorizationCodeGrantType))
        {
            logger.LogError(TokenValidationMessages.UnsupportedGrantType);
            return ValidationResult.Failure(TokenValidationMessages.UnsupportedGrantType);
        }

        return ValidationResult.Success();
    }
}
