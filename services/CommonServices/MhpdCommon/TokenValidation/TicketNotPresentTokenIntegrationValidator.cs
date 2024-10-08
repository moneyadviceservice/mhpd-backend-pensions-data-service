using MhpdCommon.Models.MessageBodyModels;
using Microsoft.Extensions.Logging;

namespace MhpdCommon.TokenValidation;

public class TicketNotPresentTokenIntegrationValidator(ILogger<TicketNotPresentTokenIntegrationValidator> logger) : 
    ITokenRequestValidator<TokenIntegrationRequestModel>
{
    public int Order => 3;

    public string GrantType => string.Empty;
    
    public ValidationResult Validate(TokenIntegrationRequestModel request)
    {
        if (string.IsNullOrEmpty(request.Ticket))
        {
            logger.LogError(TokenValidationMessages.TicketNotPresent);
            return ValidationResult.Failure(TokenValidationMessages.InvalidTicketQuery);
        }

        return ValidationResult.Success();
    }
}