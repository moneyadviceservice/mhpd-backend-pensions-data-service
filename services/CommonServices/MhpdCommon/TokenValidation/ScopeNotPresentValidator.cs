using MhpdCommon.Models.MessageBodyModels;
using Microsoft.Extensions.Logging;

namespace MhpdCommon.TokenValidation;

public class ScopeNotPresentValidator(ILogger<ScopeNotPresentValidator> logger) : ITokenRequestValidator<CdaTokenRequestModel>
{
    public int Order => 3;
    
    public string GrantType => TokenQueryParams.UmaGrantType;

    public ValidationResult Validate(CdaTokenRequestModel request)
    {
        if (request.GrantType == TokenQueryParams.UmaGrantType && string.IsNullOrEmpty(request.Scope))
        {
            logger.LogError(TokenValidationMessages.ScopeNotPresent);
            return ValidationResult.Failure(TokenValidationMessages.InvalidScope);
        }

        return ValidationResult.Success();
    }
}