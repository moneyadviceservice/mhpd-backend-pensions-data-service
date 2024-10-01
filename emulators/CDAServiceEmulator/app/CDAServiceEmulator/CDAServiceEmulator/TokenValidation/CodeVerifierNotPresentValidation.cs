using CDAServiceEmulator.Models.Token;

namespace CDAServiceEmulator.TokenValidation;

public class CodeVerifierNotPresentValidation(ILogger<CodeVerifierNotPresentValidation> logger) : ITokenRequestValidator
{
    public int Order => 8;
    public string GrantType => TokenQueryParams.AuthorizationCodeGrantType;
    
    public ValidationResult Validate(CdaTokenRequestModel request)
    {
        if (string.IsNullOrEmpty(request.CodeVerifier))
        {
            logger.LogError(TokenValidationMessages.CodeVerifierNotPresent);
            return ValidationResult.Failure(TokenValidationMessages.InvalidRequest);
        }

        return ValidationResult.Success();
    }
}