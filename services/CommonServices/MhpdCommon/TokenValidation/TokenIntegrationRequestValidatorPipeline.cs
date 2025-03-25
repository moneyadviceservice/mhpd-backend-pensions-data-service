using MhpdCommon.Models.MessageBodyModels;

namespace MhpdCommon.TokenValidation;

public class TokenIntegrationRequestValidatorPipeline : IRequestValidator<TokenClientRequestModel>
{
    private readonly IEnumerable<ITokenRequestValidator<TokenClientRequestModel>> _validators;

    public TokenIntegrationRequestValidatorPipeline(IEnumerable<ITokenRequestValidator<TokenClientRequestModel>> validators)
    {
        _validators = validators.OrderBy(v => v.Order);
    }

    public ValidationResult Validate(TokenClientRequestModel request)
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
