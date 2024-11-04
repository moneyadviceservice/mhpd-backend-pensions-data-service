using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.Utils;
using Microsoft.Extensions.Logging;

namespace MhpdCommon.TokenValidation;

public class RedirectUrlNotValidUrlValidation(ILogger<RedirectUrlNotValidUrlValidation> logger) : ITokenRequestValidator<CdaTokenRequestModel>
{
    public int Order => 11;
    public string GrantType => TokenQueryParams.AuthorizationCodeGrantType;
    
    public ValidationResult Validate(CdaTokenRequestModel request)
    {
        if (request.RedirectUrl != null && !TokenUtility.IsValidUrl(request.RedirectUrl))
        {
            logger.LogError(TokenValidationMessages.RedirectUriNotValidFormat);
            return ValidationResult.Failure(TokenValidationMessages.InvalidRedirectUri);
        }

        return ValidationResult.Success();
    }
}