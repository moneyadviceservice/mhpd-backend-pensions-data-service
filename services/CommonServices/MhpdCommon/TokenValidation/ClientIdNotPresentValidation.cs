using MhpdCommon.Models.MessageBodyModels;
using Microsoft.Extensions.Logging;

namespace MhpdCommon.TokenValidation;

public class ClientIdNotPresentValidation(ILogger<ClientIdNotPresentValidation> logger) : ITokenRequestValidator<CdaTokenRequestModel>
{
    public int Order => 2;
    public string GrantType => string.Empty;
    
    public ValidationResult Validate(CdaTokenRequestModel request)
    {
        if (string.IsNullOrWhiteSpace(request.ClientId))
        {
            logger.LogError(TokenValidationMessages.ClientIdNotPresent);
            return ValidationResult.Failure(TokenValidationMessages.ClientIdNotPresent);
        }

        return ValidationResult.Success();
    }
}