using MhpdCommon.Models.MessageBodyModels;
using Microsoft.Extensions.Logging;

namespace MhpdCommon.TokenValidation;

public class ClaimTokenFormatNotPresentValidator(ILogger<ClaimTokenFormatNotPresentValidator> logger)
    : ITokenRequestValidator<CdaTokenRequestModel>
{
    public int Order => 7;

    public string GrantType => TokenQueryParams.UmaGrantType;

    public ValidationResult Validate(CdaTokenRequestModel request)
    {
        if (request.GrantType == TokenQueryParams.UmaGrantType && string.IsNullOrEmpty(request.ClaimTokenFormat))
        {
            logger.LogError(TokenValidationMessages.ClaimTokenFormatNotPresent);
            return ValidationResult.Failure(TokenValidationMessages.InvalidClaimTokenFormat);
        }

        return ValidationResult.Success();
    }
}
