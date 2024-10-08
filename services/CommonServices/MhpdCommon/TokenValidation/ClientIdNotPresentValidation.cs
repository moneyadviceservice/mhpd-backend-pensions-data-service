using MhpdCommon.Models.MessageBodyModels;
using Microsoft.Extensions.Logging;

namespace MhpdCommon.TokenValidation;

public class ClientIdNotPresentValidation(ILogger<ClientIdNotPresentValidation> logger) : ITokenRequestValidator<CdaTokenRequestModel>
{
    public int Order => 2;
    public string GrantType => TokenQueryParams.AuthorizationCodeGrantType;
    
    public ValidationResult Validate(CdaTokenRequestModel request)
    {
        if (string.IsNullOrEmpty(request.ClientId))
        {
            logger.LogError(TokenValidationMessages.ClientIdNotPresent);
            return ValidationResult.Failure(TokenValidationMessages.InvalidRequest);
        }

        return ValidationResult.Success();
    }
}