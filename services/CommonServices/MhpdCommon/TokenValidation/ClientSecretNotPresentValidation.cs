using MhpdCommon.Models.MessageBodyModels;
using Microsoft.Extensions.Logging;

namespace MhpdCommon.TokenValidation;

public class ClientSecretNotPresentValidation(ILogger<ClientSecretNotPresentValidation> logger) : ITokenRequestValidator<CdaTokenRequestModel>
{
    public int Order => 4;
    public string GrantType => TokenQueryParams.AuthorizationCodeGrantType;
    
    public ValidationResult Validate(CdaTokenRequestModel request)
    {
        if (string.IsNullOrEmpty(request.ClientSecret))
        {
            logger.LogError(TokenValidationMessages.ClientSecretNotPresent);
            return ValidationResult.Failure(TokenValidationMessages.InvalidRequest);
        }

        return ValidationResult.Success();
    }
}