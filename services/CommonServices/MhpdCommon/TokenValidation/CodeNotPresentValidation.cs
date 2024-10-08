using MhpdCommon.Models.MessageBodyModels;
using Microsoft.Extensions.Logging;

namespace MhpdCommon.TokenValidation;

public class CodeNotPresentValidation(ILogger<CodeNotPresentValidation> logger) : ITokenRequestValidator<CdaTokenRequestModel>
{
    public int Order => 6;
    public string GrantType => TokenQueryParams.AuthorizationCodeGrantType;
    
    public ValidationResult Validate(CdaTokenRequestModel request)
    {
        if (string.IsNullOrEmpty(request.Code))
        {
            logger.LogError(TokenValidationMessages.CodeNotPresent);
            return ValidationResult.Failure(TokenValidationMessages.InvalidRequest);
        }

        return ValidationResult.Success();
    }
}