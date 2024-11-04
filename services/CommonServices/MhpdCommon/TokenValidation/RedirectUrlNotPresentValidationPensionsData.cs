using MhpdCommon.Models.MessageBodyModels;
using Microsoft.Extensions.Logging;

namespace MhpdCommon.TokenValidation;

public class RedirectUrlNotPresentValidationPensionsData(ILogger<RedirectUrlNotPresentValidationPensionsData> logger) : ITokenRequestValidator<PensionsDataRequestModel>
{
    public int Order => 3;
    public string GrantType => string.Empty;
    
    public ValidationResult Validate(PensionsDataRequestModel request)
    {
        if (string.IsNullOrEmpty(request.RedirectUrl))
        {
            logger.LogError(TokenValidationMessages.RedirectUriNotPresent);
            return ValidationResult.Failure(TokenValidationMessages.RedirectUriNotPresent);
        }

        return ValidationResult.Success();
    }
}