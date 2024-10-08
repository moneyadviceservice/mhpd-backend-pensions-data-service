using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.Utils;
using Microsoft.Extensions.Logging;

namespace MhpdCommon.TokenValidation;

public class ClaimTokenNotJwtValidator(ILogger<ClaimTokenNotJwtValidator> logger) : ITokenRequestValidator<CdaTokenRequestModel>
{
    public int Order => 6;
    public string GrantType => TokenQueryParams.UmaGrantType;

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