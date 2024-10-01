using CDAServiceEmulator.Models.Token;

namespace CDAServiceEmulator.TokenValidation;

public class ClientIdInvalidFormatValidation(ILogger<ClientIdInvalidFormatValidation> logger) : ITokenRequestValidator
{
    public int Order => 3;
    public string GrantType => TokenQueryParams.AuthorizationCodeGrantType;
    
    public ValidationResult Validate(CdaTokenRequestModel request)
    {
        if (request.ClientId != null && !Utils.IsValidString(request.ClientId))
        {
            logger.LogError(TokenValidationMessages.ClientIdInvalidFormat);
            return ValidationResult.Failure(TokenValidationMessages.InvalidClientIdFormat);
        }

        return ValidationResult.Success();
    }
}