using CDAServiceEmulator.Models.Token;

namespace CDAServiceEmulator.TokenValidation;

public class ClaimTokenNotPresentValidation(ILogger<ClaimTokenNotPresentValidation> logger)
    : ITokenRequestValidator
{
    public int Order => 5;

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
