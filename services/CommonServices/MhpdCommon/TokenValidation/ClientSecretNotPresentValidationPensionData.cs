using MhpdCommon.Models.MessageBodyModels;
using Microsoft.Extensions.Logging;

namespace MhpdCommon.TokenValidation;

public class ClientSecretNotPresentValidationPensionData(ILogger<ClientSecretNotPresentValidationPensionData> logger) : ITokenRequestValidator<PensionsDataRequestModel>
{
    public int Order => 9;
    public string GrantType => string.Empty;
    
    public ValidationResult Validate(PensionsDataRequestModel request)
    {
        if (string.IsNullOrEmpty(request.ClientSecret))
        {
            logger.LogError(TokenValidationMessages.ClientSecretNotPresent);
            return ValidationResult.Failure(TokenValidationMessages.InvalidRequest);
        }

        return ValidationResult.Success();
    }
}