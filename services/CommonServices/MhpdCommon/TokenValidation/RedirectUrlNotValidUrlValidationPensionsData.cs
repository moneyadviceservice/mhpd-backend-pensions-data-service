using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.Utils;
using Microsoft.Extensions.Logging;

namespace MhpdCommon.TokenValidation;

public class RedirectUrlNotValidUrlValidationPensionsData(ILogger<RedirectUrlNotValidUrlValidationPensionsData> logger) : ITokenRequestValidator<PensionsDataRequestModel>
{
    public int Order => 4;
    public string GrantType => string.Empty;
    
    public ValidationResult Validate(PensionsDataRequestModel request)
    {
        if (request.RedirectUrl != null && !TokenUtility.IsValidUrl(request.RedirectUrl))
        {
            logger.LogError(TokenValidationMessages.RedirectUriNotValidFormat);
            return ValidationResult.Failure(TokenValidationMessages.InvalidRedirectUri);
        }

        return ValidationResult.Success();
    }
}