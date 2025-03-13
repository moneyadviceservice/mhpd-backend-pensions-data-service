using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.Utils;
using Microsoft.Extensions.Logging;

namespace MhpdCommon.TokenValidation;

public class TicketNotAJweTokenIntegrationValidator(ILogger<TicketNotAJweTokenIntegrationValidator> logger) : 
    ITokenRequestValidator<TokenIntegrationRequestModel>
{
    public int Order => 4;
    public string GrantType => string.Empty;
    
    public ValidationResult Validate(TokenIntegrationRequestModel request)
    {
        if (request.Ticket != null && !JweValidator.IsJweFormatValid(request.Ticket))
        {
            logger.LogError(TokenValidationMessages.TicketNotAJwe);
            return ValidationResult.Failure(TokenValidationMessages.InvalidJweTicketQueryFormat);
        }

        return ValidationResult.Success();
    }
}