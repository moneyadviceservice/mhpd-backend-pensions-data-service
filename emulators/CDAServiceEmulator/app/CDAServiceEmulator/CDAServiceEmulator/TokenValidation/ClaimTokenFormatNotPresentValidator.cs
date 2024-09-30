using CDAServiceEmulator.Models.Token;

namespace CDAServiceEmulator.TokenValidation;

public class ClaimTokenFormatNotPresentValidator(ILogger<ClaimTokenFormatNotPresentValidator> logger)
    : ITokenRequestValidator
{
    public int Order => 7;

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
