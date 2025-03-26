using MhpdCommon.Models.MessageBodyModels;
using Microsoft.Extensions.Logging;

namespace MhpdCommon.TokenValidation;

public class ClientIdNotPresentValidationRpts(ILogger<ClientIdNotPresentValidationRpts> logger) : ITokenRequestValidator<TokenClientRequestModel>
{
    public int Order => 7;
    public string GrantType => TokenQueryParams.AuthorizationCodeGrantType;
    
    public ValidationResult Validate(TokenClientRequestModel request)
    {
        if (string.IsNullOrEmpty(request.ClientId))
        {
            logger.LogError(TokenValidationMessages.ClientIdNotPresent);
            return ValidationResult.Failure(TokenValidationMessages.InvalidRequest);
        }

        return ValidationResult.Success();
    }
}