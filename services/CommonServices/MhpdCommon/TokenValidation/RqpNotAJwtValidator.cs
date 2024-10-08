using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.Utils;
using Microsoft.Extensions.Logging;

namespace MhpdCommon.TokenValidation;

public class RqpNotAJwtValidator(ILogger<RqpNotAJwtValidator> logger) : ITokenRequestValidator<TokenIntegrationRequestModel>
{
    public int Order => 2;

    public string GrantType => String.Empty;
    
    public ValidationResult Validate(TokenIntegrationRequestModel request)
    {
        if (request.Rqp != null && !JwtValidator.IsJwtFormatValid(request.Rqp))
        {
            logger.LogError(TokenValidationMessages.RqpNotAJwt);
            return ValidationResult.Failure(TokenValidationMessages.InvalidRqpFormat);
        }

        return ValidationResult.Success();
    }
}