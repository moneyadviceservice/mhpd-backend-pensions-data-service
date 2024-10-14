using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.Utils;
using Microsoft.Extensions.Logging;

namespace MhpdCommon.TokenValidation;

public class AuthorisationCodeInvalidFormatValidationPensionsData(ILogger<AuthorisationCodeInvalidFormatValidationPensionsData> logger) : ITokenRequestValidator<PensionsDataRequestModel>
{
    public int Order => 2;
    public string GrantType => string.Empty;
    
    public ValidationResult Validate(PensionsDataRequestModel request)
    {
        if (request.AuthorisationCode != null && !IdValidator.IsValidString(request.AuthorisationCode))
        {
            logger.LogError(TokenValidationMessages.AuthorisationCodeInvalidFormat);
            return ValidationResult.Failure(TokenValidationMessages.InvalidAuthorisationCodeFormat);
        }

        return ValidationResult.Success();
    }
}