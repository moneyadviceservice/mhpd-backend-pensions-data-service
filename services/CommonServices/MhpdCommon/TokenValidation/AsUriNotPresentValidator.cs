using MhpdCommon.Models.MessageBodyModels;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace MhpdCommon.TokenValidation;

public class AsUriNotPresentValidator(ILogger<AsUriNotPresentValidator> logger) : ITokenRequestValidator<TokenClientRequestModel>
{
    public int Order => 5;

    public string GrantType => string.Empty;
    
    public ValidationResult Validate(TokenClientRequestModel request)
    {
        if (string.IsNullOrEmpty(request.AsUri))
        {
            logger.LogError(TokenValidationMessages.AsUriNotPresent);
            return ValidationResult.Failure(TokenValidationMessages.InvalidAsUri);
        }

        return ValidationResult.Success();
    }
}