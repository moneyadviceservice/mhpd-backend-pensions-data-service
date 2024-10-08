using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.Utils;
using Microsoft.Extensions.Logging;

namespace MhpdCommon.TokenValidation;

public class ClientSecretNotGuidValidation(ILogger<ClientSecretNotGuidValidation> logger, IIdValidator idValidator) : ITokenRequestValidator<CdaTokenRequestModel>
{
    public int Order => 5;
    public string GrantType => TokenQueryParams.AuthorizationCodeGrantType;
    
    public ValidationResult Validate(CdaTokenRequestModel request)
    {
        if (!idValidator.IsValidGuid(request.ClientSecret))
        {
            logger.LogError(TokenValidationMessages.ClientSecretNotAGuid);
            return ValidationResult.Failure(TokenValidationMessages.InvalidClientSecretFormat);
        }

        return ValidationResult.Success();
    }
}