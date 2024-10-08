using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.Utils;
using Microsoft.Extensions.Logging;

namespace MhpdCommon.TokenValidation;

public class ClientIdInvalidFormatValidation(ILogger<ClientIdInvalidFormatValidation> logger) : ITokenRequestValidator<CdaTokenRequestModel>
{
    public int Order => 3;
    public string GrantType => TokenQueryParams.AuthorizationCodeGrantType;
    
    public ValidationResult Validate(CdaTokenRequestModel request)
    {
        if (request.ClientId != null && !IdValidator.IsValidString(request.ClientId))
        {
            logger.LogError(TokenValidationMessages.ClientIdInvalidFormat);
            return ValidationResult.Failure(TokenValidationMessages.InvalidClientIdFormat);
        }

        return ValidationResult.Success();
    }
}