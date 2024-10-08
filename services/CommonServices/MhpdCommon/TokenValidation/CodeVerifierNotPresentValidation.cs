using MhpdCommon.Models.MessageBodyModels;
using Microsoft.Extensions.Logging;

namespace MhpdCommon.TokenValidation;

public class CodeVerifierNotPresentValidation(ILogger<CodeVerifierNotPresentValidation> logger) : ITokenRequestValidator<CdaTokenRequestModel>
{
    public int Order => 8;
    public string GrantType => TokenQueryParams.AuthorizationCodeGrantType;
    
    public ValidationResult Validate(CdaTokenRequestModel request)
    {
        if (string.IsNullOrEmpty(request.CodeVerifier))
        {
            logger.LogError(TokenValidationMessages.CodeVerifierNotPresent);
            return ValidationResult.Failure(TokenValidationMessages.InvalidRequest);
        }

        return ValidationResult.Success();
    }
}