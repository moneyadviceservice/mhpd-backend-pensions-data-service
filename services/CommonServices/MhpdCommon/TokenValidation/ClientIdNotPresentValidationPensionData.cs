using MhpdCommon.Models.MessageBodyModels;
using Microsoft.Extensions.Logging;

namespace MhpdCommon.TokenValidation;

public class ClientIdNotPresentValidationPensionData(ILogger<ClientIdNotPresentValidationPensionData> logger) : ITokenRequestValidator<PensionsDataRequestModel>
{
    public int Order => 8;
    public string GrantType => string.Empty;
    
    public ValidationResult Validate(PensionsDataRequestModel request)
    {
        if (string.IsNullOrEmpty(request.ClientId))
        {
            logger.LogError(TokenValidationMessages.ClientIdNotPresent);
            return ValidationResult.Failure(TokenValidationMessages.InvalidRequest);
        }

        return ValidationResult.Success();
    }
}