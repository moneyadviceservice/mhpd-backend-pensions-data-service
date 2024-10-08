using MhpdCommon.Models.MessageBodyModels;
using Microsoft.Extensions.Logging;

namespace MhpdCommon.TokenValidation;

public class GrantTypeNotPresentValidator(ILogger<GrantTypeNotPresentValidator> logger) : ITokenRequestValidator<CdaTokenRequestModel>
{
    public int Order => 1;
    
    public string GrantType => string.Empty;

    public ValidationResult Validate(CdaTokenRequestModel request)
    {
        if (string.IsNullOrEmpty(request.GrantType))
        {
            logger.LogError(TokenValidationMessages.GrantTypeNotPresent);
            return ValidationResult.Failure(TokenValidationMessages.MissingGrantType);
        }

        return ValidationResult.Success();
    }
}