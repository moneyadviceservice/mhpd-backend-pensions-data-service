using MhpdCommon.Models.MessageBodyModels;
using Microsoft.Extensions.Logging;

namespace MhpdCommon.TokenValidation;

public class RedirectUriNotPresentValidation(ILogger<RedirectUriNotPresentValidation> logger) : ITokenRequestValidator<CdaTokenRequestModel>
{
    public int Order => 10;
    public string GrantType => TokenQueryParams.AuthorizationCodeGrantType;
    
    public ValidationResult Validate(CdaTokenRequestModel request)
    {
        if (string.IsNullOrEmpty(request.RedirectUri))
        {
            logger.LogError(TokenValidationMessages.RedirectUriNotPresent);
            return ValidationResult.Failure(TokenValidationMessages.InvalidRequest);
        }

        return ValidationResult.Success();
    }
}