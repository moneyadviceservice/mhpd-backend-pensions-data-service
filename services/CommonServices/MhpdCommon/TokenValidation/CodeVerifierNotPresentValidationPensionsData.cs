using MhpdCommon.Models.MessageBodyModels;
using Microsoft.Extensions.Logging;

namespace MhpdCommon.TokenValidation;

public class CodeVerifierNotPresentValidationPensionsData(ILogger<CodeVerifierNotPresentValidationPensionsData> logger) : ITokenRequestValidator<PensionsDataRequestModel>
{
    public int Order => 5;
    public string GrantType => string.Empty;
    
    public ValidationResult Validate(PensionsDataRequestModel request)
    {
        if (string.IsNullOrEmpty(request.CodeVerifier))
        {
            logger.LogError(TokenValidationMessages.CodeVerifierNotPresent);
            return ValidationResult.Failure(TokenValidationMessages.CodeVerifierNotPresent);
        }

        return ValidationResult.Success();
    }
}