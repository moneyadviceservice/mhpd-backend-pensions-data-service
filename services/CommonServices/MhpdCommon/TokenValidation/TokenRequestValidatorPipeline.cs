using MhpdCommon.Models.MessageBodyModels;

namespace MhpdCommon.TokenValidation;

public class TokenRequestValidatorPipeline : IRequestValidator<CdaTokenRequestModel>
{
    private readonly IEnumerable<ITokenRequestValidator<CdaTokenRequestModel>> _validators;

    public TokenRequestValidatorPipeline(IEnumerable<ITokenRequestValidator<CdaTokenRequestModel>> validators)
    {
        _validators = validators.OrderBy(v => v.Order);
    }
    
    public ValidationResult Validate(CdaTokenRequestModel request)
    {
        // Check for grant_type presence and supported grant_type
        var result = RunValidation(request, string.Empty);
        if (!result.IsValid)
        {
            return result;
        }

        // Now load validators by grant_type & ordered
        return RunValidation(request, request.GrantType);
    }

    public ValidationResult RunValidation(CdaTokenRequestModel request, string? grantType)
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
