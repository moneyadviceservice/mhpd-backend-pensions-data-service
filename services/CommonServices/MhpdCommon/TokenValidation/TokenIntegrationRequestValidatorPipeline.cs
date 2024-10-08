using MhpdCommon.Models.MessageBodyModels;

namespace MhpdCommon.TokenValidation;

public class TokenIntegrationRequestValidatorPipeline : IRequestValidator<TokenIntegrationRequestModel>
{
    private readonly IEnumerable<ITokenRequestValidator<TokenIntegrationRequestModel>> _validators;

    public TokenIntegrationRequestValidatorPipeline(IEnumerable<ITokenRequestValidator<TokenIntegrationRequestModel>> validators)
    {
        _validators = validators.OrderBy(v => v.Order);
    }

    public ValidationResult Validate(TokenIntegrationRequestModel request)
    {
        if (!_validators.Any())
        {
            throw new InvalidOperationException("No validators found.");
        }
        
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
