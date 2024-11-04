using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.Utils;
using Microsoft.Extensions.Logging;

namespace MhpdCommon.TokenValidation;

public class ClientSecretNotGuidValidationPensionData(ILogger<ClientSecretNotGuidValidationPensionData> logger, IIdValidator idValidator) : ITokenRequestValidator<PensionsDataRequestModel>
{
    public int Order => 10;
    public string GrantType => string.Empty;
    
    public ValidationResult Validate(PensionsDataRequestModel request)
    {
        if (!idValidator.IsValidGuid(request.ClientSecret))
        {
            logger.LogError(TokenValidationMessages.ClientSecretNotAGuid);
            return ValidationResult.Failure(TokenValidationMessages.InvalidClientSecretFormat);
        }

        return ValidationResult.Success();
    }
}