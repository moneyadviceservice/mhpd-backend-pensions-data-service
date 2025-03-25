using MhpdCommon.Models.MessageBodyModels;
using Microsoft.Extensions.Logging;

namespace MhpdCommon.TokenValidation;

public class TicketNotPresentTokenIntegrationValidator(ILogger<TicketNotPresentTokenIntegrationValidator> logger) : 
    ITokenRequestValidator<TokenClientRequestModel>
{
    public int Order => 3;

    public string GrantType => string.Empty;
    
    public ValidationResult Validate(TokenClientRequestModel request)
    {
        if (string.IsNullOrEmpty(request.Ticket))
        {
            logger.LogError(TokenValidationMessages.TicketNotPresent);
            return ValidationResult.Failure(TokenValidationMessages.InvalidTicketQuery);
        }

        return ValidationResult.Success();
    }
}