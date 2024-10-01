using CDAServiceEmulator.Models.Token;

namespace CDAServiceEmulator.TokenValidation;

public class RedirectUriNotValidUrlValidation(ILogger<RedirectUriNotValidUrlValidation> logger) : ITokenRequestValidator
{
    public int Order => 11;
    public string GrantType => TokenQueryParams.AuthorizationCodeGrantType;
    
    public ValidationResult Validate(CdaTokenRequestModel request)
    {
        if (request.RedirectUri != null && !IsValidUrl(request.RedirectUri))
        {
            logger.LogError(TokenValidationMessages.RedirectUriNotValidFormat);
            return ValidationResult.Failure(TokenValidationMessages.InvalidRedirectUri);
        }

        return ValidationResult.Success();
    }
    
    public static bool IsValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult) 
               && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}