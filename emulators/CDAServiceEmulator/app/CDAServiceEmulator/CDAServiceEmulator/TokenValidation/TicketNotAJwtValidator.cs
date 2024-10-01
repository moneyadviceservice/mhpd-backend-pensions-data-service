using CDAServiceEmulator.Models.Token;
using MhpdCommon.Utils;

namespace CDAServiceEmulator.TokenValidation;

public class TicketNotAJwtValidator(ILogger<TicketNotAJwtValidator> logger) : ITokenRequestValidator
{
    public int Order => 10;
    
    public string GrantType => TokenQueryParams.UmaGrantType;

    public ValidationResult Validate(CdaTokenRequestModel request)
    {
        if (request is { Ticket: not null, GrantType: TokenQueryParams.UmaGrantType } && !JwtValidator.IsJwtFormatValid(request.Ticket))
        {
            logger.LogError(TokenValidationMessages.TicketNotAJwt);
            return ValidationResult.Failure(TokenValidationMessages.InvalidTicketQueryFormat);
        }

        return ValidationResult.Success();
    }
}