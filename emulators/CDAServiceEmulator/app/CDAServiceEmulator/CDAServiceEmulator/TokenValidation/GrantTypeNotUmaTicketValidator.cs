using CDAServiceEmulator.Models.Token;

namespace CDAServiceEmulator.TokenValidation;

public class GrantTypeNotUmaTicketValidator(ILogger<GrantTypeNotUmaTicketValidator> logger) : ITokenRequestValidator
{
    public int Order => 2;

    public ValidationResult Validate(CdaTokenRequestModel request)
    {
        if (request.GrantType != TokenQueryParams.UmaGrantType)
        {
            logger.LogError(TokenValidationMessages.GrantTypeNotUmaTicket);
            return ValidationResult.Failure(TokenValidationMessages.InvalidGrantType);
        }

        return ValidationResult.Success();
    }
}
