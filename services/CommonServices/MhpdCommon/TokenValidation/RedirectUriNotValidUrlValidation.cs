using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.Utils;
using Microsoft.Extensions.Logging;

namespace MhpdCommon.TokenValidation;

public class RedirectUriNotValidUrlValidation(ILogger<RedirectUriNotValidUrlValidation> logger) : ITokenRequestValidator<CdaTokenRequestModel>
{
    public int Order => 11;
    public string GrantType => TokenQueryParams.AuthorizationCodeGrantType;
    
    public ValidationResult Validate(CdaTokenRequestModel request)
    {
        if (request.RedirectUri != null && !TokenUtility.IsValidUrl(request.RedirectUri))
        {
            logger.LogError(TokenValidationMessages.RedirectUriNotValidFormat);
            return ValidationResult.Failure(TokenValidationMessages.InvalidRedirectUri);
        }

        return ValidationResult.Success();
    }
}