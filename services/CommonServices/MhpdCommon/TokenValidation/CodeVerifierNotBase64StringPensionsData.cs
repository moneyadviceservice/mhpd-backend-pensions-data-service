using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.Utils;
using Microsoft.Extensions.Logging;

namespace MhpdCommon.TokenValidation;

public class CodeVerifierNotBase64StringPensionsData(ILogger<CodeVerifierNotBase64StringPensionsData> logger) : ITokenRequestValidator<PensionsDataRequestModel>
{
    public int Order => 6;
    public string GrantType => string.Empty;
    
    public ValidationResult Validate(PensionsDataRequestModel request)
    {
        if (request.CodeVerifier != null && !TokenUtility.IsValidCodeVerifier(request.CodeVerifier))
        {
            logger.LogError(TokenValidationMessages.InvalidCodeVerifierFormat);
            return ValidationResult.Failure(TokenValidationMessages.InvalidCodeVerifier);
        }

        return ValidationResult.Success();
    }
}