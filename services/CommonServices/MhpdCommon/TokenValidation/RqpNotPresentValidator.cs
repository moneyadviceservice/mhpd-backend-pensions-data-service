using MhpdCommon.Models.MessageBodyModels;
using Microsoft.Extensions.Logging;

namespace MhpdCommon.TokenValidation;

public class RqpNotPresentValidator(ILogger<RqpNotPresentValidator> logger) : ITokenRequestValidator<TokenIntegrationRequestModel>
{
    public int Order => 1;

    public string GrantType => String.Empty;
    
    public ValidationResult Validate(TokenIntegrationRequestModel request)
    {
        if (string.IsNullOrEmpty(request.Rqp))
        {
            logger.LogError(TokenValidationMessages.RqpNotPresent);
            return ValidationResult.Failure(TokenValidationMessages.InvalidRqp);
        }

        return ValidationResult.Success();
    }
}