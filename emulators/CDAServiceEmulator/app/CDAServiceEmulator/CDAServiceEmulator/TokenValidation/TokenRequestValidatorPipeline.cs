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
        foreach (var validator in _validators)
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
