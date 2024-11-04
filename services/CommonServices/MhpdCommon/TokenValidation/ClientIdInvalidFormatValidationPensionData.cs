using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.Utils;
using Microsoft.Extensions.Logging;

namespace MhpdCommon.TokenValidation;

public class ClientIdInvalidFormatValidationPensionData(ILogger<ClientIdInvalidFormatValidationPensionData> logger) : ITokenRequestValidator<PensionsDataRequestModel>
{
    public int Order => 7;
    public string GrantType => TokenQueryParams.AuthorizationCodeGrantType;
    
    public ValidationResult Validate(PensionsDataRequestModel request)
    {
        if (request.ClientId != null && !IdValidator.IsValidString(request.ClientId))
        {
            logger.LogError(TokenValidationMessages.ClientIdInvalidFormat);
            return ValidationResult.Failure(TokenValidationMessages.InvalidClientIdFormat);
        }

        return ValidationResult.Success();
    }
}