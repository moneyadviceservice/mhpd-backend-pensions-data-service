using MhpdCommon.Models.MessageBodyModels;
using Microsoft.Extensions.Logging;

namespace MhpdCommon.TokenValidation;

public class AuthorisationCodeNotPresentValidationPensionsData(ILogger<AuthorisationCodeNotPresentValidationPensionsData> logger) : ITokenRequestValidator<PensionsDataRequestModel>
{
    public int Order => 1;
    public string GrantType => string.Empty;
    
    public ValidationResult Validate(PensionsDataRequestModel request)
    {
        if (string.IsNullOrEmpty(request.AuthorisationCode))
        {
            logger.LogError(TokenValidationMessages.AuthorisationCodeNotPresent);
            return ValidationResult.Failure(TokenValidationMessages.MissingAuthorisationCode);
        }

        return ValidationResult.Success();
    }
}