using CDAServiceEmulator.Models.Token;

namespace CDAServiceEmulator.TokenValidation;

public class CodeNotPresentValidation(ILogger<CodeNotPresentValidation> logger) : ITokenRequestValidator
{
    public int Order => 6;
    public string GrantType => TokenQueryParams.AuthorizationCodeGrantType;
    
    public ValidationResult Validate(CdaTokenRequestModel request)
    {
        if (string.IsNullOrEmpty(request.Code))
        {
            logger.LogError(TokenValidationMessages.CodeNotPresent);
            return ValidationResult.Failure(TokenValidationMessages.InvalidRequest);
        }

        return ValidationResult.Success();
    }
}