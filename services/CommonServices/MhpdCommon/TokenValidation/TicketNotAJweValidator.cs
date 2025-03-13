using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.Utils;
using Microsoft.Extensions.Logging;

namespace MhpdCommon.TokenValidation;

public class TicketNotAJweValidator(ILogger<TicketNotAJweValidator> logger) : ITokenRequestValidator<CdaTokenRequestModel>
{
    public int Order => 10;
    
    public string GrantType => TokenQueryParams.UmaGrantType;

    public ValidationResult Validate(CdaTokenRequestModel request)
    {
        if (request is { Ticket: not null, GrantType: TokenQueryParams.UmaGrantType } && !JweValidator.IsJweFormatValid(request.Ticket))
        {
            logger.LogError(TokenValidationMessages.TicketNotAJwe);
            return ValidationResult.Failure(TokenValidationMessages.InvalidJweTicketQueryFormat);
        }

        return ValidationResult.Success();
    }
}