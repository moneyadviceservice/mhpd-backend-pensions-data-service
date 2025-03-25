using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace MhpdCommon.TokenValidation;

public class AsUriNotAUrlValidator(ILogger<AsUriNotAUrlValidator> logger) : ITokenRequestValidator<TokenClientRequestModel>
{
    public int Order => 6;

    public string GrantType => string.Empty;
    
    public ValidationResult Validate(TokenClientRequestModel request)
    {
        if (request.AsUri != null && !TokenUtility.IsValidUrl(request.AsUri))
        {
            logger.LogError(TokenValidationMessages.AsUriNotValidFormat);
            return ValidationResult.Failure(TokenValidationMessages.InvalidAsUri);
        }
        
        return ValidationResult.Success();
    }
}