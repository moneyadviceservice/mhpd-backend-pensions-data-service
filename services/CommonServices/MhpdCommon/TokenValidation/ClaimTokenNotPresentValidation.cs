using MhpdCommon.Models.MessageBodyModels;
using Microsoft.Extensions.Logging;

namespace MhpdCommon.TokenValidation;

public class ClaimTokenNotPresentValidation(ILogger<ClaimTokenNotPresentValidation> logger)
    : ITokenRequestValidator<CdaTokenRequestModel>
{
    public int Order => 5;
    public string GrantType => TokenQueryParams.UmaGrantType;

    public ValidationResult Validate(CdaTokenRequestModel request)
    {
        if (request.GrantType == TokenQueryParams.UmaGrantType && string.IsNullOrEmpty(request.ClaimToken))
        {
            logger.LogError(TokenValidationMessages.ClaimTokenNotPresent);
            return ValidationResult.Failure(TokenValidationMessages.InvalidClaimToken);
        }

        return ValidationResult.Success();
    }
}
