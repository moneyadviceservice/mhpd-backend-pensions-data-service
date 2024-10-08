using MhpdCommon.Models.MessageBodyModels;
using Microsoft.Extensions.Logging;

namespace MhpdCommon.TokenValidation;

public class ScopeNotOwnerValidator(ILogger<ScopeNotOwnerValidator> logger) : ITokenRequestValidator<CdaTokenRequestModel>
{
    public int Order => 4;

    public string GrantType => TokenQueryParams.UmaGrantType;

    public ValidationResult Validate(CdaTokenRequestModel request)
    {
        if (request.GrantType == TokenQueryParams.UmaGrantType && request.Scope != TokenQueryParams.Owner)
        {
            logger.LogError(TokenValidationMessages.ScopeNotOwner);
            return ValidationResult.Failure(TokenValidationMessages.ScopeNotOwner);
        }

        return ValidationResult.Success();
    }
}
