using CDAServiceEmulator.Models.Token;

namespace CDAServiceEmulator.TokenValidation;

public class RedirectUriNotPresentValidation(ILogger<RedirectUriNotPresentValidation> logger) : ITokenRequestValidator
{
    public int Order => 10;
    public string GrantType => TokenQueryParams.AuthorizationCodeGrantType;
    
    public ValidationResult Validate(CdaTokenRequestModel request)
    {
        if (string.IsNullOrEmpty(request.RedirectUri))
        {
            logger.LogError(TokenValidationMessages.RedirectUriNotPresent);
            return ValidationResult.Failure(TokenValidationMessages.InvalidRequest);
        }

        return ValidationResult.Success();
    }
}