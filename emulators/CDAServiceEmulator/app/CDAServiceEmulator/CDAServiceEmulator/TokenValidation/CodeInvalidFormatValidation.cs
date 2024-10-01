using CDAServiceEmulator.Models.Token;

namespace CDAServiceEmulator.TokenValidation;

public class CodeInvalidFormatValidation(ILogger<CodeInvalidFormatValidation> logger) : ITokenRequestValidator
{
    public int Order => 7;
    public string GrantType => TokenQueryParams.AuthorizationCodeGrantType;
    
    public ValidationResult Validate(CdaTokenRequestModel request)
    {
        if (request.Code != null && !Utils.IsValidString(request.Code))
        {
            logger.LogError(TokenValidationMessages.CodeInvalidFormat);
            return ValidationResult.Failure(TokenValidationMessages.InvalidCodeFormat);
        }

        return ValidationResult.Success();
    }
}