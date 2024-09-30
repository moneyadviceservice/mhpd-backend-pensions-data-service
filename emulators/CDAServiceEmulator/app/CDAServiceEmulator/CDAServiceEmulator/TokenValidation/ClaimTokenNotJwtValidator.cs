using CDAServiceEmulator.Models.Token;
using MhpdCommon.Utils;

namespace CDAServiceEmulator.TokenValidation;

public class ClaimTokenNotJwtValidator(ILogger<ClaimTokenNotJwtValidator> logger) : ITokenRequestValidator
{
    public int Order => 6;

    public ValidationResult Validate(CdaTokenRequestModel request)
    {
        if (request is { ClaimToken: not null, GrantType: TokenQueryParams.UmaGrantType } && !JwtValidator.IsJwtFormatValid(request.ClaimToken))
        {
            logger.LogError(TokenValidationMessages.ClaimTokenNotAJwt);
            return ValidationResult.Failure(TokenValidationMessages.InvalidClaimToken);
        }

        return ValidationResult.Success();
    }
}