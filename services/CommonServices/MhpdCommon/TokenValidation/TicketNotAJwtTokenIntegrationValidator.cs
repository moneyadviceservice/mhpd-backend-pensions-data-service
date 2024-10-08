using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.Utils;
using Microsoft.Extensions.Logging;

namespace MhpdCommon.TokenValidation;

public class TicketNotAJwtTokenIntegrationValidator(ILogger<TicketNotAJwtTokenIntegrationValidator> logger) : 
    ITokenRequestValidator<TokenIntegrationRequestModel>
{
    public int Order => 4;
    public string GrantType => string.Empty;
    
    public ValidationResult Validate(TokenIntegrationRequestModel request)
    {
        if (request.Ticket != null && !JwtValidator.IsJwtFormatValid(request.Ticket))
        {
            logger.LogError(TokenValidationMessages.TicketNotAJwt);
            return ValidationResult.Failure(TokenValidationMessages.InvalidTicketQueryFormat);
        }

        return ValidationResult.Success();
    }
}