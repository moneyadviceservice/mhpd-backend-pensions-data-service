using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.Utils;
using Microsoft.Extensions.Logging;

namespace MhpdCommon.TokenValidation;

public class CodeInvalidFormatValidation(ILogger<CodeInvalidFormatValidation> logger) : ITokenRequestValidator<CdaTokenRequestModel>
{
    public int Order => 7;
    public string GrantType => TokenQueryParams.AuthorizationCodeGrantType;
    
    public ValidationResult Validate(CdaTokenRequestModel request)
    {
        if (request.Code != null && !IdValidator.IsValidString(request.Code))
        {
            logger.LogError(TokenValidationMessages.CodeInvalidFormat);
            return ValidationResult.Failure(TokenValidationMessages.InvalidCodeFormat);
        }

        return ValidationResult.Success();
    }
}