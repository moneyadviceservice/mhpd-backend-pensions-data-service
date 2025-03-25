using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace MhpdCommon.TokenValidation;

public class RqpNotAJwtValidator(ILogger<RqpNotAJwtValidator> logger) : ITokenRequestValidator<TokenClientRequestModel>
{
    public int Order => 2;

    public string GrantType => String.Empty;
    
    public ValidationResult Validate(TokenClientRequestModel request)
    {
        if (!JwtValidator.IsJwtFormatValid(request.Rqp))
        {
            logger.LogError(TokenValidationMessages.RqpNotAJwt);
            return ValidationResult.Failure(TokenValidationMessages.InvalidRqpFormat);
        }

        return ValidationResult.Success();
    }
}