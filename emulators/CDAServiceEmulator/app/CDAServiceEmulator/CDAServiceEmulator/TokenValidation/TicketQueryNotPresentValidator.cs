using CDAServiceEmulator.Models.Token;

namespace CDAServiceEmulator.TokenValidation;

public class TicketQueryNotPresentValidator(ILogger<TicketQueryNotPresentValidator> logger) : ITokenRequestValidator
{
    public int Order => 7;
    
    public ValidationResult Validate(CdaTokenRequestModel request)
    {
        if (request.GrantType == TokenQueryParams.UmaGrantType && string.IsNullOrEmpty(request.Ticket))
        {
            logger.LogError(TokenValidationMessages.TicketNotPresent);
            return ValidationResult.Failure(TokenValidationMessages.InvalidTicketQuery);
        }

        return ValidationResult.Success();
    }
}