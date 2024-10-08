using MhpdCommon.Models.MessageBodyModels;
using Microsoft.Extensions.Logging;

namespace MhpdCommon.TokenValidation;

public class TicketQueryNotPresentValidator(ILogger<TicketQueryNotPresentValidator> logger) : ITokenRequestValidator<CdaTokenRequestModel>
{
    public int Order => 9;

    public string GrantType => TokenQueryParams.UmaGrantType;
    
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