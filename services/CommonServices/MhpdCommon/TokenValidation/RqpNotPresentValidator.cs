using MhpdCommon.Models.MessageBodyModels;
using Microsoft.Extensions.Logging;

namespace MhpdCommon.TokenValidation;

public class RqpNotPresentValidator(ILogger<RqpNotPresentValidator> logger) : ITokenRequestValidator<TokenClientRequestModel>
{
    public int Order => 1;

    public string GrantType => String.Empty;
    
    public ValidationResult Validate(TokenClientRequestModel request)
    {
        if (string.IsNullOrEmpty(request.Rqp))
        {
            logger.LogError(TokenValidationMessages.RqpNotPresent);
            return ValidationResult.Failure(TokenValidationMessages.InvalidRqp);
        }

        return ValidationResult.Success();
    }
}