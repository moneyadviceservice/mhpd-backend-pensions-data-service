using System.Text.RegularExpressions;
using MhpdCommon.Models.MessageBodyModels;
using Microsoft.Extensions.Logging;

namespace MhpdCommon.TokenValidation;

public partial class CodeVerifierNotBase64String(ILogger<CodeVerifierNotBase64String> logger) : ITokenRequestValidator<CdaTokenRequestModel>
{
    // Regular expression pattern for valid code_verifier
    // Use the GeneratedRegexAttribute to compile the regex pattern at compile-time
    [GeneratedRegex("^[a-zA-Z0-9\\-\\.\\\\_\\~]{43,128}$")]
    private static partial Regex CodeVerifierPattern();
    
    public int Order => 9;
    public string GrantType => TokenQueryParams.AuthorizationCodeGrantType;
    
    public ValidationResult Validate(CdaTokenRequestModel request)
    {
        if (request.CodeVerifier != null && !IsValidCodeVerifier(request.CodeVerifier))
        {
            logger.LogError(TokenValidationMessages.InvalidCodeVerifierFormat);
            return ValidationResult.Failure(TokenValidationMessages.InvalidCodeVerifier);
        }

        return ValidationResult.Success();
    }

    /// <summary>
    /// Validates that the input code_verifier is between 43 and 128 characters long and matches the required pattern.
    /// </summary>
    /// <param name="codeVerifier">The code verifier to validate.</param>
    /// <returns>True if the code_verifier is valid; otherwise, false.</returns>
    public static bool IsValidCodeVerifier(string codeVerifier)
    {
        if (string.IsNullOrEmpty(codeVerifier))
        {
            return false;
        }

        // Check if the code verifier matches the length requirements and regex pattern
        return CodeVerifierPattern().IsMatch(codeVerifier);
    }
}