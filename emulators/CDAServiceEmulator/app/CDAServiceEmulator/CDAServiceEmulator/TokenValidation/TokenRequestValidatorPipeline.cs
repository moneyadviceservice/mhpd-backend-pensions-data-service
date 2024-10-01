using CDAServiceEmulator.Models.Token;

namespace CDAServiceEmulator.TokenValidation;

public class TokenRequestValidatorPipeline
{
    private readonly IEnumerable<ITokenRequestValidator> _validators;

    public TokenRequestValidatorPipeline(IEnumerable<ITokenRequestValidator> validators)
    {
        _validators = validators.OrderBy(v => v.Order);
    }
    
    public ValidationResult Validate(CdaTokenRequestModel request)
    {
        // Check for grant_type presence and supported grant_type
        var result = RunValidation(string.Empty, request);
        if (!result.IsValid)
        {
            return result;
        }

        // Now load validators by grant_type & ordered
        return RunValidation(request.GrantType, request);
    }

    private ValidationResult RunValidation(string? grantType, CdaTokenRequestModel request)
    {
        // Now load validators by grant_type & ordered
        foreach (var validator in _validators
                     .Where(v => v.GrantType == grantType)
                     .OrderBy(o => o.Order))
        {
            var result = validator.Validate(request);
            if (!result.IsValid)
            {
                return result;
            }
        }

        return ValidationResult.Success();
    }
}
