using MhpdCommon.Models.MessageBodyModels;
using Microsoft.Extensions.Logging;

namespace MhpdCommon.TokenValidation;

public class ClaimTokenFormatNotPensionDashboardRqpValidator(ILogger<ClaimTokenFormatNotPensionDashboardRqpValidator> logger)
    : ITokenRequestValidator<CdaTokenRequestModel>
{
    public int Order => 8;
    
    public string GrantType => TokenQueryParams.UmaGrantType;

    public ValidationResult Validate(CdaTokenRequestModel request)
    {
        if (request.GrantType == TokenQueryParams.UmaGrantType && request.ClaimTokenFormat != TokenQueryParams.PensionDashboardRqp)
        {
            logger.LogError(TokenValidationMessages.ClaimTokenFormatNotDashboardRqp);
            return ValidationResult.Failure(TokenValidationMessages.InvalidClaimTokenFormat);
        }

        return ValidationResult.Success();
    }
}
