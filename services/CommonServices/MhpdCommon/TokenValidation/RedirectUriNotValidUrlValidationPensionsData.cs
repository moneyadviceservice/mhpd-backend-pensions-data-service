using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.Utils;
using Microsoft.Extensions.Logging;

namespace MhpdCommon.TokenValidation;

public class RedirectUriNotValidUrlValidationPensionsData(ILogger<RedirectUriNotValidUrlValidationPensionsData> logger) : ITokenRequestValidator<PensionsDataRequestModel>
{
    public int Order => 4;
    public string GrantType => string.Empty;
    
    public ValidationResult Validate(PensionsDataRequestModel request)
    {
        if (request.RedirectUri != null && !TokenUtility.IsValidUrl(request.RedirectUri))
        {
            logger.LogError(TokenValidationMessages.RedirectUriNotValidFormat);
            return ValidationResult.Failure(TokenValidationMessages.InvalidRedirectUri);
        }

        return ValidationResult.Success();
    }
}